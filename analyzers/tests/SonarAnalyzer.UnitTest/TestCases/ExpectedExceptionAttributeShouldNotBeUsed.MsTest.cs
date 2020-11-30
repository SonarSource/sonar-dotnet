using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    class Program
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]  // Noncompliant
        public void TestFoo1()
        {
            var x = true;
            x.ToString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public void TestFoo3()
        {
            new object().ToString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public string TestFoo5() => new object().ToString();

        [TestMethod]
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
    }
}
