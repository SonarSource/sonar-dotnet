namespace MicrosoftTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [DerivedTestClassAttribute<int>]
    class TestSuite
    {
        public void Method()
        {
            [DerivedTestMethodAttribute<int>]
            void NestedTest() { } // Noncompliant

            [DerivedDataTestMethodAttribute<int>]
            void NestedDataTest() { } // Noncompliant
        }
    }

    public class DerivedTestClassAttribute<T> : TestClassAttribute { }

    public class DerivedTestMethodAttribute<T>: TestMethodAttribute { }

    public class DerivedDataTestMethodAttribute<T> : DataTestMethodAttribute { }
}
