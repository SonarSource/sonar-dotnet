namespace MicrosoftTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    class TestSuite
    {
        public void Method()
        {
            [TestMethod, Ignore]
//                       ^^^^^^ Noncompliant {{Either remove this 'Ignore' attribute or add an explanation about why this test is ignored.}}
// Currently nested tests are not identified by test runner so they are skipped by default.
            void NestedTest()
            {
            }
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
            [Test, Ignore("reason")]
// Currently nested tests are not identified by test runner so they are skipped by default. We are not consistent since we raise in case of MsTest.
            void NestedTest()
            {
            }
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
            [Fact(Skip = "Reason")]
// Currently nested tests are not identified by test runner so they are skipped by default. We are not consistent since we raise in case of MsTest.
            void NestedTest()
            {
            }
        }
    }
}
