using System;

namespace Reaganism.Recon;

/// <summary>
///     A window that abstracts over a doubly-linked list, providing views
///     into the <see cref="Previous"/>, <see cref="Current"/>, and
///     <see cref="Next"/> elements.
/// </summary>
/// <typeparam name="T">The doubly-linked element type.</typeparam>
/// <remarks>
///     It is possible for the <see cref="Current"/> element to be
///     <see langword="null"/>. This is useful for representing being positioned
///     at the very start or very end of a list for purposes of allowing an
///     <see cref="Advance"/> operation to be performed successfully when
///     certain match operations still expect them to (e.g. if a following match
///     in a pattern only cares about the [directional] next or [directional]
///     previous elements).
/// </remarks>
public interface IElementWindow<T> where T : class, IDoublyLinkedElement<T> {
    /// <summary>
    ///     The element previous to the <see cref="Current"/>.
    /// </summary>
    T? Previous { get; }

    /// <summary>
    ///     The current element.
    /// </summary>
    T? Current { get; set; }

    /// <summary>
    ///     The element following the <see cref="Current"/>.
    /// </summary>
    T? Next { get; }

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
public class ElementWindow<T> : IElementWindow<T> where T : class, IDoublyLinkedElement<T> {
    public T? Previous { get; private set; }

    private T? current;

    public T? Current {
        get => current;

        set {
            Previous = value is null ? default : value.Previous;
            current = value;
            Next = value is null ? default : value.Next;
        }
    }

    public T? Next { get; private set; }

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
        throw new System.NotImplementedException();
    }
}
