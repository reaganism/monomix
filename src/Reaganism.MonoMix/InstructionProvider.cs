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
    /// <summary>
    ///     Gets the instructions of a <see cref="MethodDefinition"/>.
    /// </summary>
    public static IEnumerable<Instruction> FromMethodDefinition(MethodDefinition methodDefinition) {
        return FromMethodBody(methodDefinition.Body);
    }

    /// <summary>
    ///     Gets the instructions of an <see cref="ILContext"/>.
    /// </summary>
    public static IEnumerable<Instruction> FromILContext(ILContext context) {
        return FromMethodBody(context.Body);
    }

    /// <summary>
    ///     Gets the instructions of an <see cref="ILCursor"/>.
    /// </summary>
    public static IEnumerable<Instruction> FromILCursor(ILCursor cursor) {
        return FromMethodBody(cursor.Body);
    }

    /// <summary>
    ///     Gets the instructions of a <see cref="MethodBody"/>.
    /// </summary>
    public static IEnumerable<Instruction> FromMethodBody(MethodBody methodBody) {
        return methodBody.Instructions;
    }

    /// <summary>
    ///     Gets the instructions of a <see cref="MethodBase"/> with operands
    ///     mapped to Mono.Cecil types.
    /// </summary>
    /// <remarks>
    ///     This method uses a <see cref="DynamicMethodDefinition"/> to handle
    ///     disassembling the method, meaning one has to manually dispose of
    ///     the instance (<paramref name="disposable"/>) when appropriate.
    /// </remarks>
    public static IEnumerable<Instruction> FromMethodBaseAsCecil(MethodBase methodBase, out IDisposable disposable) {
        var dynDef = new DynamicMethodDefinition(methodBase);
        disposable = dynDef;
        return dynDef.Definition.Body.Instructions;
    }

    /// <summary>
    ///     Gets the instructions of a <see cref="MethodBase"/> with operands
    ///     mapped to System types.
    /// </summary>
    public static IEnumerable<Instruction> FromMethodBaseAsSystem(MethodBase methodBase) {
        return MethodBodyDisassembler.FromMethodBase(methodBase);
    }
}
