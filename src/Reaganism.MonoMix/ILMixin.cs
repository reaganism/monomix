using System;
using System.Collections.Generic;
using MonoMod.Cil;

namespace Reaganism.MonoMix;

/// <summary>
///     An advanced wrapper over <see cref="ILCursor"/>.
/// </summary>
public sealed class ILMixin(ILCursor cursor) {
    private ILCursor Cursor { get; } = cursor;

    #region GotoXPattern
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

    public bool TryGotoPrevPattern(MoveType moveType, ILPattern pattern) {
        var instrs = Cursor.Instrs;
        var i = Cursor.Index - 1;
        if (Cursor.SearchTarget == SearchTarget.Prev)
            i--;

        i = Math.Min(i, instrs.Count - pattern.MinimumLength);

        var ilProvider = IILProvider.FromILCursor(Cursor);

        for (; i >= 0; i--) {
            ilProvider.Instruction = instrs[i];
            if (ILPattern.Match(ilProvider, pattern, ILPattern.Direction.Backward) is not { } matchedInstruction)
                continue;

            Cursor.Goto(matchedInstruction, moveType, true);
            return true;
        }

        return false;
    }

    public ILMixin GotoNextPattern(MoveType moveType, ILPattern pattern) {
        if (!TryGotoNextPattern(moveType, pattern))
            throw new KeyNotFoundException();

        return this;
    }

    public ILMixin GotoPrevPattern(MoveType moveType, ILPattern pattern) {
        if (!TryGotoPrevPattern(moveType, pattern))
            throw new KeyNotFoundException();

        return this;
    }
    #endregion

    public static implicit operator ILCursor(ILMixin ilMixin) {
        return ilMixin.Cursor;
    }

    public static implicit operator ILMixin(ILCursor ilCursor) {
        return new ILMixin(ilCursor);
    }
}
