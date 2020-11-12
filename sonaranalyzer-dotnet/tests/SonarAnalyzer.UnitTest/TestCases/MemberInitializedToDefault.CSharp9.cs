
record NativeInts
{
    public const nint myConst = 0;  // Compliant
    public nint field1 = 0;         // FN
    public nint field2 = 42;
    public nuint field3 = 0;        // FN
    public nuint field4 = 42;

    public nint Property1 { get; set; } = 0;    // FN
    public nint Property2 { get; set; } = 42;

    public nuint Property3 { get; init; } = 0; // FN
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
}
