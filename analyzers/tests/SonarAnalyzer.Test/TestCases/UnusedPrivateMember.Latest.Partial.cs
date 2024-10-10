namespace CSharp9
{
    public partial class PartialMethods
    {
        partial void UnusedMethod(); // Noncompliant

    }
}

namespace CSharp13
{
    public partial class PartialProperty
    {
        partial int PartialProp { get => 42; } // Noncompliant
    }
}
