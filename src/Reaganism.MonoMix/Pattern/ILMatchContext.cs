using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil.Cil;

namespace Reaganism.MonoMix.Pattern;

public sealed class ILMatchContext(Instruction? instruction, IILProvider ilProvider, ILPattern.Direction direction = ILPattern.Direction.Forward) : IDisposable {
    public Instruction? Instruction { get; set; } = instruction ?? ilProvider.First();

    public ILPattern.Direction Direction { get; } = direction;

    private readonly Dictionary<object, object> data = [];

    public bool TryGetData(object key, [NotNullWhen(returnValue: true)] out object? value) {
        return data.TryGetValue(key, out value);
    }

    public void AddData(object key, object value) {
        data[key] = value;
    }

    /// <summary>
    ///     Attempts to advance the current <see cref="Instruction"/> to the
    ///     next. Depending on the <see cref="ILPattern.Direction"/>, this
    ///     either advances forward (to the next instruction) or backward (to
    ///     the previous instruction).
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the operation was successful;
    ///     <see langword="false"/> if the operation failed.
    /// </returns>
    public bool TryAdvance() {
        if (Instruction is null)
            return false;

        if (Instruction.Next is null && direction == ILPattern.Direction.Forward)
            return false;

        if (Instruction.Previous is null && Direction == ILPattern.Direction.Backward)
            return false;

        Instruction = Direction == ILPattern.Direction.Forward ? Instruction?.Next : Instruction?.Previous;
        return true;
    }

    public void Dispose() {
        ilProvider.Dispose();
    }
}
