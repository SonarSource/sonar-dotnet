using System;
using NUnit.Framework;

namespace Tests.Diagnostics
{
    [TestFixture]
    class ClassTest1 // Noncompliant
//        ^^^^^^^^^^
    {
    }

    [TestFixture]
    class ClassTest3 // Noncompliant
    {
        public void Foo() { }
    }

    class ClassTest5
    {
        public void Foo() { }
    }

    class ClassTest6
    {
        public void Foo() { }
    }

    [TestFixture]
    class ClassTest7
    {
        [Test]
        public void Foo() { }
    }

    [TestFixture]
    class ClassTest8
    {
        [TestCase("")]
        [TestCase("test")]
        public void Foo(string a) { }
    }

    [TestFixture]
    class ClassTest10
    {
        [Theory]
        public void Foo() { }
    }

    [TestFixture]
    public abstract class MyCommonCode2
    {
    }

    [TestFixture]
    public class MySubCommonCode2 : MyCommonCode2 // Noncompliant
    {
    }

    public class MySubCommonCode22 : MyCommonCode2 // Compliant
    {
    }

    [TestFixture]
    public class ClassTest11
    {
        [TestCaseSource("DivideCases")]
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

    [TestFixture]
    public abstract class TestFooBase
    {
        [Test]
        public void Foo_WhenFoo_ExpectsFoo() { }
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/1196
    [TestFixture]
    public class TestSubFoo : TestFooBase // Compliant - base abstract and at least 1 test in base
    {

    }
}

namespace DerivedAttributes
{

    public class DerivedTestFixtureAttribute : TestFixtureAttribute
    {

    }

    public class DerivedTestCaseAttribute : TestCaseAttribute
    {

    }

    public class DerivedTheoryAttribute : TheoryAttribute
    {

    }

    [DerivedTestFixtureAttribute]
    class DerivedClassAttribute // Noncompliant
//        ^^^^^^^^^^^^^^^^^^^^^
    {
    }

    [DerivedTestFixtureAttribute]
    class DerivedMethodAttribute1
    {
        [DerivedTestCaseAttribute]
        public void Foo() { }
    }

    [DerivedTestFixtureAttribute]
    class DerivedMethodAttribute2
    {
        [DerivedTheoryAttribute]
        public void Foo() { }
    }
}
