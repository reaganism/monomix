using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil.Cil;

namespace Reaganism.MonoMix;

internal static class MethodBodyDisassembler {
    private sealed class ThisParameterInfo : ParameterInfo {
        public ThisParameterInfo(MethodBase methodBase) {
            MemberImpl = methodBase;
            ClassImpl = methodBase.DeclaringType;
            NameImpl = "this";
            PositionImpl = -1;
        }
    }

    private sealed record DasmContext(
        MethodBase Method,
        Type[]? MethodArguments,
        Type[]? TypeArguments,
        ThisParameterInfo? ThisParameter,
        ParameterInfo[]? Parameters,
        IList<LocalVariableInfo>? Locals,
        Module Module,
        List<Instruction> Instructions
    );

    private static readonly OpCode[] op_codes_1 = new OpCode[0xe0 + 1];
    private static readonly OpCode[] op_codes_2 = new OpCode[0x1e + 1];

    static MethodBodyDisassembler() {
        foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)) {
            if (field.GetValue(null) is not OpCode opCode || opCode.OpCodeType == OpCodeType.Nternal)
                continue;

            if (opCode.Size == 1)
                op_codes_1[opCode.Value] = opCode;
            else
                op_codes_2[opCode.Value & 0xff] = opCode;
        }
    }

    public static List<Instruction> FromMethodBase(MethodBase methodBase) {
        var body = methodBase.GetMethodBody();
        if (body is null)
            throw new InvalidOperationException("Cannot disassemble method without body");

        var ilBytes = body.GetILAsByteArray();
        if (ilBytes is null)
            throw new InvalidOperationException("Cannot disassemble method without IL bytes");

        var ctx = new DasmContext(
            methodBase,
            methodBase is ConstructorInfo ? null : methodBase.GetGenericArguments(),
            methodBase.DeclaringType?.GetGenericArguments(),
            methodBase.IsStatic ? null : new ThisParameterInfo(methodBase),
            methodBase.GetParameters(),
            body.LocalVariables,
            methodBase.Module,
            new List<Instruction>((ilBytes.Length + 1) / 2)
        );

        // Read instructions.
        {
            using var reader = new BinaryReader(new MemoryStream(ilBytes));

            for (Instruction? currInstr, prevInstr = null; reader.BaseStream.Position < reader.BaseStream.Length; prevInstr = currInstr) {
                currInstr = Instruction.Create(OpCodes.Nop);
                currInstr.Offset = (int)reader.BaseStream.Position;

                var op = reader.ReadByte();
                currInstr.OpCode = op != 0xfe ? op_codes_1[op] : op_codes_2[reader.ReadByte()];

                ReadOperand(reader, currInstr, ctx);

                if (prevInstr is not null) {
                    currInstr.Previous = prevInstr;
                    prevInstr.Next = currInstr;
                }

                ctx.Instructions.Add(currInstr);
            }
        }

        // Resolve branches.
        {
            foreach (var instr in ctx.Instructions) {
                switch (instr.OpCode.OperandType) {
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.InlineBrTarget:
                        instr.Operand = GetInstruction((int)instr.Operand, ctx);
                        break;

                    case OperandType.InlineSwitch:
                        var offsets = (int[])instr.Operand;
                        var branches = new Instruction?[offsets.Length];
                        for (var i = 0; i < offsets.Length; i++)
                            branches[i] = GetInstruction(offsets[i], ctx);

                        instr.Operand = branches;
                        break;
                }
            }
        }

        return ctx.Instructions;
    }

    private static void ReadOperand(BinaryReader reader, Instruction instruction, DasmContext ctx) {
        switch (instruction.OpCode.OperandType) {
            case OperandType.InlineNone:
                break;

            case OperandType.ShortInlineBrTarget:
                instruction.Operand = reader.ReadSByte() + instruction.Offset;
                break;

            case OperandType.InlineBrTarget:
                instruction.Operand = reader.ReadInt32() + instruction.Offset;
                break;

            case OperandType.InlineSwitch:
                var len = reader.ReadInt32();
                var off = (int)(reader.BaseStream.Position + 4 * len);
                var branches = new int[len];
                for (var i = 0; i < len; i++)
                    branches[i] = reader.ReadInt32() + off;

                instruction.Operand = branches;
                break;

            case OperandType.InlineTok:
            case OperandType.InlineType:
            case OperandType.InlineMethod:
            case OperandType.InlineField:
                instruction.Operand = ctx.Module.ResolveMember(reader.ReadInt32(), ctx.TypeArguments, ctx.MethodArguments);
                break;

            case OperandType.ShortInlineI:
                instruction.Operand = instruction.OpCode == OpCodes.Ldc_I4_S ? reader.ReadSByte() : reader.ReadByte();
                break;

            case OperandType.InlineI:
                instruction.Operand = reader.ReadInt32();
                break;

            case OperandType.InlineI8:
                instruction.Operand = reader.ReadInt64();
                break;

            case OperandType.ShortInlineR:
                instruction.Operand = reader.ReadSingle();
                break;

            case OperandType.InlineR:
                instruction.Operand = reader.ReadDouble();
                break;

            case OperandType.InlineSig:
                instruction.Operand = ctx.Module.ResolveSignature(reader.ReadInt32());
                break;

            case OperandType.InlineString:
                instruction.Operand = ctx.Module.ResolveString(reader.ReadInt32());
                break;

            case OperandType.ShortInlineVar:
                instruction.Operand = GetLocalVariable(reader.ReadByte(), ctx);
                break;

            case OperandType.InlineVar:
                instruction.Operand = GetLocalVariable(reader.ReadInt16(), ctx);
                break;

            case OperandType.ShortInlineArg:
                instruction.Operand = GetParameter(reader.ReadByte(), ctx);
                break;

            case OperandType.InlineArg:
                instruction.Operand = GetParameter(reader.ReadInt16(), ctx);
                break;

            case OperandType.InlinePhi:
            default:
                throw new NotSupportedException($"Attempted to read operand of an unsupported opcode: {instruction.OpCode.Name}");
        }
    }

    private static LocalVariableInfo GetLocalVariable(int index, DasmContext ctx) {
        if (ctx.Locals is null)
            throw new InvalidOperationException("Cannot get local variable from null locals");

        return ctx.Locals[index];
    }

    private static ParameterInfo GetParameter(int index, DasmContext ctx) {
        if (ctx.Parameters is null)
            throw new InvalidOperationException("Cannot get parameter from null parameters");

        if (ctx.Method.IsStatic)
            return ctx.Parameters[index];

        return index == 0 ? ctx.ThisParameter! : ctx.Parameters[index - 1];
    }

    private static Instruction? GetInstruction(int off, DasmContext ctx) {
        var len = ctx.Instructions.Count;
        if (off < 0 || off > ctx.Instructions[len - 1].Offset)
            return null;

        var min = 0;
        var max = len - 1;
        while (min <= max) {
            var mid = min + (max - min) / 2;
            var instr = ctx.Instructions[mid];
            var instrOff = instr.Offset;

            if (instrOff == off)
                return instr;

            if (instrOff < off)
                min = mid + 1;
            else
                max = mid - 1;
        }

        return null;
    }
}
