using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Cil;
using MonoMod.Utils;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Reaganism.MonoMix;

/// <summary>
///     Minimally-contractual object that provides enumeration over
///     Mono.Cecil-compatible IL instructions.
/// </summary>
public interface IILProvider : IEnumerable<Instruction> {
    private abstract class AbstractILProvider(Instruction? instruction) : IILProvider {
        public Instruction? Instruction { get; set; } = instruction;

        public bool TryGotoPrev() {
            if (Instruction?.Previous is null)
                return false;

            Instruction = Instruction.Previous;
            return true;
        }

        public bool TryGotoNext() {
            if (Instruction is null)
                return false;

            // We don't care if Next is null; if anything, we like that -- it
            // means we can easily denote the end of the instruction list.
            Instruction = Instruction.Next;
            return true;
        }

        public abstract IEnumerator<Instruction> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    private sealed class MethodBodyILProvider(MethodBody methodBody) : AbstractILProvider(methodBody.Instructions.First()) {
        public override IEnumerator<Instruction> GetEnumerator() {
            // ReSharper disable once NotDisposedResourceIsReturned
            return ((IEnumerable<Instruction>)methodBody.Instructions).GetEnumerator();
        }
    }

    private sealed class ILContextILProvider(ILContext context) : AbstractILProvider(context.Instrs.First()) {
        public override IEnumerator<Instruction> GetEnumerator() {
            // ReSharper disable once NotDisposedResourceIsReturned
            return ((IEnumerable<Instruction>)context.Instrs).GetEnumerator();
        }
    }

    private sealed class ILCursorILProvider(ILCursor cursor) : AbstractILProvider(cursor.Instrs.First()) {
        public override IEnumerator<Instruction> GetEnumerator() {
            // ReSharper disable once NotDisposedResourceIsReturned
            return ((IEnumerable<Instruction>)cursor.Instrs).GetEnumerator();
        }
    }

    private sealed class MethodBaseILProvider : AbstractILProvider {
        private readonly Collection<Instruction> instructions;

        public MethodBaseILProvider(MethodBase methodBase) : this(new DynamicMethodDefinition(methodBase).Definition.Body.Instructions) { }

        private MethodBaseILProvider(Collection<Instruction> instructions) : base(instructions.First()) {
            this.instructions = instructions;
        }

        public override IEnumerator<Instruction> GetEnumerator() {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    ///     The current instruction.
    /// </summary>
    /// <remarks>
    ///     <see langword="null"/> when the instruction list has been advanced
    ///     to the end or when it is empty.
    /// </remarks>
    Instruction? Instruction { get; set; }

    /// <summary>
    ///     Attempts to set <see cref="Instruction"/> to the previous
    ///     instruction.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the operation was successful;
    ///     <see langword="false"/> if the operation failed.
    /// </returns>
    bool TryGotoPrev();

    /// <summary>
    ///     Attempts to set <see cref="Instruction"/> to the next instruction.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the operation was successful;
    ///     <see langword="false"/> if the operation failed.
    /// </returns>
    bool TryGotoNext();

    public static IILProvider FromMethodBody(MethodBody methodBody) {
        return new MethodBodyILProvider(methodBody);
    }

    public static IILProvider FromILContext(ILContext context) {
        return new ILContextILProvider(context);
    }

    public static IILProvider FromILCursor(ILCursor cursor) {
        return new ILCursorILProvider(cursor);
    }

    public static IILProvider FromMethodBase(MethodBase methodBase) {
        return new MethodBaseILProvider(methodBase);
    }
}
