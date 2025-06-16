using System;
using System.Diagnostics.CodeAnalysis;

record IntPointers
{
    public const IntPtr myConst = 0; // Compliant
    public static IntPtr staticField = 0; // Noncompliant

    public IntPtr field1 = 0; // Noncompliant
    public IntPtr field2 = 42; // Compliant

    public UIntPtr field3 = 0; // Noncompliant
    public UIntPtr field4 = 42;// Compliant

    public IntPtr field5 = IntPtr.Zero; // Noncompliant
    public IntPtr field6 = 0x0000000000000000; // Noncompliant
    public UIntPtr field7 = UIntPtr.Zero; // Noncompliant
    public IntPtr field8 = new IntPtr(0); // Noncompliant
    public IntPtr field9 = new IntPtr(staticField); // Compliant

    public UIntPtr field10 = new UIntPtr(0); // Noncompliant
    public UIntPtr field11 = new();  // Noncompliant
    public UIntPtr field12 = new UIntPtr { }; // Noncompliant

    public IntPtr Property1 { get; set; } = 0; // Noncompliant
    public IntPtr Property2 { get; set; } = 42; // Compliant

    public UIntPtr Property3 { get; init; } = 0; // Noncompliant
    public UIntPtr Property4 { get; init; } = 42; // Compliant

    public IntPtr Property5 { get; set; } = 0 * 20 - 0; // FN - Expression is not evaluated
    public UIntPtr Property6 { get; set; } = 0 * 20 - 0; // FN - Expression is not evaluated
}

public readonly struct FooStruct
{
    public FooStruct() { }

    public int Value { get; init; } // Compliant
    public bool BoolValue { get; init; } // Compliant

    public int ValueDefault { get; init; } = 0; // Noncompliant
    public bool BoolValueDefault { get; init; } = false; // Noncompliant
}

public struct BarStruct
{
    public int someField = 0; // Noncompliant Initializing this field is optional for C# 11 due to the auto-default-struct C# feature https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#auto-default-struct
    public required int someRequiredField = 0; // Noncompliant "required" indicates C# 11 and the compiler takes care of the initialization to the default value for structs in C# 11 and above
    public BarStruct(int dummy) { }

    [SetsRequiredMembers]
    public BarStruct() { } // this constructor will init all required members to their default values.
}

public struct FooBarStruct
{
    public int someField; // Compliant - does not raise a CS0171 issue anymore due to the auto-default-struct C# 11 feature https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#auto-default-struct
    public required int someRequiredField; // Compliant - does not raise a CS0171 and the compiler will default this to 0.

    public FooBarStruct(int dummy) { }
}

public class TestRequiredProperties
{
    void Method()
    {
        var classWithRequiredProperties = new ClassWithRequiredProperties() { RequiredProperty = 0 };
        var classWithRequiredProperties_initWithConstructor = new ClassWithRequiredProperties();
    }

    public class ClassWithRequiredProperties
    {
        public required int RequiredProperty { get; init; } = 0; // Noncompliant -  the required property is to be set on the caller's side anyways or by the constructor.
        public int AnotherProperty { get; set; }

        [SetsRequiredMembers]
        public ClassWithRequiredProperties() { }
    }
}
