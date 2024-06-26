namespace Reaganism.Recon;

/// <summary>
///     An element that is part of a doubly-linked list.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public interface IDoublyLinkedElement<out T> where T : class, IDoublyLinkedElement<T> {
    /// <summary>
    ///     The element previous to this element.
    /// </summary>
    T? Previous { get; }

    /// <summary>
    ///     The element following this element.
    /// </summary>
    T? Next { get; }
}
