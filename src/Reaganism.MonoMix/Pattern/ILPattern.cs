using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;

namespace Reaganism.MonoMix.Pattern;

/// <summary>
///     An IL pattern, which is an abstract object that may match against IL
///     instructions.
/// </summary>
public abstract class ILPattern {
    /// <summary>
    ///     The match direction.
    /// </summary>
    public enum Direction {
        /// <summary>
        ///     Whether matching is done forward.
        /// </summary>
        Forward,

        /// <summary>
        ///     Whether matching is done backward.
        /// </summary>
        Backward,
    }

    private sealed class OptionalILPattern(ILPattern pattern) : ILPattern {
        public override int MinimumLength => 0;

        public override bool Match(ILMatchContext ctx) {
            pattern.TryMatch(ctx);
            return true;
        }
    }

    private sealed class SequenceILPattern(IEnumerable<ILPattern> patterns) : ILPattern {
        public override int MinimumLength => patterns.Sum(pattern => pattern.MinimumLength);

        public override bool Match(ILMatchContext ctx) {
            var thePatterns = ctx.Direction == Direction.Forward ? patterns : patterns.Reverse();
            foreach (var pattern in thePatterns) {
                if (!pattern.Match(ctx))
                    return false;
            }

            return true;
        }
    }

    private sealed class EitherILPattern(ILPattern either, ILPattern or) : ILPattern {
        public override int MinimumLength => Math.Min(either.MinimumLength, or.MinimumLength);

        public override bool Match(ILMatchContext ctx) {
            return either.TryMatch(ctx) || or.Match(ctx);
        }
    }

    private sealed class OpCodeILPattern(OpCode opCode) : ILPattern {
        public override int MinimumLength => 1;

        public override bool Match(ILMatchContext ctx) {
            if (ctx.Current is null)
                return false;

            var success = ctx.Current.OpCode == opCode;
            ctx.TryAdvance();
            return success;
        }
    }

    public abstract int MinimumLength { get; }

    /// <summary>
    ///     Matches an arbitrary condition given a set of instructions.
    /// </summary>
    /// <param name="ctx">The context of the match.</param>
    /// <returns>Whether the match was successful.</returns>
    /// <remarks>
    ///     While <see cref="TryMatch"/> also optionally performs a match,
    ///     <see cref="Match(Reaganism.MonoMix.Pattern.ILMatchContext)"/> on its
    ///     own will leave the position of the <paramref name="ctx"/> modified.
    /// </remarks>
    public abstract bool Match(ILMatchContext ctx);

    /// <summary>
    ///     Attempts to match and resets to the starting position if the match
    ///     fails.
    /// </summary>
    /// <param name="ctx">The context of the match.</param>
    /// <returns>Whether the match was successful.</returns>
    /// <remarks>
    ///     While <see cref="Match(Reaganism.MonoMix.Pattern.ILMatchContext)"/>
    ///     also communicates whether the match was successful,
    ///     <see cref="TryMatch"/> explicitly resets the position of the
    ///     <paramref name="ctx"/> if the match fails.
    /// </remarks>
    public bool TryMatch(ILMatchContext ctx) {
        var instruction = ctx.Current;
        if (Match(ctx))
            return true;

        ctx.Current = instruction;
        return false;
    }

    public static ILMatchResult Match(ILMatchContext ctx, ILPattern pattern) {
        return pattern.Match(ctx) ? new ILMatchResult(true, ctx.Previous, ctx.Current, ctx.Next) : new ILMatchResult(false, null, null, null);
    }

    public static ILMatchResult Match(Instruction? instruction, Direction direction, ILPattern pattern) {
        return Match(new ILMatchContext(instruction, direction), pattern);
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
