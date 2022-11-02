using System;

record IntPointers 
{
    public const IntPtr myConst = 0;  // Compliant
    public static IntPtr staticField = 0; // Noncompliant

    public IntPtr field1 = 0;         // Noncompliant
    public IntPtr field2 = 42;        // Compliant

    public UIntPtr field3 = 0;        // Noncompliant
    public UIntPtr field4 = 42;       // Compliant

    public IntPtr field5 = IntPtr.Zero;             // Noncompliant
    public IntPtr field6 = 0x0000000000000000;      // Noncompliant
    public UIntPtr field7 = UIntPtr.Zero;           // Noncompliant
    public IntPtr field8 = new IntPtr(0);           // Noncompliant
    public IntPtr field9 = new IntPtr(staticField); // Compliant

    public UIntPtr field10 = new UIntPtr(0);        // Noncompliant
    public UIntPtr field11 = new();                 // Noncompliant
    public UIntPtr field12 = new UIntPtr { };       // Noncompliant

    public IntPtr Property1 { get; set; } = 0;      // Noncompliant
    public IntPtr Property2 { get; set; } = 42;     // Compliant

    public UIntPtr Property3 { get; init; } = 0;    // Noncompliant
    public UIntPtr Property4 { get; init; } = 42;   // Compliant

    public IntPtr Property5 { get; set; } = 0 * 20 - 0; // FN - Expression is not evaluated
    public UIntPtr Property6 { get; set; } = 0 * 20 - 0; // FN - Expression is not evaluated
}
