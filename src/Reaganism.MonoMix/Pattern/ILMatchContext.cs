using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mono.Cecil.Cil;

namespace Reaganism.MonoMix.Pattern;

public sealed class ILMatchContext(Instruction? instruction, ILPattern.Direction direction = ILPattern.Direction.Forward) {
    private sealed class InstructionWindow {
        public Instruction? Previous { get; private set; }

        public Instruction? Current { get; private set; }

        public Instruction? Next { get; private set; }

        public InstructionWindow(Instruction? instruction) {
            SetCurrentInstruction(instruction);
        }

        public void SetCurrentInstruction(Instruction? instruction) {
            Previous = instruction?.Previous;
            Current = instruction;
            Next = instruction?.Next;
        }

        /// <summary>
        ///     Advances the current instruction to the next instruction
        ///     (depending on the <see cref="ILPattern.Direction"/>).
        ///     <br />
        ///     If the direction is <see cref="ILPattern.Direction.Forward"/>,
        ///     the current instruction will be set to the next instruction and
        ///     references will be updated appropriately. If the next
        ///     instruction is <see langword="null"/>, the current instruction
        ///     will be set to <see langword="null"/> and the previous
        ///     instruction will hold a reference to the last instruction in the
        ///     list.
        ///     <br />
        ///     If the direction is <see cref="ILPattern.Direction.Backward"/>,
        ///     the current instruction will be set to the previous instruction
        ///     and references will be updated appropriately. If the previous
        ///     instruction is <see langword="null"/>, the current instruction
        ///     will be set to <see langword="null"/> and the next instruction
        ///     will hold a reference to the first instruction in the list.
        /// </summary>
        /// <param name="direction">
        ///     The direction to advance the current instruction.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation was successful;
        ///     <see langword="false"/> if the operation failed.
        /// </returns>
        public bool TryAdvance(ILPattern.Direction direction) {
            switch (direction) {
                case ILPattern.Direction.Forward:
                    // When we have reached the end of the list.
                    if (Next is null) {
                        // If the next and current are both null, we cannot
                        // advance farther.
                        // In a way, we are assuming Previous isn't null, but it
                        // isn't necessarily guaranteed.
                        if (Current is null)
                            return false;

                        // Make sure we preserve a reference to the last
                        // instruction.
                        var previous = Current;
                        SetCurrentInstruction(null);
                        Previous = previous;
                        return true;
                    }

                    // We can update like normal if Next isn't null.
                    SetCurrentInstruction(Next);
                    return true;

                case ILPattern.Direction.Backward:
                    // When we have reached the beginning of the list.
                    if (Previous is null) {
                        // If the previous and current are both null, we cannot
                        // advance farther.
                        // In a way, we are assuming Next isn't null, but it
                        // isn't necessarily guaranteed.
                        if (Current is null)
                            return false;

                        // Make sure we preserve a reference to the first
                        // instruction.
                        var next = Current;
                        SetCurrentInstruction(null);
                        Next = next;
                        return true;
                    }

                    // We can update like normal if Previous isn't null.
                    SetCurrentInstruction(Previous);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }

    /// <summary>
    ///     The instruction previous to the current instruction relative to the
    ///     instruction list.
    /// </summary>
    public Instruction? Previous => instructionWindow.Previous;

    /// <summary>
    ///     The instruction functionally previous to the current instruction
    ///     accounting for the <see cref="ILPattern.Direction"/>.
    /// </summary>
    public Instruction? DirectionalPrevious => Direction switch {
        ILPattern.Direction.Forward => Previous,
        ILPattern.Direction.Backward => Next,
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null),
    };

    /// <summary>
    ///     The current instruction being pointed to. It is possible for
    ///     <see cref="Current"/> to be <see langword="null"/> in arguably
    ///     unorthodox positional states.
    /// </summary>
    public Instruction? Current {
        get => instructionWindow.Current;
        set => instructionWindow.SetCurrentInstruction(value);
    }

    /// <summary>
    ///     The instruction following the current instruction relative to the
    ///     instruction list.
    /// </summary>
    public Instruction? Next => instructionWindow.Next;

    /// <summary>
    ///     The instruction functionally following the current instruction
    ///     accounting for the <see cref="ILPattern.Direction"/>.
    /// </summary>
    public Instruction? DirectionalNext => Direction switch {
        ILPattern.Direction.Forward => Next,
        ILPattern.Direction.Backward => Previous,
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null),
    };

    public ILPattern.Direction Direction { get; } = direction;

    private readonly Dictionary<object, object> data = [];
    private readonly InstructionWindow instructionWindow = new(instruction);

    public bool TryGetData(object key, [NotNullWhen(returnValue: true)] out object? value) {
        return data.TryGetValue(key, out value);
    }

    public void AddData(object key, object value) {
        data[key] = value;
    }

    /// <summary>
    ///     Advances the current instruction to the next instruction (depending
    ///     on the <see cref="ILPattern.Direction"/>).
    ///     <br />
    ///     If the direction is <see cref="ILPattern.Direction.Forward"/>, the
    ///     current instruction will be set to the next instruction and
    ///     references will be updated appropriately. If the next instruction
    ///     is <see langword="null"/>, the current instruction will be set to
    ///     <see langword="null"/> and the previous instruction will hold a
    ///     reference to the last instruction in the list.
    ///     <br />
    ///     If the direction is <see cref="ILPattern.Direction.Backward"/>, the
    ///     current instruction will be set to the previous instruction and
    ///     references will be updated appropriately. If the previous
    ///     instruction is <see langword="null"/>, the current instruction will
    ///     be set to <see langword="null"/> and the next instruction will hold
    ///     a reference to the first instruction in the list.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the operation was successful;
    ///     <see langword="false"/> if the operation failed.
    /// </returns>
    public bool TryAdvance() {
        return instructionWindow.TryAdvance(Direction);
    }
}
