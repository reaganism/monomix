using System;
using System.Collections.Generic;

namespace Reaganism.Recon;

/// <summary>
///     A window that abstracts over a list, positioning itself in between list
///     elements and providing access to the elements before
///     (<see cref="Previous"/>) and after (<see cref="Next"/>
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public interface IElementWindow<T> {
    /// <summary>
    ///     The element before the current position.
    /// </summary>
    T? Previous { get; }

    /// <summary>
    ///     The element after the current position
    /// </summary>
    T? Next { get; }

    /// <summary>
    ///     The index of the element immediately following the position.
    /// </summary>
    int Index { get; }

    void Goto(T? element);

    /// <summary>
    ///     Attempts to advance the window in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to advance in.</param>
    /// <returns>
    ///     <see langword="true"/> if the window was successfully advanced;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    bool TryAdvance(Direction direction);

    /// <summary>
    ///     Advances the window in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to advance in.</param>
    void Advance(Direction direction);
}

/// <summary>
///     A simple implementation of <see cref="IElementWindow{T}"/>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public class ElementWindow<T>(IList<T> list) : IElementWindow<T> {
    public T? Previous { get; private set; }

    public T? Next { get; private set; }

    private readonly IList<T> list = list;

    public bool TryAdvance(Direction direction) {
        switch (direction) {
            case Direction.Forward:
                // When we have reached the end of the list.
                if (Next is null) {
                    // If the Next and Current are both null, we cannot advance
                    // farther.
                    // In a way, we are assuming Previous isn't null, but it
                    // isn't necessarily guaranteed (e.g. a list with zero
                    // elements).
                    if (Current is null)
                        return false;

                    // Make sure we preserve a reference to the last
                    // instruction.
                    var previous = Current;
                    Current = default;
                    Previous = previous;
                    return true;
                }

                // We can update like normal if Next isn't null.
                Current = Next;
                return true;

            case Direction.Backward:
                // When we have reached the start of the list.
                if (Previous is null) {
                    // If the Previous and Current are both null, we cannot
                    // advance farther.
                    // In a way, we are assuming Next isn't null, but it isn't
                    // necessarily guaranteed (e.g. a list with zero elements).
                    if (Current is null)
                        return false;

                    // Make sure we preserve a reference to the first
                    // instruction.
                    var next = Current;
                    Current = default;
                    Next = next;
                    return true;
                }

                // We can update like normal if Previous isn't null.
                Current = Previous;
                return true;

            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public void Advance(Direction direction) {
        if (!TryAdvance(direction))
            throw new InvalidOperationException("Cannot advance the window in the specified direction.");
    }
}
