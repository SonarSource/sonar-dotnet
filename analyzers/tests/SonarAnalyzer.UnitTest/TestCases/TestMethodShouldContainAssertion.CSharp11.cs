namespace MicrosoftTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    class TestSuite
    {
        [DerivedTestMethodAttribute<int>]
        public void TestMethod1() // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^^^^^^^
        {
            var x = 42;
        }
    }

    public class DerivedTestMethodAttribute<T> : TestMethodAttribute
    {
    }
}
