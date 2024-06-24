using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Reaganism.MonoMix;

/// <summary>
///     Minimally-contractual object that provides enumeration over
///     Mono.Cecil-compatible IL instructions.
/// </summary>
public interface IILProvider : IEnumerable<Instruction>, IDisposable {
    private sealed class ILProvider(Instruction? instruction, IEnumerable<Instruction> instructions, params IDisposable[] disposables) : IILProvider {
        public Instruction? Instruction { get; set; } = instruction;

        public bool TryGotoPrev() {
            if (Instruction?.Previous is null)
                return false;

            Instruction = Instruction.Previous;
            return true;
        }

        public bool TryGotoNext() {
            if (Instruction is null)
                return false;

            // We don't care if Next is null; if anything, we like that -- it
            // means we can easily denote the end of the instruction list.
            Instruction = Instruction.Next;
            return true;
        }

        public IEnumerator<Instruction> GetEnumerator() {
            // ReSharper disable once NotDisposedResourceIsReturned
            return instructions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Dispose() {
            foreach (var disposable in disposables)
                disposable.Dispose();
        }
    }

    /// <summary>
    ///     The current instruction.
    /// </summary>
    /// <remarks>
    ///     <see langword="null"/> when the instruction list has been advanced
    ///     to the end or when it is empty.
    /// </remarks>
    Instruction? Instruction { get; set; }

    /// <summary>
    ///     Attempts to set <see cref="Instruction"/> to the previous
    ///     instruction.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the operation was successful;
    ///     <see langword="false"/> if the operation failed.
    /// </returns>
    bool TryGotoPrev();

    /// <summary>
    ///     Attempts to set <see cref="Instruction"/> to the next instruction.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the operation was successful;
    ///     <see langword="false"/> if the operation failed.
    /// </returns>
    bool TryGotoNext();

    public static IILProvider FromMethodDefinition(MethodDefinition methodDefinition) {
        return FromMethodBody(methodDefinition.Body);
    }

    public static IILProvider FromMethodBody(MethodBody methodBody) {
        var instructions = methodBody.Instructions;
        return new ILProvider(methodBody.Instructions.First(), instructions);
    }

    public static IILProvider FromILContext(ILContext context) {
        return FromMethodBody(context.Body);
    }

    public static IILProvider FromILCursor(ILCursor cursor) {
        return FromMethodBody(cursor.Body);
    }

    public static IILProvider FromMethodBaseAsCecil(MethodBase methodBase) {
        var dynDef = new DynamicMethodDefinition(methodBase);
        return new ILProvider(dynDef.Definition.Body.Instructions.First(), dynDef.Definition.Body.Instructions, dynDef);
    }

    public static IILProvider FromMethodBaseAsSystem(MethodBase methodBase) {
        var instructions = MethodBodyDisassembler.FromMethodBase(methodBase);
        return new ILProvider(instructions.First(), instructions);
    }
}
