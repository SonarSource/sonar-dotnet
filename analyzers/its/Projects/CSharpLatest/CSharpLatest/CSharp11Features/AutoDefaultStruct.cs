namespace CSharpLatest.CSharp11Features;

internal class AutoDefaultStruct
{
    public readonly struct Measurement
    {
        public Measurement(double value)
        {
            Value = value;
        }

        public Measurement(double value, string description)
        {
            Value = value;
            Description = description;
        }

        public Measurement(string description)
        {
            Description = description;
        }

        public double Value { get; init; }

        public string Description { get; init; } = "Ordinary measurement";

        public override string ToString() => $"{Value} ({Description})";
    }

    public static void Method()
    {
        var m1 = new Measurement(5);
        Console.WriteLine(m1);  // output: 5 (Ordinary measurement)

        var m2 = new Measurement();
        Console.WriteLine(m2);  // output: 0 ()

        var m3 = default(Measurement);
        Console.WriteLine(m3);  // output: 0 ()
    }
}
