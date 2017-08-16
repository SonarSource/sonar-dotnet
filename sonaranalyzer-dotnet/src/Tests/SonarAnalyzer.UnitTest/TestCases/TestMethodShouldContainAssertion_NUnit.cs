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

        [Test]
        public void Test8()
        {
            AssertSomething();
        }

        [Test]
        public void Test9()
        {
            ShouldSomething();
        }

        [Test]
        public void Test10()
        {
            ExpectSomething();
        }

        [Test]
        public void Test11()
        {
            MustSomething();
        }

        [Test]
        public void Test12()
        {
            VerifySomething();
        }

        [Test]
        public void Test13()
        {
            ValidateSomething();
        }

        [Test]
        public void Test14()
        {
            dynamic d = 10;
            Assert.AreEqual(d, 10.0);
        }

        [Test]
        [Ignore("Some reason")]
        public void Test15() // Don't raise on skipped test methods
        {
        }

        public void AssertSomething()
        {
            Assert.IsTrue(true);
        }

        public void ShouldSomething()
        {
            Assert.IsTrue(true);
        }

        public void AssertSomething()
        {
            Assert.IsTrue(true);
        }

        public void ExpectSomething()
        {
            Assert.IsTrue(true);
        }

        public void MustSomething()
        {
            Assert.IsTrue(true);
        }

        public void VerifySomething()
        {
            Assert.IsTrue(true);
        }

        public void ValidateSomething()
        {
            Assert.IsTrue(true);
        }
    }
}