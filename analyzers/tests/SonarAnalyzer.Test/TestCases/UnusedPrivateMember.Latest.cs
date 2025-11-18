using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp13
{
    public partial class PartialProperty
    {
        partial int PartialProp { get; } //Noncompliant
    }
}

namespace CSharp12
{
    // https://github.com/SonarSource/sonar-dotnet/issues/8024
    public sealed record Line(decimal Field);

    public sealed class Repro
    {
        public Line[] GetLines() => CreateLines();

        private static Line[] CreateLines() => [new(0)];
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9652
    class Repro_9652
    {
        class Inner
        {
            public object? this[int test] => 0;
            public int this[int x, int y] => 0;
            public int this[int x, int y, int z] => 0;  // Noncompliant
            public int this[string a, string b] => 0;
            private int this[string test] => 0;
            private int this[double test] => 0;         // Noncompliant

            public int Method() => _ = this[""];
        }

        class AnotherInner
        {
            private static Inner inner = new();
            public int Method() => inner["a", "b"];
        }

        private int this[int test] => 0;

        public Repro_9652()
        {
            var inner = new Inner();
            _ = inner[0];
            _ = inner?[0];
            _ = inner![0];
            _ = inner!?[0];
            _ = inner[0].ToString();
            _ = inner?[0].ToString();
            _ = inner[0]?.ToString();
            _ = inner?[0]?.ToString();
            _ = inner![0].ToString();
            _ = inner[0]!.ToString();
            _ = inner![0]!.ToString();
            _ = inner!?[0]!?.ToString();
            _ = inner[0, 0];
            _ = this[0];
            _ = inner.Method();

            var anotherInner = new AnotherInner();
            _ = anotherInner.Method();
        }
    }
}

namespace CSharp11
{
    class UnusedPrivateMember
    {
        private interface MyInterface
        {
            static abstract void Method();
        }

        private class Class1 : MyInterface // Noncompliant {{Remove the unused private class 'Class1'.}}
        {
            public static void Method() { var x = 42; }
            public void Method1() { var x = Method2(); } // Noncompliant {{Remove the unused private method 'Method1'.}}
            public static int Method2() { return 2; }
        }
    }
}
namespace CSharp10
{
    public record struct RecordStruct
    {
        public RecordStruct() { }

        private int a = 42; // Noncompliant {{Remove the unused private field 'a'.}}

        private int b = 42;
        public int B() => b;

        private nint Value { get; init; } = 42;

        private nint UnusedValue { get; init; } = 0; // Compliant - properties with initializer are considered as used.

        public RecordStruct Create() => new() { Value = 1 };

        private interface IFoo // Noncompliant
        {
            public void Bar() { }
        }

        private record struct Nested(string Name, int CategoryId);

        public void UseNested()
        {
            Nested d = new("name", 2);
        }

        private record struct Nested2(string Name, int CategoryId);

        public void UseNested2()
        {
            _ = new Nested2("name", 2);
        }

        private record struct UnusedNested1(string Name, int CategoryId); // Noncompliant
//                            ^^^^^^^^^^^^^
        internal record struct UnusedNested2(string Name, int CategoryId); // Noncompliant
        public record struct UnusedNested3(string Name, int CategoryId);
        record struct UnusedNested4(string Name, int CategoryId); // Noncompliant

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
            static void Bar()
            { }

            [Obsolete]
            [NotExisting] // Error [CS0246]
                          // Error@-1 [CS0246]
            [LocalFunctionAttribute2]
            [LocalFunction]
            static void Quix()
            { }

            [Obsolete]
            static void ForCoverage() { }

            static void NoAttribute() { } // Noncompliant
        }
    }

    public record struct PositionalRecord(string Value)
    {
        private int a = 42; // Noncompliant
        private int b = 42;
        public int B() => b;

        private record struct UnusedNested(string Name, int CategoryId) { } // Noncompliant
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2752
    public class ReproIssue2752
    {
        private record struct PrivateRecordStruct
        {
            public PrivateRecordStruct() { }

            public uint part1 = 0; // Noncompliant FP. Type is communicated an external call.
        }

        [DllImport("user32.dll")]
        private static extern bool ExternalMethod(ref PrivateRecordStruct reference);
    }
}

namespace CSharp9
{
    public record Record
    {
        private int a; // Noncompliant {{Remove the unused private field 'a'.}}

        private int b;
        public int B() => b;

        private nint Value { get; init; }

        private nint UnusedValue { get; init; } // Noncompliant

        public Record Create() => new() { Value = 1 };

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
            static void Bar()
            { }

            [Obsolete]
            [NotExisting] // Error [CS0246]
                          // Error@-1 [CS0246]
            [LocalFunctionAttribute2]
            [LocalFunction]
            static void Quix()
            { }

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

    // https://github.com/SonarSource/sonar-dotnet/issues/9379
    public static class Repro_9379
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

namespace CustomDynamicallyAccessedMembersAttribute
{
    public class DynamicallyAccessedMembersAttribute : Attribute { }

    [DynamicallyAccessedMembers]
    public class UnusedClassMarkedWithCustomAttribute
    {
        private int privateField;
    }
}

namespace CSharp8
{
    public interface MyInterface1
    {
        public void Method1() { }
    }

    public class Class1
    {
        private interface MyInterface2 // Noncompliant
        {
            public void Method1() { }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3842
    public class ReproIssue3842
    {
        public void SomeMethod()
        {
            ForSwitchArm x = null;
            var result = (x) switch
            {
                null => 1,
                // normally, when deconstructing in a switch, we don't actually know the type
                (object x1, object x2) => 2
            };
            var y = new ForIsPattern();
            if (y is (string a, string b))
            { }
        }

        private sealed class ForSwitchArm
        {
            public void Deconstruct(out object a, out object b) { a = b = null; } // Compliant, Deconstruct methods are ignored
        }

        private sealed class ForIsPattern
        {
            public void Deconstruct(out string a, out string b) { a = b = null; } // Compliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8342
    public class Repro_8342
    {
        [Private1] private protected void APrivateProtectedMethod() { }
        [Public1, Private2] public void APublicMethodWithMultipleAttributes1() { }
        [Public1][Private2] public void APublicMethodWithMultipleAttributes2() { }

        private class Private1Attribute : Attribute { }
        private class Private2Attribute : Attribute { }
        private class Private3Attribute : Attribute { }  // Noncompliant
        public class Public1Attribute : Attribute { }    // Compliant: public
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/4102
namespace Repro1144
{

    public interface IMyService { }
    public interface ISomeExternalDependency { }

    public static class UIServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultMyService(this IServiceCollection services)
        {
            services.AddSingleton<IMyService, MyServiceSingleton>();
            services.AddScoped<IMyService, MyServiceScoped>();
            services.AddTransient<IMyService, MyServiceTransient>();

            return services;
        }

        private class MyServiceSingleton : IMyService
        {
            private readonly ISomeExternalDependency dependency;

            public MyServiceSingleton(ISomeExternalDependency dependency) => this.dependency = dependency; // Noncompliant FP
        }

        private class MyServiceScoped : IMyService
        {
            private readonly ISomeExternalDependency dependency;

            public MyServiceScoped(ISomeExternalDependency dependency) => this.dependency = dependency; // Noncompliant FP
        }

        private class MyServiceTransient : IMyService
        {
            private readonly ISomeExternalDependency dependency;

            public MyServiceTransient(ISomeExternalDependency dependency) => this.dependency = dependency; // Noncompliant FP
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6653
namespace Repro6653
{
    public class UsingCoalescingAssignmentOperator
    {
        private static string StaticField1;

        private string Field0_1;       // Noncompliant, never read nor written
        private string Field0_2 = "1"; // Noncompliant, written by initializer but not read
        private string Field0_3 = "1";
        private string Field1;
        private string Field2;
        private string Field3;
        private string Field4;
        private string Field5;
        private string Field6_1;
        private string Field6_2;
        private string Field6_3;
        private string Field7_1;
        private string Field7_2;
        private string Field7_3;
        private string Field8;
        private string Field9;
        private string Field10;
        private string Field11;
        private string[] Field12;
        private string Field13_1;
        private string Field13_2;
        private string Field14_1;
        private string Field14_2;
        private string Field15_1;
        private string Field15_2;

        public UsingCoalescingAssignmentOperator(string sPar) { }

        public string this[string sPar] => null;

        public void SomeMethod()
        {
            _ = Field0_3;
            _ = Field1 ??= "1";
            _ = Field2 ??= Field2;
            _ = Field3 ?? "1";
            _ = Field4 ?? (Field4 ??= "1");
            _ = Field5;
            _ = Field5 ??= Field5;
            _ = Field6_1 ??= Field6_2 ??= Field6_3;
            _ = Field7_1 ??= (Field7_2 ?? Field7_3);
            _ = "1" + (Field8 ??= "1");
            _ = OtherMethod(Field9 ??= "1");
            _ = this[Field10 ??= "1"];
            _ = this.Field11 ?? "1";
            _ = Field12[0] ??= "1";
            _ = Field13_1 ??= (Field13_2 += "1");
            _ = Field14_1 ??= (Field14_2 = "1");
            _ = Field15_1 ??= OtherMethod(Field15_2 = "1");
        }

        public static UsingCoalescingAssignmentOperator StaticMethod() => new UsingCoalescingAssignmentOperator(StaticField1 ??= "1");

        private string OtherMethod(string sPar) => null;
    }
}

namespace CSharp7
{
    public class ExpressionBodyProperties
    {
        private int aField;

        private int Property01
        {
            get => aField;
            set => aField = value; // Noncompliant
        }

        private int Property02
        {
            get => aField; // Noncompliant
            set => aField = value;
        }

        public void Method()
        {
            int x;

            x = Property01;
            Property02 = x;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2478
    public class ReproIssue2478
    {
        public void SomeMethod()
        {
            var (a, (barA, barB)) = new PublicDeconstructWithInnerType();

            var (_, _, c) = new PublicDeconstruct();

            var qix = new MultipleDeconstructors();
            object b;
            (a, b, c) = qix;

            (a, b) = ReturnFromMethod();

            (a, b) = new ProtectedInternalDeconstruct();

            (a, b, c) = new Ambiguous(); // Error [CS0121]
            (a, b) = new NotUsedDifferentArgumentCount();   // Error [CS1501, CS8129]
            (a, b) = new NotUsedNotVisible();               // Error [CS1061, CS8129]
        }

        internal void InternalMethod(InternalDeconstruct bar)
        {
            var (a, b) = bar;
        }

        private sealed class PublicDeconstructWithInnerType
        {
            public void Deconstruct(out object a, out InternalDeconstruct b) { a = b = null; }

            private void Deconstruct(out object a, out object b) { a = b = null; } // Compliant, Deconstruct methods are ignored
        }

        internal sealed class InternalDeconstruct
        {
            internal void Deconstruct(out object a, out object b) { a = b = null; }

            private void Deconstruct(out object a, out string b, out string c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
        }

        private class PublicDeconstruct
        {
            public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; }

            protected void Deconstruct(out string a, out string b, out string c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
            private void Deconstruct(out object a, out string b, out string c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
        }

        private sealed class MultipleDeconstructors
        {
            public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; }

            public void Deconstruct(out object a, out object b) // Compliant, Deconstruct methods are ignored
            {
                a = b = null;
            }
        }

        private class ProtectedInternalDeconstruct
        {
            protected internal void Deconstruct(out object a, out object b) { a = b = null; }

            protected internal void Deconstruct(out object a, out object b, out object c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
        }

        private class Ambiguous
        {
            public void Deconstruct(out string a, out string b, out string c) { a = b = c = null; }
            public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
        }

        private class NotUsedDifferentArgumentCount
        {
            public void Deconstruct(out string a, out string b, out string c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
            public void Deconstruct(out string a, out string b, out string c, out string d) { a = b = c = d = null; } // Compliant, Deconstruct methods are ignored
        }

        private class NotUsedNotVisible
        {
            protected void Deconstruct(out object a, out object b) { a = b = null; } // Compliant, Deconstruct methods are ignored
            private void Deconstruct(out string a, out string b) { a = b = null; } // Compliant, Deconstruct methods are ignored
        }

        public class InvalidDeconstruct
        {
            private void Deconstruct(object a, out object b, out object c) { b = c = a; } // Noncompliant
            private void Deconstruct() { } // Noncompliant

            private int Deconstruct(out object a, out object b, out object c) // Noncompliant
            {
                a = b = c = null;
                return 42;
            }
        }

        private ForMethod ReturnFromMethod() => null;
        private sealed class ForMethod
        {
            public void Deconstruct(out object a, out object b) { a = b = null; }
        }
    }

    public class ReproIssue2333
    {
        public void Method()
        {
            PrivateNestedClass x = new PrivateNestedClass();
            (x.ReadAndWrite, x.OnlyWriteNoBody, x.OnlyWrite) = ("A", "B", "C");
            var tuple = (x.ReadAndWrite, x.OnlyRead);
        }

        private class PrivateNestedClass
        {
            private string hasOnlyWrite;

            public string ReadAndWrite { get; set; }        // Setters are compliant, they are used in tuple assignment
            public string OnlyWriteNoBody { get; set; }     // Compliant, we don't raise on get without body

            public string OnlyRead
            {
                get;
                set;    // Noncompliant
            }

            public string OnlyWrite
            {
                get => hasOnlyWrite;    // Noncompliant
                set => hasOnlyWrite = value;
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2752
    public class ReproIssue2752
    {
        private struct PrivateStructRef
        {
            public uint part1; // Noncompliant FP. Type is communicated an external call.
        }

        private class PrivateClassRef
        {
            public uint part1; // Noncompliant FP. Type is communicated an external call.
        }

        [DllImport("user32.dll")]
        private static extern bool ExternalMethodWithStruct(ref PrivateStructRef reference);

        [DllImport("user32.dll")]
        private static extern bool ExternalMethodWithClass(ref PrivateClassRef reference);
    }

    public class EmptyCtor
    {
        // That's invalid syntax, but it is still empty ctor and we should not raise for it, even if it is not used
        public EmptyCtor() => // Error [CS1525,CS1002]
}

    public class WithEnums
    {
        private enum X // Noncompliant
        {
            A
        }

        public void UseEnum()
        {
            var b = Y.B;
        }

        private enum Y
        {
            A,
            B
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/6724
    public class Repro_6724
    {
        public int PrivateGetter { private get; set; } // Noncompliant
        public int PrivateSetter { get; private set; } // Noncompliant

        public int ExpressionBodiedPropertyWithPrivateGetter { private get => 1; set => _ = value; } // Noncompliant
        public int ExpressionBodiedPropertyWithPrivateSetter { get => 1; private set => _ = value; } // Noncompliant
    }
}

// https://sonarsource.atlassian.net/browse/NET-675
namespace Repro_NET675
{
    internal readonly record struct RecordStruct
    {
        public string Value { get; }

        private RecordStruct(string value) => Value = value; // Compliant

        public static void Create(string value, out RecordStruct? result)
        {
            result = new(value);
        }
    }

    internal struct MyStruct
    {
        public string Value { get; }

        private MyStruct(string value) => Value = value; // Compliant

        public static void Create(string value, out MyStruct? result)
        {
            result = new(value);
        }
    }

    internal record class RecordClass
    {
        public string Value { get; }

        private RecordClass(string value) => Value = value; // Compliant

        public static void Create(string value, out RecordClass? result)
        {
            result = new(value);
        }
    }

    internal class MyClass
    {
        public string Value { get; }

        private MyClass(string value) => Value = value; // Compliant

        public static void Create(string value, out MyClass? result)
        {
            result = new(value);
        }
    }
}

public static class Extensions // Repro for https://sonarsource.atlassian.net/browse/NET-2644
{
    extension(List<string> strings)
    {
        public void ExtensionMethods()
        {
            PrivateExtensionAsMethod(strings);
            PrivateStaticExtensionAsMethod();

            strings.PrivateExtensionAsExtension();
            List<string>.PrivateStaticExtensionAsExtension();
        }

        private string PrivateExtensionAsMethod() => "a";                 // Noncompliant FP
        private static string PrivateStaticExtensionAsMethod() => "a";    // Noncompliant FP

        private string PrivateExtensionAsExtension() => "a";
        private static string PrivateStaticExtensionAsExtension() => "a";

    }
}
