using System;
using Xunit;

namespace Tests.Diagnostics
{
    class Program
    {
        [Fact]
        public void Simple(string str, double d)
        {
            Assert.Equal("", str);       // Compliant
            Assert.Equal(str, "");       // Noncompliant {{Make sure these 2 arguments are in the correct order: expected value, actual value.}}
            //           ^^^^^^^
            Assert.Equal(42, d);         // Compliant
            Assert.Equal(d, 42);         // Noncompliant
            //           ^^^^^
            Assert.NotEqual("", str);    // Compliant
            Assert.NotEqual(str, "");    // Noncompliant
            //              ^^^^^^^
            Assert.NotEqual(42, d);      // Compliant
            Assert.NotEqual(d, 42);      // Noncompliant
            //              ^^^^^
            Assert.Same("", str);        // Compliant
            Assert.Same(str, "");        // Noncompliant
            //          ^^^^^^^
            Assert.Same(42, d);          // Compliant
            Assert.Same(d, 42);          // Noncompliant
            //          ^^^^^
            Assert.NotSame("", str);     // Compliant
            Assert.NotSame(str, "");     // Noncompliant
            //             ^^^^^^^
            Assert.NotSame(42, d);       // Compliant
            Assert.NotSame(d, 42);       // Noncompliant
            //             ^^^^^

            Assert.Null(str);
        }

        [Fact]
        public void Dynamic()
        {
            dynamic d = 42;
            Assert.Equal(d, 35);                    // Noncompliant
            Assert.Equal(35, d);                    // Compliant
            Assert.Equal(actual: d, expected: 35);  // Compliant
            Assert.Equal(actual: 35, expected: d);  // Noncompliant
        }

        [Fact]
        public void BrokeSyntax()
        {
            double d = 42;
            Assert.Equual(d, 42);   // Error [CS0117]
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6630
namespace Repro_6630
{
    class Program
    {
        [Fact]
        public void Foo()
        {
            var str = "";
            Assert.Equal(actual: "", expected: str);    // Noncompliant
            Assert.Equal(expected: "", actual: str);    // Compliant
            Assert.Equal(actual: str, expected: "");    // Compliant
            Assert.Equal(expected: str, actual: "");    // Noncompliant

            Assert.Same(actual: "", expected: str);     // Noncompliant
            Assert.NotSame(actual: "", expected: str);  // Noncompliant

            int d = 42;
            Assert.Equal<int>(actual: 1, expected: d);              // Noncompliant
            Assert.Equal(actual: null, expected: new Program());    // Noncompliant

            Assert.Equal(expected: str, actual: "");    // Noncompliant
            //           ^^^^^^^^^^^^^^^^^^^^^^^^^
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
        public void TestString()
        {
            string stringToTest = RetrieveString();
            const string constString = "Spring";

            Assert.Same(expected: stringToTest, actual: constString); // Noncompliant
            Assert.Same(expected: constString, actual: stringToTest); // Compliant
        }

        [Fact]
        public void TestEnum()
        {
            Seasons seasonToTest = RetrieveSeason();

            Assert.Same(expected: seasonToTest, actual: Seasons.Spring); // Noncompliant
            Assert.Same(expected: Seasons.Spring, actual: seasonToTest); // Compliant
        }

        public Seasons RetrieveSeason() => Seasons.Spring;
        public string RetrieveString() => "Spring";
    }
}
