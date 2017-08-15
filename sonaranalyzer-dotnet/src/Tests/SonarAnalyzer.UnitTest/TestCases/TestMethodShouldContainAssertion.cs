namespace TestMsTest
{
    using System;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    class Program
    {
        [TestMethod]
        public void TestMethod1() // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^^^^^^^
        {
            var x = 42;
        }

        [TestMethod]
        public void TestMethod2()
        {
            var x = 42;
            Assert.AreEqual(x, 42);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var x = 42;
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(x, 42);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestMethod4()
        {
            var x = System.IO.File.Open("", System.IO.FileMode.Open);
        }

        [TestMethod]
        public void TestMethod5()
        {
            var x = 42;
            x.Should().Be(42);
        }

        [TestMethod]
        public void TestMethod6()
        {
            Action act = () => { throw new Exception(); };
            act.ShouldThrow<Exception>();
        }

        [TestMethod]
        public void TestMethod7()
        {
            Action act = () => { throw new Exception(); };
            act.ShouldNotThrow<Exception>();
        }
    }
}

namespace TestNUnit
{
    using System;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    class Program
    {
        [Test]
        public void Test1() // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^
        {
            var x = 42;
        }

        [Test]
        public void Test2()
        {
            var x = 42;
            Assert.AreEqual(x, 42);
        }

        [Test]
        public void Test3()
        {
            var x = 42;
            NUnit.Framework.Assert.AreEqual(x, 42);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void Test4()
        {
            var x = System.IO.File.Open("", System.IO.FileMode.Open);
        }

        [Test]
        public void Test5()
        {
            var x = 42;
            x.Should().Be(42);
        }

        [Test]
        public void Test6()
        {
            Action act = () => { throw new Exception(); };
            act.ShouldThrow<Exception>();
        }

        [Test]
        public void Test7()
        {
            Action act = () => { throw new Exception(); };
            act.ShouldNotThrow<Exception>();
        }
    }
}

namespace TestXunit
{
    using System;
    using FluentAssertions;
    using Xunit;

    class Program
    {
        [Fact]
        public void Fact1() // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^
        {
            var x = 42;
        }

        [Fact]
        public void Fact2()
        {
            var x = 42;
            Assert.Equal(x, 42);
        }

        [Fact]
        public void Fact3()
        {
            var x = 42;
            Xunit.Assert.Equal(x, 42);
        }

        [Fact]
        public void Fact5()
        {
            var x = 42;
            x.Should().Be(42);
        }

        [Fact]
        public void Fact6()
        {
            Action act = () => { throw new Exception(); };
            act.ShouldThrow<Exception>();
        }

        [Fact]
        public void Fact7()
        {
            Action act = () => { throw new Exception(); };
            act.ShouldNotThrow<Exception>();
        }
    }
}