namespace Reaganism.Recon;

// TODO: AfterLabel? How to generalize?
/// <summary>
///     Specifies how a cursor should be positioned relative to the target of a
///     search function.
/// </summary>
public enum CursorMoveType {
    /// <summary>
    ///     Moves the cursor before the first element in the search.
    /// </summary>
    Before,

    /// <summary>
    ///     Moves the cursor after the last element in the search.
    /// </summary>
    After,
}

/// <summary>
///     A cursor that may be positioned between elements in a collection.
/// </summary>
/// <typeparam name="T">The doubly-linked element type.</typeparam>
/// <remarks>
///     Generalized advance positioning methods (finding, go-to) are provided.
///     Specialized search functionality should be provided through extension
///     methods.
/// </remarks>
public interface ICursor<T> where T : IDoublyLinkedElement { }
