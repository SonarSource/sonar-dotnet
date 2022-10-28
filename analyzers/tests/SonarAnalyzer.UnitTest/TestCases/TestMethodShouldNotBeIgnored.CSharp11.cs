namespace MicrosoftTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [DerivedTestClassAttribute<int>]
    class TestSuite
    {
        [DerivedTestMethodAttribute<int>]
        [Ignore] // FN
        public void Foo1()
        {
        }

        [DerivedDataTestMethodAttribute<int>]
        [Ignore] // FN
        public void Foo2()
        {
        }
    }

    public class DerivedTestClassAttribute<T> : TestClassAttribute { }

    public class DerivedTestMethodAttribute<T> : TestMethodAttribute { }

    public class DerivedDataTestMethodAttribute<T> : DataTestMethodAttribute { }
}
