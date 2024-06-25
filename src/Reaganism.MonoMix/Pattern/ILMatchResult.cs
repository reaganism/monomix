using Mono.Cecil.Cil;

namespace Reaganism.MonoMix.Pattern;

public readonly record struct ILMatchResult(bool Successful, Instruction? Previous, Instruction? Current, Instruction? Next);
