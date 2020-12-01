namespace MicrosoftTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    class TestSuite
    {
        public void Method()
        {
            [TestMethod]
            void NestedTest() { } // Compliant - FN, test is not run by the test runner

            [DataTestMethod]
            void NestedDataTest() { } // Compliant - FN, test is not run by the test runner
        }
    }
}

namespace NUnitTests
{
    using NUnit.Framework;

    [TestFixture]
    class TestSuite
    {
        public void Method()
        {
            [Test]
            void NestedTest() { } // Compliant - FN, test is not run by the test runner

            [TestCase(42)]
            void NestedTestCase() { } // Compliant - FN, test is not run by the test runner
        }
    }
}

namespace XUnitTests
{
    using Xunit;

    class TestSuite
    {
        public void Method()
        {
            [Fact]
            void NestedFact() { } // Compliant - FN, test is not run by the test runner

            [Theory]
            void NestedTheory() { } // Compliant - FN, test is not run by the test runner
        }
    }
}
