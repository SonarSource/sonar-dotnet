using System;

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

public partial class PartialEvents
{
    private partial event EventHandler Compliant { add { compliant += value; } remove { compliant -= value; } } // Compliant https://sonarsource.atlassian.net/browse/NET-2825
}
