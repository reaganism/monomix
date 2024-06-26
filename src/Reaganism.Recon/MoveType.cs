namespace Reaganism.Recon;

// TODO: AfterLabel? How to generalize?
/// <summary>
///     Specifies how a cursor should be positioned relative to the target of a
///     search function.
/// </summary>
public enum MoveType {
    /// <summary>
    ///     Moves the cursor before the first element in the search.
    /// </summary>
    Before,

    /// <summary>
    ///     Moves the cursor after the last element in the search.
    /// </summary>
    After,
}
