﻿// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ValueParameterNotUsed

using System.Reflection;
using Mono.Cecil.Cil;
using Reaganism.MonoMix.Pattern;

namespace Reaganism.MonoMix.Tests.Pattern;

[TestFixture]
public static class MatchingTests {
    private static class StaticProperties_Success {
        public static object? AutoProperty_WithGetter_Unset { get; }

        public static object? AutoProperty_WithGetter_Set { get; } = new();

        public static object? AutoProperty_WithGetterAndSetter_Unset { get; set; }

        public static object? AutoProperty_WithGetterAndSetter_Set { get; set; } = new();

        public static object? Property_GetterToNamedBackingField => field;

        public static object? Property_SetterToNamedBackingField {
            set => field = value;
        }

        public static object? Property_GetterAndSetterToNamedBackingField {
            get => field;
            set => field = value;
        }

        private static object? field;
    }

    private static class StaticProperties_Failure {
        public static object? Property_WithEmptyGetter => null;

        public static object? Property_WithEmptySetter {
            set { }
        }

        public static object? Property_WithEmptyGetterAndSetter {
            get => null;
            set { }
        }
    }

    private sealed class InstanceProperties_Success {
        public object? AutoProperty_WithGetter_Unset { get; }

        public object? AutoProperty_WithGetter_Set { get; } = new();

        public object? AutoProperty_WithGetterAndSetter_Unset { get; set; }

        public object? AutoProperty_WithGetterAndSetter_Set { get; set; } = new();

        public object? Property_GetterToNamedBackingField => field;

        public object? Property_SetterToNamedBackingField {
            set => field = value;
        }

        public object? Property_GetterAndSetterToNamedBackingField {
            get => field;
            set => field = value;
        }

        private object? field;
    }

    private class InstanceProperties_Failure {
#pragma warning disable CA1822
        public object? Property_WithEmptyGetter => null;

        public object? Property_WithEmptySetter {
            set { }
        }

        public object? Property_WithEmptyGetterAndSetter {
            get => null;
            set { }
        }
#pragma warning restore CA1822
    }

    private sealed class FieldILPattern(ILPattern pattern) : ILPattern {
        public static readonly object FIELD_KEY = new();

        public override int MinimumLength => 1;

        public override bool Match(ILMatchContext ctx) {
            if (!pattern.Match(ctx))
                return false;

            var match = ctx.DirectionalPrevious;
            if (match?.Operand is not FieldInfo fieldInfo)
                throw new InvalidOperationException("Field instruction must have a field operand");

            if (ctx.TryGetData(FIELD_KEY, out var otherFieldInfo) && !ReferenceEquals(fieldInfo, otherFieldInfo))
                throw new InvalidOperationException("Field instruction must have the same field operand");

            ctx.AddData(FIELD_KEY, fieldInfo);
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

    // Be lazy and reuse the tests from BackingFieldResolverTests, but go back
    // and forth to ensure matching works in both directions.
    [TestCase(typeof(StaticProperties_Success), true)]
    [TestCase(typeof(StaticProperties_Failure), false)]
    [TestCase(typeof(InstanceProperties_Success), true)]
    [TestCase(typeof(InstanceProperties_Failure), false)]
    public static void TestResolveBackingFields(Type type, bool expectSuccessful) {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        foreach (var property in properties) {
            var backingField = property.GetBackingField();
            Assert.That(backingField, expectSuccessful ? Is.Not.Null : Is.Null);
        }
    }

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
        // For testing purposes, we duplicate code here; we search forward, then
        // repeat the search backward, then repeat it forward again (before
        // using the final result). This is done to ensure consistent behavior
        // in both directions.

        var instrs = InstructionProvider.FromMethodBaseAsSystem(methodInfo).ToList();

        {
            var ctx = new ILMatchContext(instrs.First());
            if (!ILPattern.Match(ctx, pattern).Successful)
                return null;

            if (!ctx.TryGetData(FieldILPattern.FIELD_KEY, out var fieldInfo) || fieldInfo is not FieldInfo)
                return null;
        }

        {
            var ctx = new ILMatchContext(instrs.Last(), ILPattern.Direction.Backward);
            if (!ILPattern.Match(ctx, pattern).Successful)
                return null;

            if (!ctx.TryGetData(FieldILPattern.FIELD_KEY, out var fieldInfo) || fieldInfo is not FieldInfo)
                return null;
        }

        {
            var ctx = new ILMatchContext(instrs.First());
            if (!ILPattern.Match(ctx, pattern).Successful)
                return null;

            if (!ctx.TryGetData(FieldILPattern.FIELD_KEY, out var fieldInfo) || fieldInfo is not FieldInfo theFieldInfo)
                return null;

            return theFieldInfo;
        }
    }
}
