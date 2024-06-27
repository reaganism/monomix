using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;
using Reaganism.MonoMix.Cil;
using Reaganism.MonoMix.Cil.Match;
using Reaganism.Recon.Matching;
using static Reaganism.Recon.Matching.PatternBuilder;
using static Reaganism.MonoMix.Cil.Match.PatternBuilderExtensions;

namespace Reaganism.MonoMix.Reflection;

/// <summary>
///     Utilities for resolving backing fields from properties.
/// </summary>
public static class BackingFieldResolver {
    private sealed class FieldILPattern(Pattern<Instruction> pattern) : Pattern<Instruction> {
        public static readonly object FIELD_KEY = new();

        public override int MinimumLength => 1;

        public override bool Match(MatchContext<Instruction> ctx) {
            if (!pattern.Match(ctx))
                return false;

            var match = ctx.Previous;
            if (match?.Operand is not FieldInfo fieldInfo)
                throw new InvalidOperationException("Field instruction must have a field operand");

            if (ctx.TryGetData(FIELD_KEY, out var otherFieldInfo) && !ReferenceEquals(fieldInfo, otherFieldInfo))
                throw new InvalidOperationException("Field instruction must have the same field operand");

            ctx.SetData(FIELD_KEY, fieldInfo);
            return true;
        }
    }

    private static readonly Pattern<Instruction> getter_pattern = Sequence<Instruction>(
        x => {
            x.Optional(OpCodes.Nop);
            x.Either(
                new FieldILPattern(OpCode(OpCodes.Ldsfld)),
                Sequence<Instruction>(
                    y => {
                        y.OpCode(OpCodes.Ldarg_0);
                        y.AddPattern(new FieldILPattern(OpCode(OpCodes.Ldfld)));
                    }
                )
            );

            x.Optional(
                Sequence<Instruction>(
                    y => {
                        OpCode(OpCodes.Stloc_0);
                        OpCode(OpCodes.Br_S);
                        OpCode(OpCodes.Ldloc_0);
                    }
                )
            );

            x.Optional(OpCodes.Br_S);
            x.OpCode(OpCodes.Ret);
        }
    );

    private static readonly Pattern<Instruction> setter_pattern = Sequence<Instruction>(
        x => {
            x.Optional(OpCodes.Nop);
            x.OpCode(OpCodes.Ldarg_0);
            x.Either(
                new FieldILPattern(OpCode(OpCodes.Stsfld)),
                Sequence<Instruction>(
                    y => {
                        y.OpCode(OpCodes.Ldarg_1);
                        y.AddPattern(new FieldILPattern(OpCode(OpCodes.Stfld)));
                    }
                )
            );

            x.OpCode(OpCodes.Ret);
        }
    );

    /// <summary>
    ///     Gets the backing field of a property.
    /// </summary>
    /// <param name="propertyInfo">The property.</param>
    /// <returns>
    ///     The backing field of the property, or <see langword="null"/> if the
    ///     property does not have a backing field (or it for some reason could
    ///     not be resolved).
    /// </returns>
    public static FieldInfo? GetBackingField(this PropertyInfo propertyInfo) {
        var getter = propertyInfo.GetGetMethod(true);
        if (getter is not null)
            return GetBackingField(getter, getter_pattern);

        var setter = propertyInfo.GetSetMethod(true);
        if (setter is not null)
            return GetBackingField(setter, setter_pattern);

        return null;
    }

    private static FieldInfo? GetBackingField(MethodInfo methodInfo, Pattern<Instruction> pattern) {
        var c = new TemporaryILCursorForTesting(InstructionProvider.FromMethodBaseAsSystem(methodInfo).ToList());
        if (!c.TryFindNextPattern(pattern, out var ctx, out _))
            return null;

        if (!ctx.TryGetData(FieldILPattern.FIELD_KEY, out var fieldInfo) || fieldInfo is not FieldInfo theFieldInfo)
            return null;

        return theFieldInfo;
    }
}
