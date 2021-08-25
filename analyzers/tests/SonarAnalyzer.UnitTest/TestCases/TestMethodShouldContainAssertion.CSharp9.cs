namespace MicrosoftTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    class TestSuite
    {
        public void Method()
        {
            [TestMethod]
            void NestedTest() { } // Compliant - test methods must be public, this code does not work

            [DataTestMethod]
            void NestedDataTest() { } // Compliant - test methods must be public, this code does not work
        }
    }

    [TestClass]
    public partial class WithPartialMethods
    {
        [TestMethod]
        public partial void ThisHasNoAssert();

        [TestMethod]
        public partial void ThisInvokesSomethingWithAssert();
    }

    public partial class WithPartialMethods
    {
        public partial void ThisHasNoAssert()   // Noncompliant
        {
            DoNothing();
        }

        private void DoNothing() { }

        public partial void ThisInvokesSomethingWithAssert()
        {
            DoTheWork();
        }

        private void DoTheWork() =>
            Assert.AreEqual(true, true);
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
            void NestedTest() { } // Compliant - test methods must be public, this code does not work

            [TestCase(42)]
            void NestedTestCase() { } // Compliant - test methods must be public, this code does not work
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
            void NestedFact() { } // Noncompliant {{Add at least one assertion to this test case.}}

            [Fact]
            void NestedFactWithAssert() => Assert.True(true);

            [Theory]
            void NestedTheory() { } // Noncompliant

            [Theory]
            void NestedTheoryWithAssert()
            {
                Assert.True(true);
            }
        }
    }
}
