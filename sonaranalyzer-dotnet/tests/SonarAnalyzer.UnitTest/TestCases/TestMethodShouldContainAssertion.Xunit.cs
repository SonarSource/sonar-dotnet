namespace TestXunit
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class Program
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
            AssertSomething(42);
        }

        [Fact]
        public void Fact9()
        {
            ShouldSomething(42);
        }

        [Fact]
        public void Fact10()
        {
            ExpectSomething(42);
        }

        [Fact]
        public void Fact11()
        {
            MustSomething(42);
        }

        [Fact]
        public void Fact12()
        {
            VerifySomething(42);
        }

        [Fact]
        public void Fact13()
        {
            ValidateSomething(42);
        }

        [Fact]
        public void Fact14()
        {
            dynamic d = 10;
            Assert.Equal(d, 10.0);
        }

        [Fact(Skip = "reason")]
        public void Fact15() // Don't raise on skipped test methods
        {
        }

        [Theory]
        [InlineData(1)]
        public void Theory1(int arg1) // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^^^
        {
            var x = 42;
        }

        [Theory]
        [InlineData(1)]
        public void Theory2(int arg1)
        {
            Assert.Equal(42, arg1);
        }

        [Theory]
        [InlineData(1)]
        public void Theory3(int arg1)
        {
            Xunit.Assert.Equal(42, arg1);
        }

        [Theory]
        [InlineData(1)]
        public void Theory5(int arg1)
        {
            arg1.Should().Be(42);
        }

        [Theory]
        [InlineData(1)]
        public void Theory6(int arg1)
        {
            Action act = () => { throw new Exception(); };
            act.ShouldThrow<Exception>();
        }

        [Theory]
        [InlineData(1)]
        public void Theory7(int arg1)
        {
            Action act = () => { throw new Exception(); };
            act.ShouldNotThrow<Exception>();
        }

        [Theory]
        [InlineData(1)]
        public void Theory8(int arg1)
        {
            AssertSomething(arg1);
        }

        [Theory]
        [InlineData(1)]
        public void Theory9(int arg1)
        {
            ShouldSomething(arg1);
        }

        [Theory]
        [InlineData(1)]
        public void Theory10(int arg1)
        {
            ExpectSomething(arg1);
        }

        [Theory]
        [InlineData(1)]
        public void Theory11(int arg1)
        {
            MustSomething(arg1);
        }

        [Theory]
        [InlineData(1)]
        public void Theory12(int arg1)
        {
            VerifySomething(arg1);
        }

        [Theory]
        [InlineData(1)]
        public void Theory13(int arg1)
        {
            ValidateSomething(arg1);
        }

        [Theory]
        [InlineData(1)]
        public void Theory14(int arg1)
        {
            dynamic d = 10;
            Assert.Equal(d, 10.0);
        }

        [Theory(Skip = "reason")]
        [InlineData(1)]
        public void Theory15(int arg1) // Don't raise on skipped test methods
        {
        }

        public void AssertSomething(int arg1)
        {
            Assert.True(true);
        }

        public void ShouldSomething(int arg1)
        {
            Assert.True(true);
        }

        public void ExpectSomething(int arg1)
        {
            Assert.True(true);
        }

        public void MustSomething(int arg1)
        {
            Assert.True(true);
        }

        public void VerifySomething(int arg1)
        {
            Assert.True(true);
        }

        public void ValidateSomething(int arg1)
        {
            Assert.True(true);
        }
    }
}
