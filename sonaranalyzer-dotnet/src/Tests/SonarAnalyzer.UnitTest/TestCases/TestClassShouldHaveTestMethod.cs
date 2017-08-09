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
}
