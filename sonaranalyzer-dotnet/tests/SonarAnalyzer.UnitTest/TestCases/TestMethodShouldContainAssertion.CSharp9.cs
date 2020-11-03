namespace MicrosoftTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    class TestSuite
    {
        public void Method()
        {
            [TestMethod]
            void NestedTest() { } // Compliant - FN, there is no assert

            [DataTestMethod]
            void NestedDataTest() { } // Compliant - FN, there is no assert
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
            void NestedTest() { } // Compliant - FN, there is no assert

            [TestCase(42)]
            void NestedTestCase() { } // Compliant - FN, there is no assert
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
            void NestedFact() { } // Compliant - FN, there is no assert

            [Theory]
            void NestedTheory() { } // Compliant - FN, there is no assert
        }
    }
}
