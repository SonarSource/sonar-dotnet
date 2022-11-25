using System;
using NUnit.Framework;
using MSTest = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    [TestFixture]
    public class NUnitClass
    {
        // False positive: using an MSTest Ignore against an NUnit test does nothing,
        // but we still raise an issue.
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
        public void Foo1()
        {
        }

        [Test]
        [NUnit.Framework.Ignore] // This test is ignored because 'blah blah'
        public void Foo2()
        {
        }

        [Test]
        [Ignore]
        [MSTest.WorkItem(1234)]
        public void Foo3()
        {
        }

        [TestCase("")]
        [Ignore()]
//       ^^^^^^^^ Noncompliant
        public void Foo4(string s)
        {
        }

        [TestCase("")]
        [NUnit.Framework.Ignore] // This test is ignored because 'blah blah'
        public void Foo5(string s)
        {
        }

        [TestCase("")]
        [Ignore]
        [MSTest.WorkItem(1234)]
        public void Foo6(string s)
        {
        }

        [TestCase("")]
        [DerivedIgnoreAttribute()]
//       ^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
        public void Foo7(string s)
        {
        }

        [TestCase("")]
        [DerivedIgnoreAttribute] // This test is ignored because 'blah blah'
        public void Foo8(string s)
        {
        }

        [TestCaseSource("DivideCases")]
        [Ignore]
//       ^^^^^^ Noncompliant
        public void DivideTest(int n, int d, int q)
        {
            Assert.AreEqual(q, n / d);
        }

        static object[] DivideCases =
        {
            new object[] { 12, 3, 4 }
        };

        [Datapoint]
        public int Data = 42;

        [Theory]
        [Ignore]
//       ^^^^^^ Noncompliant
        public void Theory1(int arg)
        {
        }

        [Theory]
        [Ignore] // some reason
        public void Theory2(int arg) // Compliant
        {
        }

        [Theory]
        [Ignore("a reason")]
        public void Theory3(int arg)
        {
        }
    }

    [TestFixture, Ignore]
    public class IgnoredClass1
    {
        [Test]
        public void TestInIgnoredClass()
        {
        }
    }

    [Ignore]
    public class IgnoredClass2 // No TestClass attribute
    {
    }

    [Ignore, TestFixture] // This test is ignored because 'blah blah'
    public class IgnoredClass3
    {
    }

    [Ignore("Ignored because reasons"), TestFixture]
    public class IgnoredClass4
    {
    }

    public class DerivedIgnoreAttribute : NUnit.Framework.IgnoreAttribute { }
}
