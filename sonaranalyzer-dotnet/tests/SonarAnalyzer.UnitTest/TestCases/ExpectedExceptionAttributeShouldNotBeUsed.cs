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

        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]  // Noncompliant
        public void TestFoo2()
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

        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public void TestFoo4()
        {
            new object().ToString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public void TestFoo5 => new object().ToString();
        }

        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public void TestFoo6 => new object().ToString();

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

        [Test]
        public void TestFoo8()
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
