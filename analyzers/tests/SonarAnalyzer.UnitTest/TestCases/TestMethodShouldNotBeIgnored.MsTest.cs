using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics.TestMethods
{
    [TestClass]
    public class MsTestClass_TestMethods
    {
        [TestMethod]
        [Ignore]
//       ^^^^^^ Noncompliant {{Either remove this 'Ignore' attribute or add an explanation about why this test is ignored.}}
        public void Foo1()
        {
        }

        [TestMethod]
        [Ignore] // This test is ignored because 'blah blah'
        public void Foo2()
        {
        }

        [Ignore, TestMethod] // This test is ignored because 'blah blah'
        public void Foo3()
        {
        }

        [TestMethod]
        [Ignore("Ignored because reasons")]
        public void Foo4()
        {
        }

        [TestMethod]
        [Ignore]
        [WorkItem(1234)]
        public void Foo5()
        {
        }
    }

    [TestClass]
    public class MsTestClass_DataTestMethods
    {
        [DataTestMethod]
        [Ignore]
//       ^^^^^^ Noncompliant {{Either remove this 'Ignore' attribute or add an explanation about why this test is ignored.}}
        public void Foo1()
        {
        }

        [DataTestMethod]
        [Ignore] // This test is ignored because 'blah blah'
        public void Foo2()
        {
        }

        [Ignore, DataTestMethod] // This test is ignored because 'blah blah'
        public void Foo3()
        {
        }

        [DataTestMethod]
        [Ignore("Ignored because reasons")]
        public void Foo4()
        {
        }

        [DataTestMethod]
        [Ignore]
        [WorkItem(1234)]
        public void Foo5()
        {
        }
    }

    [Ignore, TestClass]
//   ^^^^^^ Noncompliant
    public class MsTestClass1
    {
        [TestMethod]
        public void Test1()
        {
        }
    }

    [Ignore]
    public class MsTestClass2 // No TestClass attribute
    {
    }

    [Ignore, TestClass] // This test is ignored because 'blah blah'
    public class MsTestClass3
    {
    }

    [Ignore("Ignored because reasons"), TestClass]
    public class MsTestClass4
    {
    }
}

namespace DerivedAttributeTestCases
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [DerivedTestClassAttribute]
    class TestSuite
    {
        [DerivedTestMethodAttribute]
        [Ignore]
//       ^^^^^^ Noncompliant
        public void Foo1()
        {
        }

        [DerivedDataTestMethodAttribute]
        [Ignore]
//       ^^^^^^ Noncompliant
        public void Foo2()
        {
        }
    }

    public class DerivedTestClassAttribute : TestClassAttribute { }

    public class DerivedTestMethodAttribute : TestMethodAttribute { }

    public class DerivedDataTestMethodAttribute : DataTestMethodAttribute { }
}

