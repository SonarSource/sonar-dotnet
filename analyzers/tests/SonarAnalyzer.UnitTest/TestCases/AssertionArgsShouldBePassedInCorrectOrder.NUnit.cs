using System;
using NUnit.Framework;

namespace Tests.Diagnostics
{
    [TestFixture]
    class Program
    {
        void FakeAssert(object a, object b) { }

        [Test]
        public void Foo()
        {
            var str = "";
            Assert.AreEqual(str, ""); // Noncompliant {{Make sure these 2 arguments are in the correct order: expected value, actual value.}}
//                          ^^^^^^^
            Assert.AreSame(str, ""); // Noncompliant
//                         ^^^^^^^
            Assert.AreNotSame(str, ""); // Noncompliant

            double d = 42;
            Assert.AreEqual(d, 42); // Noncompliant
//                          ^^^^^
            Assert.AreSame(d, 42); // Noncompliant
            Assert.AreEqual(d, 42, 1, "message"); // Noncompliant

            Assert.AreEqual("", str);
            Assert.AreSame("", str);
            Assert.AreEqual(42, d, 1, "message");

            FakeAssert(d, 42);
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6547
namespace Repro_6547
{
    [TestFixture]
    class Program
    {
        public enum Seasons
        {
            Spring,
            Summer,
            Autumn,
            Winter
        }

        [Test]
        public void Repro_6547(Seasons seasonEnum, string seasonString)
        {
            const string expected = "Spring";
            Assert.AreEqual(seasonString, expected); // FN
            Assert.AreEqual(seasonEnum, Seasons.Spring); //FN

            Assert.AreEqual(expected, seasonString); // Compliant
            Assert.AreEqual(Seasons.Spring, seasonEnum); // Compliant
        }
    }
}
