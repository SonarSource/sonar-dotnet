partial class PartialProperties
{
    public partial int[,] MultiDimArrayProperty { get; }            // Noncompliant
    public partial int[,] this[int index] { get; }                  // Noncompliant
}

partial class PartialProperties
{
    public partial int[,] MultiDimArrayProperty { get => null; }    // Noncompliant
    public partial int[,] this[int index] { get => null; }          // Noncompliant
}
