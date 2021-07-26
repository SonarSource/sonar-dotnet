record NativeInts
{
    public const nint myConst = 0;  // Compliant
    public nint field1 = 0;         // Noncompliant
    public nint field2 = 42;
    public nuint field3 = 0;        // Noncompliant
    public nuint field4 = 42;

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
