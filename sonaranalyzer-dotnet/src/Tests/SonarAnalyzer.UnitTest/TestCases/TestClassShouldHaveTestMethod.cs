using System;
using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    [TestFixture]
    class ClassTest1 // Noncompliant
//        ^^^^^^^^^^
    {
    }

    [TestClass]
    class ClassTest2 // Noncompliant
    {
    }

    [TestFixture]
    class ClassTest3 // Noncompliant
    {
        public void Foo() { }
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

    [TestClass]
    class ClassTest9
    {
        [TestMethod]
        public void Foo() { }
    }

    [TestFixture]
    class ClassTest10
    {
        [Theory]
        public void Foo() { }
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
}
