using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;

namespace Reaganism.MonoMix;

/// <summary>
///     An IL pattern, which is an abstract object that may match against IL
///     instructions.
/// </summary>
public abstract class ILPattern {
    private sealed class OptionalILPattern(ILPattern pattern) : ILPattern {
        public override int MinimumLength => 0;

        protected override bool Match(IILProvider ilProvider) {
            pattern.TryMatch(ilProvider);
            return true;
        }
    }

    private sealed class SequenceILPattern(IEnumerable<ILPattern> patterns) : ILPattern {
        public override int MinimumLength => patterns.Sum(pattern => pattern.MinimumLength);

        protected override bool Match(IILProvider ilProvider) {
            foreach (var pattern in patterns) {
                if (!pattern.Match(ilProvider))
                    return false;
            }

            return true;
        }
    }

    private sealed class EitherILPattern(ILPattern either, ILPattern or) : ILPattern {
        public override int MinimumLength => Math.Min(either.MinimumLength, or.MinimumLength);

        protected override bool Match(IILProvider ilProvider) {
            return either.TryMatch(ilProvider) || or.Match(ilProvider);
        }
    }

    private sealed class OpCodeILPattern(OpCode opCode) : ILPattern {
        public override int MinimumLength => 1;

        protected override bool Match(IILProvider ilProvider) {
            if (ilProvider.Instruction is null)
                return false;

            var success = ilProvider.Instruction.OpCode == opCode;
            ilProvider.TryGotoNext();
            return success;
        }
    }

    public abstract int MinimumLength { get; }

    /// <summary>
    ///     Matches an arbitrary condition given a set of instructions.
    /// </summary>
    /// <param name="ilProvider">Provides the set of instructions.</param>
    /// <returns>Whether the match was successful.</returns>
    /// <remarks>
    ///     While <see cref="TryMatch"/> also optionally performs a match,
    ///     <see cref="Match"/> on its own will leave the position of the
    ///     <paramref name="ilProvider"/> modified.
    /// </remarks>
    protected abstract bool Match(IILProvider ilProvider);

    /// <summary>
    ///     Attempts to match and resets to the starting position if the match
    ///     fails.
    /// </summary>
    /// <param name="ilProvider">Provides the set of instructions.</param>
    /// <returns>Whether the match was successful.</returns>
    /// <remarks>
    ///     While <see cref="Match"/> also communicates whether the match was
    ///     successful, <see cref="TryMatch"/> explicitly resets the position
    ///     of the <paramref name="ilProvider"/> if the match fails.
    /// </remarks>
    protected bool TryMatch(IILProvider ilProvider) {
        var instruction = ilProvider.Instruction;
        if (Match(ilProvider))
            return true;

        ilProvider.Instruction = instruction;
        return false;
    }

    public static Instruction? Match(IILProvider ilProvider, ILPattern pattern) {
        return pattern.Match(ilProvider) ? ilProvider.Instruction : null;
    }

    public static ILPattern Optional(OpCode opCode) {
        return Optional(OpCode(opCode));
    }

    public static ILPattern Optional(params OpCode[] opCodes) {
        return Optional(Sequence(opCodes.Select(OpCode).ToArray()));
    }

    public static ILPattern Optional(ILPattern pattern) {
        return new OptionalILPattern(pattern);
    }

    public static ILPattern Sequence(params ILPattern[] patterns) {
        return new SequenceILPattern(patterns);
    }

    public static ILPattern Either(ILPattern either, ILPattern or) {
        return new EitherILPattern(either, or);
    }

    public static ILPattern OpCode(OpCode opCode) {
        return new OpCodeILPattern(opCode);
    }
}
