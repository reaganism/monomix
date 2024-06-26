using System.Collections.Generic;

namespace Reaganism.Recon;

/// <summary>
///     An element that is part of a doubly-linked list.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public interface IDoublyLinkedElement<T> {
    private sealed class StrongDoublyLinkedElement<TStrong> : IDoublyLinkedElement<TStrong> {
        public IDoublyLinkedElement<TStrong>? Previous { get; set; }

        public required TStrong? Value { get; set; }

        public IDoublyLinkedElement<TStrong>? Next { get; set; }
    }

    /// <summary>
    ///     The element previous to this element.
    /// </summary>
    IDoublyLinkedElement<T>? Previous { get; set; }

    /// <summary>
    ///     The value of this element.
    /// </summary>
    T? Value { get; set; }

    /// <summary>
    ///     The element following this element.
    /// </summary>
    IDoublyLinkedElement<T>? Next { get; set; }

    /// <summary>
    ///     Creates a doubly-linked list from the specified enumerable.
    /// </summary>
    /// <param name="enumerable">The enumerable to create the list from.</param>
    /// <returns>
    ///     The head of the doubly-linked list.
    /// </returns>
    /// <remarks>
    ///     The head gives access to the entire doubly-linked list produced from
    ///     the enumerable.
    /// </remarks>
    public static IDoublyLinkedElement<T>? FromEnumerable(IEnumerable<T> enumerable) {
        IDoublyLinkedElement<T>? head = null;
        IDoublyLinkedElement<T>? previous = null;

        foreach (var value in enumerable) {
            var element = new StrongDoublyLinkedElement<T> {
                Value = value,
                Previous = previous,
            };

            if (previous is not null)
                previous.Next = element;

            previous = element;
            head ??= element;
        }

        return head;
    }
}
