using System;
using System.Collections.Generic;

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
    ///     The collection of elements the cursor is positioned within.
    /// </summary>
    IList<T> Elements { get; }

    /// <summary>
    ///     The instruction immediately preceding the cursor position;
    ///     <see langword="null"/> if the cursor is at the beginning of the
    ///     collection.
    /// </summary>
    T? Previous { get; set; }

    /// <summary>
    ///     The instruction immediately following the cursor position;
    ///     <see langword="null"/> if the cursor is at the end of the
    ///     collection.
    /// </summary>
    T? Next { get; set; }

    /// <summary>
    ///     The index of the element immediately following the cursor position.
    /// </summary>
    int Index { get; set; }

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

/// <summary>
///     An abstract implementation of <see cref="ICursor{T}"/> that provides a
///     base for specialized cursor implementations.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public abstract class Cursor<T>(IList<T> elements) : ICursor<T> {
    public IList<T> Elements { get; } = elements;

    public T? Previous {
        get => Next is null ? Elements[^1] : Elements[Index - 1];
        set => Goto(value, CursorMoveType.After);
    }

    public T? Next {
        get => next;
        set => Goto(value);
    }

    public int Index {
        get => next is null ? Elements.Count : Elements.IndexOf(next);
        set => Goto(value);
    }

    private T? next;

    public ICursor<T> Goto(T? element, CursorMoveType moveType = CursorMoveType.Before) {
        /* Understanding move logic:
         *   Before:
         *     Position the cursor *before* the given element, meaning we set
         *     `next` to `element. If `element` is null, it means we want to go
         *     to the end of the list.
         *   After:
         *     Position the cursor *after* the given element, meaning we set
         *     `next` to the element after `element`. If `element` is null, that
         *     means we're at the end of the list. We don't want to wrap so we
         *     just set it to null and move on. If `element` is the last element
         *     (and as such, the following element does not exist), we similarly
         *     set `next` to null. Otherwise, we set `next` to the element after
         *     `element`.
         */

        if (moveType == CursorMoveType.After) {
            if (element is null) {
                next = default;
            }
            else {
                var index = Elements.IndexOf(element);
                if (index == -1)
                    throw new ArgumentException("Element not found in collection.", nameof(element));

                next = index >= Elements.Count ? default : Elements[index + 1];
            }
        }
        else {
            next = element;
        }

        return this;
    }

    public ICursor<T> Goto(int index, CursorMoveType moveType = CursorMoveType.Before) {
        if (index < 0)
            throw new InvalidOperationException("Cannot go to a negative index. This behavior differs from MonoMod's relative negative indexing; did you mean to use GotoRelative?");

        return Goto(index == Elements.Count ? default : Elements[index], moveType);
    }

    public bool TryAdvance(Direction direction) {
        if (direction == Direction.Forward && Next is null)
            return false;

        if (direction == Direction.Backward && Previous is null)
            return false;

        this.GotoRelative(direction == Direction.Forward ? 1 : -1);
        return true;
    }

    public void Advance(Direction direction) {
        if (!TryAdvance(direction))
            throw new InvalidOperationException("Cannot advance cursor in the specified direction.");
    }
}

/// <summary>
///     Provides extension methods for <see cref="ICursor{T}"/> implementations.
/// </summary>
public static class CursorExtensions {
    public static ICursor<T> GotoRelative<T>(this ICursor<T> cursor, int offset) {
        return cursor.Goto(cursor.Index + offset);
    }
}
