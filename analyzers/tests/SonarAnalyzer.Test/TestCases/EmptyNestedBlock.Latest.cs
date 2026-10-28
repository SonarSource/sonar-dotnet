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

public static class ExtensionIndexerSetter
{
    extension(System.Collections.Generic.List<int> list)
    {
        public int this[string key]
        {
            get => 0;
            set { } // Noncompliant
        }
    }
}

public class Labeled
{
    public void Method(int[] values)
    {
        outer:
        foreach (var value in values)
        {
            foreach (var other in values)
            {
                if (other == value)
                {
                    continue outer;
                }
                else
                { } // Noncompliant
            }
            break outer;
        }
    }
}
