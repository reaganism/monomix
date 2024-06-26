namespace Reaganism.Recon;

/// <summary>
///     Indicates whether the position of a cursor is the result of a search
///     function and if the next search should ignore the element preceding
///     or following the cursor.
/// </summary>
public enum SearchTarget {
    /// <summary>
    ///     The cursor is not positioned as a result of a search function.
    /// </summary>
    None,

    /// <summary>
    ///     The cursor is positioned as a result of a search function, cannot
    ///     match the Next element, and must move the cursor forward.
    /// </summary>
    Next,

    /// <summary>
    ///     The cursor is positioned as a result of a search function, cannot
    ///     match the Previous element, and must move the cursor backward.
    /// </summary>
    Previous,
}
