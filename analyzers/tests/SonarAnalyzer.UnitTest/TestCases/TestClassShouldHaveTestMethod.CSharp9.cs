namespace MicrosoftTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    class TestSuite // Noncompliant {{Add some tests to this class.}}
    {
        public void Method()
        {
            [TestMethod]
            void NestedTest() { }

            [DataTestMethod]
            void NestedDataTest() { }
        }
    }
}

namespace NUnitTests
{
    using NUnit.Framework;

    [TestFixture]
    class TestSuite // Noncompliant {{Add some tests to this class.}}
    {
        public void Method()
        {
            [Test]
            void NestedTest() { }

            [TestCase(42)]
            void NestedTestCase() { }
        }
    }
}
