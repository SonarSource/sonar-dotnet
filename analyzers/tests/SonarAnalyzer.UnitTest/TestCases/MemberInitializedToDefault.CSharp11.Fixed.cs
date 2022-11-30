using System;
using System.Diagnostics.CodeAnalysis;

record IntPointers
{
    public const IntPtr myConst = 0; // Compliant
    public static IntPtr staticField; // Fixed

    public IntPtr field1; // Fixed
    public IntPtr field2 = 42; // Compliant

    public UIntPtr field3; // Fixed
    public UIntPtr field4 = 42;// Compliant

    public IntPtr field5; // Fixed
    public IntPtr field6; // Fixed
    public UIntPtr field7; // Fixed
    public IntPtr field8; // Fixed
    public IntPtr field9 = new IntPtr(staticField); // Compliant

    public UIntPtr field10; // Fixed
    public UIntPtr field11;  // Fixed
    public UIntPtr field12; // Fixed

    public IntPtr Property1 { get; set; } // Fixed
    public IntPtr Property2 { get; set; } = 42; // Compliant

    public UIntPtr Property3 { get; init; } // Fixed
    public UIntPtr Property4 { get; init; } = 42; // Compliant

    public IntPtr Property5 { get; set; } = 0 * 20 - 0; // FN - Expression is not evaluated
    public UIntPtr Property6 { get; set; } = 0 * 20 - 0; // FN - Expression is not evaluated
}

public readonly struct FooStruct
{
    public FooStruct() { }

    public int Value { get; init; } // Compliant
    public bool BoolValue { get; init; } // Compliant

    public int ValueDefault { get; init; } // Fixed
    public bool BoolValueDefault { get; init; } // Fixed
}

public struct BarStruct
{
    public int someField; // Fixed
    public required int someRequiredField; // Fixed
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
        public required int RequiredProperty { get; init; } // Fixed
        public int AnotherProperty { get; set; }

        [SetsRequiredMembers]
        public ClassWithRequiredProperties() { }
    }
}
