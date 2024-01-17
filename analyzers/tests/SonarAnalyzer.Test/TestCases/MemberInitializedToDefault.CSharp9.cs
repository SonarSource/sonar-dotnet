using System;

record NativeInts
{
    public const nint myConst = 0;  // Compliant
    public static nint staticField = 0; // Noncompliant
    public nint field1 = 0;         // Noncompliant
    public nint field2 = 42;
    public nuint field3 = 0;        // Noncompliant
    public nuint field4 = 42;
    public nint field5 = IntPtr.Zero;         // Noncompliant
    public nint field6 = 0x0000000000000000;  // Noncompliant
    public nuint field7 = UIntPtr.Zero;       // Noncompliant
    public nint field8 = new IntPtr(0);       // Noncompliant
    public nint field9 = new IntPtr(staticField);
    public nuint field10 = new UIntPtr(0);    // Noncompliant
    public nuint field11 = new ();           // Noncompliant
    public nuint field12 = new nuint { };    // Noncompliant

    public nint Property1 { get; set; } = 0;    // Noncompliant
    public nint Property2 { get; set; } = 42;

    public nuint Property3 { get; init; } = 0; // Noncompliant
    public nuint Property4 { get; init; } = 42;

    public nint Property5
    {
        get
        {
            return 0;
        }
        init
        {
            field1 = 0;     // Not tracked
        }
    }

    public nuint Property6
    {
        get => 0;
        init => field1 = 0; // Not tracked
    }

    public nuint Property7
    {
        get => 0;
        init => field1 = 0; // Not tracked
    }

    public nint Property8 { get; set; } = 0 * 20 - 0; // FN - Expression is not evaluated
}
