using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Reaganism.MonoMix;

/// <summary>
///     An advanced wrapper over <see cref="ILCursor"/>.
/// </summary>
public sealed class ILMixin(ILCursor cursor) {
    private ILCursor Cursor { get; } = cursor;

    public bool TryGotoNextPattern(MoveType moveType, ILPattern pattern) {
        var instrs = Cursor.Instrs;
        var i = Cursor.Index;
        if (Cursor.SearchTarget == SearchTarget.Next)
            i++;

        var ilProvider = IILProvider.FromILCursor(Cursor);

        for (; i + pattern.MinimumLength <= instrs.Count; i++) {
            ilProvider.Instruction = instrs[i];
            if (ILPattern.Match(ilProvider, pattern) is not { } matchedInstruction)
                continue;

            Cursor.Goto(matchedInstruction, moveType, true);
            return true;
        }

        return false;
    }

    public bool TryGotoLastPattern(MoveType moveType, ILPattern pattern) { }

    public ILMixin GotoNextPattern(MoveType moveType, ILPattern pattern) {
        if (!TryGotoNextPattern(moveType, pattern))
            throw new KeyNotFoundException();

        return this;
    }

    public void GotoLastPattern(MoveType moveType, ILPattern pattern) { }

    public static implicit operator ILCursor(ILMixin ilMixin) {
        return ilMixin.Cursor;
    }

    public static implicit operator ILMixin(ILCursor ilCursor) {
        return new ILMixin(ilCursor);
    }
}
