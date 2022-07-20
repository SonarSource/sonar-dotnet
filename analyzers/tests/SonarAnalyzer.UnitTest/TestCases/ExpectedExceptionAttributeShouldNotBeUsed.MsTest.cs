using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    class Program
    {
        [Ignore][TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]  // Noncompliant
        public void TestFoo1()
        {
            var x = true;
            x.ToString();
        }

        [Ignore][TestMethod]
        [ExpectedException(typeof(ArgumentNullException))] // Compliant - one line
        public void TestFoo3()
        {
            new object().ToString();
        }

        [Ignore][TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public string TestFoo5() => new object().ToString();

        [Ignore][TestMethod]
        public void TestFoo7()
        {
            bool callFailed = false;
            try
            {
                //...
            }
            catch (ArgumentNullException)
            {
                callFailed = true;
            }
        }

        [Ignore][TestMethod]
        public void TestWithThrowsAssertation()
        {
            object o = new object();
            Assert.ThrowsException<ArgumentNullException>(() => o.ToString());
        }
    }
}
