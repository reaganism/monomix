using System.Reflection;
using Mono.Cecil.Cil;
using Reaganism.MonoMix.Pattern;

namespace Reaganism.MonoMix.Reflection;

/// <summary>
///     Utilities for resolving backing fields from properties.
/// </summary>
public static class BackingFieldResolver {
    private sealed class FieldILPattern(ILPattern pattern) : ILPattern {
        public override int MinimumLength => 1;

        public FieldInfo? Field { get; private set; }

        public override bool Match(IILProvider ilProvider, Direction direction) {
            if (!pattern.Match(ilProvider, direction))
                return false;

            var match = ilProvider.Instruction?.Previous;
            Field = match?.Operand as FieldInfo;
            return true;
        }
    }

    private static readonly ILPattern getter_pattern = ILPattern.Sequence(
        ILPattern.Optional(OpCodes.Nop),
        ILPattern.Either(
            new FieldILPattern(ILPattern.OpCode(OpCodes.Ldsfld)),
            ILPattern.Sequence(
                ILPattern.OpCode(OpCodes.Ldarg_0),
                new FieldILPattern(ILPattern.OpCode(OpCodes.Ldfld))
            )
        ),
        ILPattern.Optional(
            ILPattern.Sequence(
                ILPattern.OpCode(OpCodes.Stloc_0),
                ILPattern.OpCode(OpCodes.Br_S),
                ILPattern.OpCode(OpCodes.Ldloc_0)
            )
        ),
        ILPattern.Optional(OpCodes.Br_S),
        ILPattern.OpCode(OpCodes.Ret)
    );

    private static readonly ILPattern setter_pattern = ILPattern.Sequence(
        ILPattern.Optional(OpCodes.Nop),
        ILPattern.OpCode(OpCodes.Ldarg_0),
        ILPattern.Either(
            new FieldILPattern(ILPattern.OpCode(OpCodes.Stsfld)),
            ILPattern.Sequence(
                ILPattern.OpCode(OpCodes.Ldarg_1),
                new FieldILPattern(ILPattern.OpCode(OpCodes.Stfld))
            )
        ),
        ILPattern.OpCode(OpCodes.Ret)
    );

    public static FieldInfo? GetBackingField(this PropertyInfo propertyInfo) {
        var getter = propertyInfo.GetGetMethod(true);
        if (getter is not null)
            return GetBackingField(getter, getter_pattern);

        var setter = propertyInfo.GetSetMethod(true);
        if (setter is not null)
            return GetBackingField(setter, setter_pattern);

        return null;
    }

    private static FieldInfo? GetBackingField(MethodInfo methodInfo, ILPattern pattern) {
        var result = ILPattern.Match(methodInfo, pattern);
    }
}
