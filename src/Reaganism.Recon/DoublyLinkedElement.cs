using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Reaganism.Recon;

/// <summary>
///     An element that is part of a doubly-linked list.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public interface IDoublyLinkedElement<T> where T : class, IDoublyLinkedElement<T> {
    /// <summary>
    ///     The element previous to this element.
    /// </summary>
    T? Previous { get; set; }

    /// <summary>
    ///     The element following this element.
    /// </summary>
    T? Next { get; set; }
}

/// <summary>
///     A <see cref="StrongBox{T}"/>-like implementation of
///     <see cref="IDoublyLinkedElement{T}"/> that allows for simple automatic
///     wrapping of any type (including value types).
/// </summary>
/// <typeparam name="T">The element to wrap.</typeparam>
public sealed class StrongElement<T> : IDoublyLinkedElement<StrongElement<T>> {
    public StrongElement<T>? Previous { get; set; }

    public required T? Value { get; init; }

    public StrongElement<T>? Next { get; set; }

    /// <summary>
    ///     Creates a new <see cref="StrongElement{T}"/> from an enumerable.
    /// </summary>
    /// <param name="enumerable">
    ///     The collection to convert to a doubly-linked list.
    /// </param>
    /// <returns>
    ///     The head of the doubly-linked list.
    /// </returns>
    /// <remarks>
    ///     Produces a doubly-linked list in the form of the head element.
    /// </remarks>
    public static StrongElement<T>? FromEnumerable(IEnumerable<T> enumerable) {
        StrongElement<T>? head = null;
        StrongElement<T>? previous = null;

        foreach (var value in enumerable) {
            var element = new StrongElement<T> { Value = value, };

            head ??= element;

            if (previous != null) {
                previous.Next = element;
                element.Previous = previous;
            }

            previous = element;
        }

        return head;
    }
}
