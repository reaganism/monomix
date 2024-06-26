namespace Reaganism.Recon.Matching;

/// <summary>
///     A pattern, which is an abstract object that may match against elements
///     provided by a <see cref="ICursor{T}"/>.
/// </summary>
public abstract class Pattern<T> {
    /// <summary>
    ///     The minimum possible length of elements required to produce a valid
    ///     match against this pattern.
    /// </summary>
    /// <remarks>
    ///     This value is used to optimize searching in cases where it may be
    ///     known that the amount of available objects to match against is less
    ///     than the minimum length, allowing for a definite early exit.
    /// </remarks>
    public abstract int MinimumLength { get; }

    /// <summary>
    ///     Matches an arbitrary condition provided by the pattern.
    /// </summary>
    /// <param name="ctx">The context of the match.</param>
    /// <returns>Whether the match was successful.</returns>
    /// <remarks>
    ///     While <see cref="TryMatch"/> also performs a match and returns whether
    ///     it was successful, <see cref="Match"/> on its own will leave the
    ///     <see cref="ICursor{T}"/> of the <paramref name="ctx"/>'s position
    ///     modified.
    /// </remarks>
    public abstract bool Match(MatchContext<T> ctx);

    /// <summary>
    ///     Attempts to match and resets to the starting position if the match
    ///     fails.
    /// </summary>
    /// <param name="ctx">The context of the match.</param>
    /// <returns>Whether the match was successful.</returns>
    /// <remarks>
    ///     While <see cref="Match"/> also communicates whether the match was
    ///     successful, <see cref="TryMatch"/> explicitly resets the position of
    ///     the <paramref name="ctx"/>'s cursor if the match fails.
    /// </remarks>
    public bool TryMatch(MatchContext<T> ctx) {
        var next = ctx.Cursor.Next;
        if (Match(ctx))
            return true;

        ctx.Cursor.Next = next;
        return false;
    }
}

/// <summary>
///     Static utilities for <see cref="Pattern{T}"/>s.
/// </summary>
public static class Pattern {
    /// <summary>
    ///     Matches a pattern within a context.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <param name="pattern">The pattern.</param>
    /// <typeparam name="T">The element type.</typeparam>
    /// <returns>
    ///     <see langword="true"/> if the pattern matched successfully;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Match<T>(MatchContext<T> ctx, Pattern<T> pattern) {
        return pattern.Match(ctx);
    }
}
