using System;
using System.Collections.Generic;
using System.Linq;

namespace Reaganism.Recon.Matching;

/// <summary>
///     Abstractions for declaratively building patterns.
/// </summary>
public abstract class PatternBuilder<T> {
    public class Optional(Pattern<T> pattern) : PatternBuilder<T> {
        private sealed class Pattern(Pattern<T> pattern) : Pattern<T> {
            public override int MinimumLength => 0;

            public override bool Match(MatchContext<T> ctx) {
                pattern.TryMatch(ctx);
                return true;
            }
        }

        public override Pattern<T> Build() {
            return new Pattern(pattern);
        }
    }

    public class Either(Pattern<T> a, Pattern<T> b) : PatternBuilder<T> {
        private sealed class Pattern(Pattern<T> a, Pattern<T> b) : Pattern<T> {
            public override int MinimumLength => Math.Min(a.MinimumLength, b.MinimumLength);

            public override bool Match(MatchContext<T> ctx) {
                return a.TryMatch(ctx) || b.Match(ctx);
            }
        }

        public override Pattern<T> Build() {
            return new Pattern(a, b);
        }
    }

    public class Sequence : PatternBuilder<T> {
        private sealed class Pattern(IEnumerable<Pattern<T>> patterns) : Pattern<T> {
            private readonly Pattern<T>[] patterns = patterns.ToArray();

            public override int MinimumLength => patterns.Sum(x => x.MinimumLength);

            public override bool Match(MatchContext<T> ctx) {
                var thePatterns = ctx.Direction == Direction.Forward ? patterns : patterns.Reverse();
                foreach (var pattern in thePatterns) {
                    if (!pattern.Match(ctx))
                        return false;
                }

                return true;
            }
        }

        private readonly List<Pattern<T>> patterns;

        public Sequence(params Pattern<T>[] patterns) {
            this.patterns = patterns.ToList();
        }

        public Sequence(Action<Sequence> func) {
            patterns = [];
            func(this);
        }

        public override void AddPattern(Pattern<T> pattern) {
            patterns.Add(pattern);
        }

        public override Pattern<T> Build() {
            return new Pattern(patterns);
        }
    }

    public virtual void AddPattern(Pattern<T> pattern) {
        throw new InvalidOperationException("Attempted to add a pattern to a pattern builder that does not support addition");
    }

    public abstract Pattern<T> Build();
}

/// <summary>
///     Static utilities and extensions for creating pattern builds and building
///     them.
/// </summary>
public static class PatternBuilder {
    #region Optional
    public static Pattern<T> Optional<T>(Pattern<T> pattern) {
        return new PatternBuilder<T>.Optional(pattern).Build();
    }

    public static PatternBuilder<T> Optional<T>(this PatternBuilder<T> builder, Pattern<T> pattern) {
        builder.AddPattern(Optional(pattern));
        return builder;
    }
    #endregion

    #region Either
    public static Pattern<T> Either<T>(Pattern<T> a, Pattern<T> b) {
        return new PatternBuilder<T>.Either(a, b).Build();
    }

    public static PatternBuilder<T> Either<T>(this PatternBuilder<T> builder, Pattern<T> a, Pattern<T> b) {
        builder.AddPattern(Either(a, b));
        return builder;
    }
    #endregion

    #region Sequence
    public static Pattern<T> Sequence<T>(params Pattern<T>[] patterns) {
        return new PatternBuilder<T>.Sequence(patterns).Build();
    }

    public static Pattern<T> Sequence<T>(Action<PatternBuilder<T>.Sequence> func) {
        return new PatternBuilder<T>.Sequence(func).Build();
    }

    public static PatternBuilder<T> Sequence<T>(this PatternBuilder<T> builder, params Pattern<T>[] patterns) {
        builder.AddPattern(Sequence(patterns));
        return builder;
    }

    public static PatternBuilder<T> Sequence<T>(this PatternBuilder<T> builder, Action<PatternBuilder<T>.Sequence> func) {
        builder.AddPattern(Sequence(func));
        return builder;
    }
    #endregion
}
