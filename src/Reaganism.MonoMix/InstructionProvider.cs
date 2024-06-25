using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Reaganism.MonoMix;

/// <summary>
///     Utilities for retrieving collections of instructions from common
///     objects.
/// </summary>
public static class InstructionProvider {
    public static IEnumerable<Instruction> FromMethodDefinition(MethodDefinition methodDefinition) {
        return FromMethodBody(methodDefinition.Body);
    }

    public static IEnumerable<Instruction> FromILContext(ILContext context) {
        return FromMethodBody(context.Body);
    }

    public static IEnumerable<Instruction> FromILCursor(ILCursor cursor) {
        return FromMethodBody(cursor.Body);
    }

    public static IEnumerable<Instruction> FromMethodBody(MethodBody methodBody) {
        return methodBody.Instructions;
    }

    public static IEnumerable<Instruction> FromMethodBaseAsCecil(MethodBase methodBase, out IDisposable disposable) {
        var dynDef = new DynamicMethodDefinition(methodBase);
        disposable = dynDef;
        return dynDef.Definition.Body.Instructions;
    }

    public static IEnumerable<Instruction> FromMethodBaseAsSystem(MethodBase methodBase) {
        return MethodBodyDisassembler.FromMethodBase(methodBase);
    }
}
