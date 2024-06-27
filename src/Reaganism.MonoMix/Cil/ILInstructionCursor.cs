using System.Collections.Generic;
using Mono.Cecil.Cil;
using Reaganism.Recon;

namespace Reaganism.MonoMix.Cil;

public class ILInstructionCursor(IList<ILInstruction> elements) : Cursor<ILInstruction>(elements) { }

public class TemporaryILCursorForTesting(IList<Instruction> elements) : Cursor<Instruction>(elements) { }
