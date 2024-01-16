namespace TestXunit
{
    using System;
    using FluentAssertions;
    using NFluent;
    using NSubstitute;
    using Shouldly;
    using Xunit;

    using static Xunit.Assert;

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
        public void Fact8() => AssertSomething(42);

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

        [Fact]
        public void Fact15()
        {
            var x = 42;
            Equal(x, 42);
        }

        [Fact]
        public void Fact16()
        {
            throw new Xunit.Sdk.XunitException("You failed me!");
        }

        [Fact(Skip = "reason")]
        public void Fact17() // Don't raise on skipped test methods
        {
        }

        [Fact]
        public void Fact18()
        {
            Check.ThatCode(() => 42).WhichResult().IsStrictlyPositive();
        }

        [Fact]
        public void Fact19()
        {
            throw new NFluent.Kernel.FluentCheckException("You failed me!");
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

    /// <summary>
    /// The NSubstitute and Shoudly assertions are extensively verified in the NUnit test files.
    /// Here we just do a simple test to confirm that the errors are not raised in conjunction with XUnit.
    /// </summary>
    public class NSubstituteAndShouldlyTests
    {
        [Fact]
        public void Received() => Substitute.For<IDisposable>().Received().Dispose();

        [Fact]
        public void Shouldly() => "".ShouldBe("foo");
    }
}
