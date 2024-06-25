using Mono.Cecil.Cil;

namespace Reaganism.MonoMix.Pattern;

/// <summary>
///     An IL match result produced by
///     <see cref="ILPattern.Match(ILMatchContext, ILPattern)"/> and
///     <see cref="ILPattern.Match(Instruction, ILPattern.Direction, ILPattern)"/>.
/// </summary>
/// <param name="Successful">Whether the match was successful.</param>
/// <param name="Previous">
///     The previous instruction relative to the current instruction.
/// </param>
/// <param name="Current">
///     The current instruction at the end of the match.
/// </param>
/// <param name="Next">
///     The next instruction relative to the current instruction.
/// </param>
public readonly record struct ILMatchResult(bool Successful, Instruction? Previous, Instruction? Current, Instruction? Next);
