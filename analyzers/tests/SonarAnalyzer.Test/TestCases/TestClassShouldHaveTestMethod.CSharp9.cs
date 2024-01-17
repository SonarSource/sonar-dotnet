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

    [TestClass]
    record TestSuiteRecord1 // Noncompliant {{Add some tests to this record.}}
    {
        public void Method()
        {
        }
    }

    [TestClass]
    record TestSuiteRecord2
    {
        [TestMethod]
        public void Method()
        {
        }
    }

    [TestClass]
    record PositionalRecord(string SomeProperty)
    {
        [TestMethod]
        public void Method()
        {
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
