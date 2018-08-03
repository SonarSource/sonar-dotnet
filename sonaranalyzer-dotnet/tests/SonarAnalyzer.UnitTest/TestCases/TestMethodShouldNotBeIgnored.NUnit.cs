using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace Tests.Diagnostics
{
    class NUnitClass
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
        public void Foo1()
        {
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore] // This test is ignored because 'blah blah'
        public void Foo2()
        {
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
        [WorkItem(1234)]
        public void Foo3()
        {
        }

        [TestCase("")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
        public void Foo4(string s)
        {
        }

        [TestCase("")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore] // This test is ignored because 'blah blah'
        public void Foo5(string s)
        {
        }

        [TestCase("")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
        [WorkItem(1234)]
        public void Foo6(string s)
        {
        }

        [TestCaseSource("DivideCases")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
        public void DivideTest(int n, int d, int q)
        {
            Assert.AreEqual(q, n / d);
        }

        static object[] DivideCases =
        {
            new object[] { 12, 3, 4 },
            new object[] { 12, 2, 6 },
            new object[] { 12, 4, 3 }
        };
    }
}
