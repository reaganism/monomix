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
public interface ICursor<T> {
    /// <summary>
    ///     The instruction immediately preceding the cursor position;
    ///     <see langword="null"/> if the cursor is at the beginning of the
    ///     collection.
    /// </summary>
    T? Previous { get; }

    /// <summary>
    ///     The instruction immediately following the cursor position;
    ///     <see langword="null"/> if the cursor is at the end of the
    ///     collection.
    /// </summary>
    T? Next { get; }

    /// <summary>
    ///     The index of the element immediately following the cursor position.
    /// </summary>
    int Index { get; }

    // TODO: Provide TryGoto functions?
    // Goto(T?, ...) doesn't need one most likely because you use references
    // that already exist.
    // Goto(int, ...) could use one because you might give a faulty index? You
    // might do Goto(Index + 1, ...) f.e. when you're already at the end? IDK.

    /// <summary>
    ///     Moves the cursor to the specified element.
    /// </summary>
    /// <param name="element">The target element.</param>
    /// <param name="moveType">How to move to it.</param>
    /// <returns>Returns <see langword="this"/> for chaining.</returns>
    ICursor<T> Goto(T? element, CursorMoveType moveType = CursorMoveType.Before);

    /// <summary>
    ///     Moves the cursor to the element at the specified index.
    /// </summary>
    /// <param name="index">The index of the target element.</param>
    /// <param name="moveType">How to move to it.</param>
    /// <returns>Returns <see langword="this"/> for chaining.</returns>
    ICursor<T> Goto(int index, CursorMoveType moveType = CursorMoveType.Before);

    /// <summary>
    ///     Attempts to advance the cursor in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to advance in.</param>
    /// <returns>
    ///     <see langword="true"/> if the cursor was successfully advanced;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    bool TryAdvance(Direction direction);

    /// <summary>
    ///     Advances the cursor in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to advance in.</param>
    void Advance(Direction direction);
}
