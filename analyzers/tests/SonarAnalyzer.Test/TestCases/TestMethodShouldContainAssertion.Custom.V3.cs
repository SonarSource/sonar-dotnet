using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomTests
{
    using TestFramework.Attributes;

    [TestClass]
    public class BaseTest
    {
        [TestMethod]
        [DerivedExpectedException]
        public void TestMethod8() // Compliant
        {
            var x = 42;
        }
    }
}

namespace TestFramework.Attributes
{
    public class DerivedExpectedExceptionAttribute : ExpectedExceptionBaseAttribute { protected override void Verify(Exception exception) { } }
}
