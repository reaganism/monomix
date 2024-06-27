using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Reaganism.Recon.Matching;

/// <summary>
///     The context in which a match is being performed.
/// </summary>
public sealed class MatchContext<T>(ICursor<T> cursor, Direction direction) {
    /// <summary>
    ///     The cursor that the match is being performed on.
    /// </summary>
    public ICursor<T> Cursor { get; } = cursor;

    /// <summary>
    ///     The direction of the match.
    /// </summary>
    public Direction Direction { get; } = direction;

    public T? Previous => Direction == Direction.Forward ? Cursor.Previous : Cursor.Next;

    public T? Next => Direction == Direction.Forward ? Cursor.Next : Cursor.Previous;

    private readonly Dictionary<object, object> data = [];

    /// <summary>
    ///     Attempts to retrieve data from the context.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///     <see langword="true"/> if the data was successfully retrieved;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetData(object key, [NotNullWhen(returnValue: true)] out object? value) {
        return data.TryGetValue(key, out value);
    }

    /// <summary>
    ///     Sets data in the context.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void SetData(object key, object value) {
        data[key] = value;
    }

    /// <summary>
    ///     Advances the cursor in the direction of the match.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the cursor was successfully advanced;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryAdvance() {
        return Cursor.TryAdvance(Direction);
    }

    /// <summary>
    ///     Advances the cursor in the direction of the match.
    /// </summary>
    public void Advance() {
        Cursor.Advance(Direction);
    }
}
