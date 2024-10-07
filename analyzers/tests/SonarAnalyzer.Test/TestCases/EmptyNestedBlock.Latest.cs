public struct S
{
    public S() // Compliant - This will replace the default constructor
    {
    }
}

public partial class PartialProperty
{
    private partial int Property_01 { set; }
}

public partial class PartialProperty
{
    private partial int Property_01 { set { } } // Noncompliant
}
