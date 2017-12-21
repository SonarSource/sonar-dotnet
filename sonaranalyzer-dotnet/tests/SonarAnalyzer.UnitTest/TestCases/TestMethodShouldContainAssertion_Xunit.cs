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

        [Fact]
        public void Fact8()
        {
            AssertSomething();
        }

        [Fact]
        public void Fact9()
        {
            ShouldSomething();
        }

        [Fact]
        public void Fact10()
        {
            ExpectSomething();
        }

        [Fact]
        public void Fact11()
        {
            MustSomething();
        }

        [Fact]
        public void Fact12()
        {
            VerifySomething();
        }

        [Fact]
        public void Fact13()
        {
            ValidateSomething();
        }

        [Fact]
        public void Fact14()
        {
            dynamic d = 10;
            Assert.Equal(d, 10.0);
        }

        [Fact(Skip="reason")]
        public void Fact15() // Don't raise on skipped test methods
        {
        }

        [Theory]
        public void Theory1() // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^^^
        {
            var x = 42;
        }

        [Theory]
        public void Theory2()
        {
            var x = 42;
            Assert.Equal(x, 42);
        }

        [Theory]
        public void Theory3()
        {
            var x = 42;
            Xunit.Assert.Equal(x, 42);
        }

        [Theory]
        public void Theory5()
        {
            var x = 42;
            x.Should().Be(42);
        }

        [Theory]
        public void Theory6()
        {
            Action act = () => { throw new Exception(); };
            act.ShouldThrow<Exception>();
        }

        [Theory]
        public void Theory7()
        {
            Action act = () => { throw new Exception(); };
            act.ShouldNotThrow<Exception>();
        }

        [Theory]
        public void Theory8()
        {
            AssertSomething();
        }

        [Theory]
        public void Theory9()
        {
            ShouldSomething();
        }

        [Theory]
        public void Theory10()
        {
            ExpectSomething();
        }

        [Theory]
        public void Theory11()
        {
            MustSomething();
        }

        [Theory]
        public void Theory12()
        {
            VerifySomething();
        }

        [Theory]
        public void Theory13()
        {
            ValidateSomething();
        }

        [Theory]
        public void Theory14()
        {
            dynamic d = 10;
            Assert.Equal(d, 10.0);
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
