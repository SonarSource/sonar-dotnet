using System;
using Xunit;

namespace Tests.Diagnostics
{
    class Program
    {
        [Fact]
        public void Foo()
        {
            var str = "";
            Assert.Equal(str, ""); // Noncompliant {{Make sure these 2 arguments are in the correct order: expected value, actual value.}}
//                       ^^^^^^^
            Assert.Same(str, ""); // Noncompliant
//                      ^^^^^^^
            double d = 42;
            Assert.Equal(d, 42); // Noncompliant
//                       ^^^^^
            Assert.Same(d, 42); // Noncompliant
            Assert.NotSame(d, 42); // Noncompliant
            Assert.NotSame(42, d);
            Assert.Equal(d, 42, 1); // Noncompliant

            Assert.Equal("", str);
            Assert.Same("", str);
            Assert.Equal(42, d, 1);
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6547
namespace Repro_6547
{
    class Program
    {
        public enum Seasons { Spring, Summer, Autumn, Winter }

        [Fact]
        public void Repro_6547()
        {
            string sutString = RetriveString();
            Seasons sutSeason = RetriveSeason();

            const string constString = "Spring";
            Assert.Same(expected: sutString, actual: constString); // FN
            Assert.Same(expected: sutSeason, actual: Seasons.Spring); //FN

            Assert.Same(expected: constString, actual: sutString); // Compliant
            Assert.Same(expected: Seasons.Spring, actual: sutSeason); // Compliant
        }

        public Seasons RetriveSeason() => Seasons.Spring;
        public string RetriveString() => "Spring";
    }
}
