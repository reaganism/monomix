using Reaganism.Recon;

namespace Reaganism.MonoMix.Cil;

public class ILInstructionCursor : ICursor<ILInstruction> {
    public IElementWindow<ILInstruction> Window { get; } = new ElementWindow<ILInstruction>();
}
