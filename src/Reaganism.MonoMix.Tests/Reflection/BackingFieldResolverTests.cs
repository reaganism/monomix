// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ValueParameterNotUsed

using System.Reflection;
using Reaganism.MonoMix.Reflection;

namespace Reaganism.MonoMix.Tests.Reflection;

[TestFixture]
public static class BackingFieldResolverTests {
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
}
