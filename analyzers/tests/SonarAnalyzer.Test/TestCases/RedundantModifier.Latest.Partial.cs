namespace CSharp11
{
    file partial class PartialFooBar { } // Noncompliant
}


namespace CSharp13
{
    public sealed partial class Thingy
    {
        public override sealed partial int Value { get => 42; } // Noncompliant
    }
}
