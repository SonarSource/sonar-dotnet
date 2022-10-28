namespace MicrosoftTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [DerivedTest<int>]
    class TestSuite // Noncompliant {{Add some tests to this class.}}
    {
        public void Method()
        {
        }
    }

    public class DerivedTestAttribute<T> : TestClassAttribute
    {
    }
}
