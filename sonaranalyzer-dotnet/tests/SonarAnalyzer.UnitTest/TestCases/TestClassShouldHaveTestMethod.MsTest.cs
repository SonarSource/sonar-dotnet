using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    [TestClass]
    class ClassTest2 // Noncompliant
//        ^^^^^^^^^^
    {
    }

    [TestClass]
    class ClassTest4 // Noncompliant
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

    [TestClass]
    class ClassTest9
    {
        [TestMethod]
        public void Foo() { }
    }

    [TestClass]
    class ClassTest12
    {
        [DataTestMethod]
        [DataRow(1)]
        public void Foo(int i) { }
    }

    [TestClass]
    public abstract class MyCommonCode1
    {
    }

    [TestClass]
    public class MySubCommonCode1 : MyCommonCode1 // Noncompliant
    {
    }

    public class MySubCommonCode11 : MyCommonCode1 // Compliant
    {
    }

    [TestClass]
    public abstract class TestFooBase
    {
        [TestMethod]
        public void Foo_WhenFoo_ExpectsFoo() { }
    }

    // See https://github.com/SonarSource/sonar-csharp/issues/1196
    [TestClass]
    public class TestSubFoo : TestFooBase // Compliant - base abstract and at least 1 test in base
    {

    }
}
