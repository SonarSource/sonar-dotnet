using System;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public record Record
    {
        private int a; // Noncompliant {{Remove the unused private field 'a'.}}

        private int b;
        public int B() => b;

        private nint Value { get; init; }

        private nint UnusedValue { get; init; } // Noncompliant

        public Record Create() => new() {Value = 1};

        private interface IFoo // Noncompliant
        {
            public void Bar() { }
        }

        private record Nested(string Name, int CategoryId);

        public void UseNested()
        {
            Nested d = new("name", 2);
        }

        private record Nested2(string Name, int CategoryId);

        public void UseNested2()
        {
            _ = new Nested2("name", 2);
        }

        private record UnusedNested1(string Name, int CategoryId); // Noncompliant
//                     ^^^^^^^^^^^^^

        internal record UnusedNested2(string Name, int CategoryId); // Noncompliant
        public record UnusedNested3(string Name, int CategoryId);

        private int usedInPatternMatching = 1;

        public int UseInPatternMatching(int val) =>
            val switch
            {
                < 0 => usedInPatternMatching,
                >= 0 => 1
            };

        private class LocalFunctionAttribute : Attribute { }
        private class LocalFunctionAttribute2 : Attribute { }

        public void Foo()
        {
            [LocalFunction]
            static void Bar() { }

            [Obsolete]
            [NotExisting] // Error [CS0246]
                          // Error@-1 [CS0246]
            [LocalFunctionAttribute2]
            [LocalFunction]
            static void Quix() { }

            [Obsolete]
            static void ForCoverage() { }

            static void NoAttribute() { } // Noncompliant
        }
    }

    public record PositionalRecord(string Value)
    {
        private int a; // Noncompliant
        private int b;
        public int B() => b;

        private record UnusedNested(string Name, int CategoryId) { } // Noncompliant
    }

    public class TargetTypedNew
    {
        private TargetTypedNew(int arg)
        {
            var x = arg;
        }

        private TargetTypedNew(string arg)                           // Noncompliant
        {
            var x = arg;
        }

        public static TargetTypedNew Create()
        {
            return new(42);
        }

        public static void Foo()
        {
            PositionalRecord @record = new PositionalRecord("");
        }
    }

    public partial class PartialMethods
    {
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2752
    public class ReproIssue2752
    {
        private record PrivateRecordRef
        {
            public uint part1; // Noncompliant FP. Type is communicated an external call.
        }

        [DllImport("user32.dll")]
        private static extern bool ExternalMethod(ref PrivateRecordRef reference);
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7904
    // In record types PrintMembers is used by the runtime to create a string representation of the record.
    // See also https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#printmembers-formatting-in-derived-records
    public class Repro_7904
    {
        public record BaseRecord(int Value);

        public record NoBaseRecord
        {
            protected virtual bool PrintMembers(StringBuilder builder) => true;     // Compliant
        }

        public record HasBaseRecord() : BaseRecord(42)
        {
            protected override bool PrintMembers(StringBuilder builder) => true;    // Compliant
        }

        public sealed record SealedWithNoBaseRecord
        {
            private bool PrintMembers(StringBuilder builder) => true;               // Compliant
        }

        public sealed record SealedWithBaseRecord() : BaseRecord(42)
        {
            protected override bool PrintMembers(StringBuilder builder) => true;   // Compliant
        }

        public sealed record NonMatchingPrintMembers
        {
            private int PrintMembers(int arg) => 42;                    // Noncompliant - different return type
            private bool PrintMembers() => false;                       // Noncompliant - different parameter list
            private bool PrintMembers(string arg) => false;             // Noncompliant - different parameter list

            public bool MethodWithLocalFunction()
            {
                return false;

                bool PrintMembers(StringBuilder builder) => true;       // Noncompliant
            }
        }

        public class ClassWithPrintMembers
        {
            private bool PrintMembers(StringBuilder builder) => true;   // Noncompliant - not a record
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9379
namespace Repro_9379
{
    public static class Program
    {
        public static void Method()
        {
            var instance = CreateInstance<ClassInstantiatedThroughReflection>();
            var instance2 = CreateInstance(typeof(AnotherClassInstantiatedThroughReflection));

            A classViaReflection = new();
            InitValue(classViaReflection);
        }

        public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() =>
            (T)Activator.CreateInstance(typeof(T), 42);

        public static object CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type) =>
            Activator.CreateInstance(type, 42);

        public static void InitValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T value) { }

        private class A
        {
            private bool a = true; // Noncompliant FP: type argument is inferred and ArgumentList is not present on syntax level (see line 183)
        }

        class C<T>
        {
            private int usedByReflection;                       // Noncompliant - FP
            C<T> Create() => Program.CreateInstance<C<T>>();    // Noncompliant - FP
        }

        private class ClassInstantiatedThroughReflection
        {
            private const int PrivateConst = 42;                // Noncompliant - FP
            private int privateField;                           // Noncompliant - FP
            private int PrivateProperty { get; set; }           // Noncompliant - FP
            private void PrivateMethod() { }                    // Noncompliant - FP
            private ClassInstantiatedThroughReflection() { }
            private event EventHandler PrivateEvent;            // Noncompliant - FP

            public ClassInstantiatedThroughReflection(int arg)
            {
            }

            private class NestedType                            // Noncompliant - FP
            {
                private int privateField;                       // Noncompliant - FP
            }
        }

        private class AnotherClassInstantiatedThroughReflection
        {
            private int privateField;                           // Noncompliant - FP
        }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private class TypeDecoratedWithDynamicallyAccessedMembers
        {
            private const int PrivateConst = 42;
            private int privateField;
            private int PrivateProperty { get; set; }
            private void PrivateMethod() { }
            public TypeDecoratedWithDynamicallyAccessedMembers() { }
            private event EventHandler PrivateEvent;

            private class NestedType
            {
                private const int PrivateConst = 42;
                private int privateField;
                private int PrivateProperty { get; set; }
                private void PrivateMethod() { }
                private NestedType() { }
                private event EventHandler PrivateEvent;
            }
        }

        private class MembersDecoratedWithAttribute                             // Noncompliant
        {
            private const int PrivateConst = 42;                                // Noncompliant
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields)] private const int PrivateConstWithAttribute = 42;

            private int privateField;                                           // Noncompliant
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields)] private int privateFieldWithAttribute;

            private int PrivateProperty { get; set; }                           // Noncompliant
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicProperties)] private int PrivatePropertyWithAttribute { get; set; }

            private void PrivateMethod() { }                                    // Noncompliant
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)] private void PrivateMethodWithAttribute() { }

            private class NestedType { }                                        // Noncompliant
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicNestedTypes)] private class NestedTypeWithAttribute { }

            [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]
            private int PrivateMethodWithReturn() { return 0; } // Noncompliant

            [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]
            private PrivateClass PrivateMethodWithReturnCustomClass() { return null; } // Noncompliant FP
        }

        private class PrivateClass { }

        private class ArgumentsDecoratedWithAttribute   // Noncompliant
        {
            public void PublicMethod() => Method(null); // Noncompliant

            private void Method([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] NestedType arg) { }

            private class NestedType
            {
                private NestedType(int arg) { }
            }

            public record RecordWithAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields)] string NotUnused); // Noncompliant
        }

        private class DerivedFromTypeDecoratedWithDynamicallyAccessedMembers : TypeDecoratedWithDynamicallyAccessedMembers  // Noncompliant
        {
            private int privateField;                                                                                       // Noncompliant - [DynamicallyAccessedMembers] attribute is not inherited
        }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        private class DynamicallyAccessedMembersOnlyForConstructors
        {
            private int privateField; // FN - only public constructors are used are indicated to be used through reflection, not fields
                                      // The analyzer assumens when the [DynamicallyAccessedMembers] attribute is used, then all members are used through reflection
        }
    }
}
