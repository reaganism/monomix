using System.Collections.Generic;
using Reaganism.Recon;

namespace Reaganism.MonoMix.Cil;

public class ILInstructionCursor(IList<ILInstruction> elements) : Cursor<ILInstruction>(elements) { }
