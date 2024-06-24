using MonoMod.Cil;

namespace Reaganism.MonoMix;

/// <summary>
///     An advanced wrapper over <see cref="ILCursor"/>.
/// </summary>
public sealed class ILMixin(ILCursor cursor) {
    public bool TryGotoNextPattern(MoveType moveType, ILPattern pattern) { }

    public bool TryGotoLastPattern(MoveType moveType, ILPattern pattern) { }

    public void GotoNextPattern(MoveType moveType, ILPattern pattern) {
        cursor.GotoNext()
    }

    public void GotoLastPattern(MoveType moveType, ILPattern pattern) { }
}
