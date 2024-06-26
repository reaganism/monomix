namespace Reaganism.Recon;

/// <summary>
///     Species the direction to perform a search or match in.
/// </summary>
/// <remarks>
///     Affects e.g. the direction a <see cref="IElementWindow{T}"/> advances.
/// </remarks>
public enum Direction {
    /// <summary>
    ///     Searches forward.
    /// </summary>
    Forward,

    /// <summary>
    ///     Searches backward.
    /// </summary>
    Backward,
}
