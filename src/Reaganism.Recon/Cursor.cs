﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Reaganism.Recon;

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

    /// <summary>
    ///     Whether the cursor has been positioned by a search function and if
    ///     the next search should ignore the element preceding or following
    ///     the cursor.
    /// </summary>
    SearchTarget SearchTarget { get; set; }

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
    /// <param name="setTarget">
    ///     Whether to set the <see cref="SearchTarget"/> and skip the target
    ///     element with the next search function.
    /// </param>
    /// <returns>Returns <see langword="this"/> for chaining.</returns>
    ICursor<T> Goto(T? element, MoveType moveType = MoveType.Before, bool setTarget = false);

    /// <summary>
    ///     Search forward and moves the cursor to the next sequence of elements
    ///     matching the corresponding predicates.
    /// </summary>
    /// <param name="moveType">How to move it.</param>
    /// <param name="predicates">The predicates to evaluate.</param>
    /// <returns>
    ///     <see langword="true"/> if the cursor was successfully advanced;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGotoNext(MoveType moveType = MoveType.Before, params Func<T, bool>[] predicates);

    /// <summary>
    ///     Search backward and moves the cursor to the next sequence of
    ///     elements matching the corresponding predicates.
    /// </summary>
    /// <param name="moveType">How to move it.</param>
    /// <param name="predicates">The predicates to evaluate.</param>
    /// <returns>
    ///     <see langword="true"/> if the cursor was successfully advanced;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGotoPrevious(MoveType moveType = MoveType.Before, params Func<T, bool>[] predicates);

    /// <summary>
    ///     Finds the next occurrences of a series of elements matching the
    ///     given set of predicates with gaps permitted.
    /// </summary>
    /// <param name="elements">
    ///     A reference to each element in the found set of elements.
    /// </param>
    /// <param name="predicates">The predicates to match.</param>
    /// <returns>
    ///     <see langword="true"/> if the elements were found; otherwise,
    ///     <see langword="false"/>.
    /// </returns>
    bool TryFindNext([NotNullWhen(returnValue: true)] out T[]? elements, params Func<T, bool>[] predicates);

    /// <summary>
    ///     Finds the previous occurrences of a series of elements matching the
    ///     given set of predicates with gaps permitted.
    /// </summary>
    /// <param name="elements">
    ///     A reference to each element found in the set of elements.
    /// </param>
    /// <param name="predicates">The predicates to match.</param>
    /// <returns>
    ///     <see langword="true"/> if the elements were found; otherwise,
    ///     <see langword="false"/>.
    /// </returns>
    bool TryFindPrevious([NotNullWhen(returnValue: true)] out T[]? elements, params Func<T, bool>[] predicates);

    /// <summary>
    ///     Attempts to advance the cursor in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to advance in.</param>
    /// <returns>
    ///     <see langword="true"/> if the cursor was successfully advanced;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    bool TryAdvance(Direction direction);
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
        set => Goto(value, MoveType.After);
    }

    public T? Next {
        get => next;
        set => Goto(value);
    }

    public int Index {
        get => next is null ? Elements.Count : Elements.IndexOf(next);
        set => this.GotoIndex(value);
    }

    public SearchTarget SearchTarget {
        get => searchTarget;

        set {
            if ((value == SearchTarget.Next && Next is null) || (value == SearchTarget.Previous && Previous is null)) {
                searchTarget = SearchTarget.None;
                return;
            }

            searchTarget = value;
        }
    }

    private T? next;
    private SearchTarget searchTarget;

    public ICursor<T> Goto(T? element, MoveType moveType = MoveType.Before, bool setTarget = false) {
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

        if (moveType == MoveType.After) {
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

        if (setTarget)
            SearchTarget = moveType == MoveType.After ? SearchTarget.Previous : SearchTarget.Next;
        else
            SearchTarget = SearchTarget.None;

        return this;
    }

    public bool TryGotoNext(MoveType moveType = MoveType.Before, params Func<T, bool>[] predicates) {
        var i = Index;
        if (SearchTarget == SearchTarget.Next)
            i++;

        for (; i + predicates.Length <= Elements.Count; i++) {
            for (var j = 0; j < predicates.Length; j++) {
                if (!predicates[j](Elements[i + j]))
                    goto next;
            }

            this.GotoIndex(moveType == MoveType.After ? i + predicates.Length - 1 : i, moveType, true);
            return true;

            next: ;
        }

        return false;
    }

    public bool TryGotoPrevious(MoveType moveType = MoveType.Before, params Func<T, bool>[] predicates) {
        var i = Index - 1;
        if (SearchTarget == SearchTarget.Previous)
            i--;

        for (; i >= 0; i--) {
            for (var j = 0; j < predicates.Length; j++) {
                if (!predicates[j](Elements[i - j]))
                    goto next;
            }

            this.GotoIndex(moveType == MoveType.After ? i + predicates.Length - 1 : i, moveType, true);
            return true;

            next: ;
        }

        return false;
    }

    public bool TryFindNext([NotNullWhen(true)] out T[]? elements, params Func<T, bool>[] predicates) {
        elements = new T[predicates.Length];

        for (var i = 0; i < predicates.Length; i++) {
            if (!TryGotoNext(MoveType.Before, predicates[i]))
                return false;

            elements[i] = Next!;
        }

        return true;
    }

    public bool TryFindPrevious([NotNullWhen(true)] out T[]? elements, params Func<T, bool>[] predicates) {
        elements = new T[predicates.Length];

        for (var i = predicates.Length - 1; i >= 0; i--) {
            if (!TryGotoPrevious(MoveType.Before, predicates[i]))
                return false;

            elements[i] = Next!;
        }

        return true;
    }

    public bool TryAdvance(Direction direction) {
        if (direction == Direction.Forward && Next is null)
            return false;

        if (direction == Direction.Backward && Previous is null)
            return false;

        this.GotoRelative(direction == Direction.Forward ? 1 : -1);
        return true;
    }
}

/// <summary>
///     Provides extension methods for <see cref="ICursor{T}"/> implementations.
/// </summary>
public static class CursorExtensions {
    /// <summary>
    ///     Moves the cursor to the element at the specified index.
    /// </summary>
    /// <param name="cursor">The cursor.</param>
    /// <param name="index">The index of the target element.</param>
    /// <param name="moveType">How to move to it.</param>
    /// <param name="setTarget">
    ///     Whether to set the <see cref="SearchTarget"/> and skip the target
    ///     element with the next search function.
    /// </param>
    /// <returns>The <paramref name="cursor"/> for chaining.</returns>
    public static ICursor<T> GotoIndex<T>(this ICursor<T> cursor, int index, MoveType moveType = MoveType.Before, bool setTarget = false) {
        if (index < 0)
            throw new InvalidOperationException("Cannot go to a negative index. This behavior differs from MonoMod's relative negative indexing; did you mean to use GotoRelative?");

        return cursor.Goto(index == cursor.Elements.Count ? default : cursor.Elements[index], moveType, setTarget);
    }

    /// <summary>
    ///     Moves the cursor to the element at the specified index.
    /// </summary>
    /// <param name="cursor">The cursor.</param>
    /// <param name="offset">The offset relative to the index.</param>
    /// <param name="moveType">How to move to it.</param>
    /// <param name="setTarget">
    ///     Whether to set the <see cref="SearchTarget"/> and skip the target
    ///     element with the next search function.
    /// </param>
    /// <returns>The <paramref name="cursor"/> for chaining.</returns>
    public static ICursor<T> GotoRelative<T>(this ICursor<T> cursor, int offset, MoveType moveType = MoveType.Before, bool setTarget = false) {
        return GotoIndex(cursor, cursor.Index + offset, moveType, setTarget);
    }

    /// <summary>
    ///     Search forward and moves the cursor to the next sequence of elements
    ///     matching the corresponding predicates.
    /// </summary>
    /// <param name="cursor">The cursor.</param>
    /// <param name="moveType">How to move it.</param>
    /// <param name="predicates">The predicates to evaluate.</param>
    public static void GotoNext<T>(this ICursor<T> cursor, MoveType moveType = MoveType.Before, params Func<T, bool>[] predicates) {
        if (!cursor.TryGotoNext(moveType, predicates))
            throw new InvalidOperationException("Cannot advance cursor given the predicates.");
    }

    /// <summary>
    ///     Search backward and moves the cursor to the next sequence of
    ///     elements matching the corresponding predicates.
    /// </summary>
    /// <param name="cursor">The cursor.</param>
    /// <param name="moveType">How to move it.</param>
    /// <param name="predicates">The predicates to evaluate.</param>
    public static void GotoPrevious<T>(this ICursor<T> cursor, MoveType moveType = MoveType.Before, params Func<T, bool>[] predicates) {
        if (!cursor.TryGotoPrevious(moveType, predicates))
            throw new InvalidOperationException("Cannot advance cursor given the predicates.");
    }

    /// <summary>
    ///     Finds the next occurrences of a series of elements matching the
    ///     given set of predicates with gaps permitted.
    /// </summary>
    /// <param name="cursor">The cursor.</param>
    /// <param name="elements">
    ///     A reference to each element in the found set of elements.
    /// </param>
    /// <param name="predicates">The predicates to match.</param>
    public static void FindNext<T>(this ICursor<T> cursor, out T[] elements, params Func<T, bool>[] predicates) {
        if (!cursor.TryFindNext(out elements!, predicates))
            throw new InvalidOperationException("Cannot find the specified elements.");
    }

    /// <summary>
    ///     Finds the previous occurrences of a series of elements matching the
    ///     given set of predicates with gaps permitted.
    /// </summary>
    /// <param name="cursor">The cursor.</param>
    /// <param name="elements">
    ///     A reference to each element found in the set of elements.
    /// </param>
    /// <param name="predicates">The predicates to match.</param>
    public static void FindPrevious<T>(this ICursor<T> cursor, out T[]? elements, params Func<T, bool>[] predicates) {
        if (!cursor.TryFindPrevious(out elements!, predicates))
            throw new InvalidOperationException("Cannot find the specified elements.");
    }

    /// <summary>
    ///     Advances the cursor in the specified direction.
    /// </summary>
    /// <param name="cursor">The cursor.</param>
    /// <param name="direction">The direction to advance in.</param>
    public static void Advance<T>(this ICursor<T> cursor, Direction direction) {
        if (!cursor.TryAdvance(direction))
            throw new InvalidOperationException("Cannot advance cursor in the specified direction.");
    }
}
