using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    [TestClass]
    class Program
    {
        [TestMethod]
        public void Foo()
        {
            var str = "";
            Assert.AreEqual(str, ""); // Noncompliant {{Make sure these 2 arguments are in the correct order: expected value, actual value.}}
//                          ^^^^^^^
            Assert.AreSame(str, ""); // Noncompliant
//                         ^^^^^^^

            double d = 42;
            Assert.AreEqual(d, 42); // Noncompliant
//                          ^^^^^
            Assert.AreNotEqual(d, 42); // Noncompliant

            Assert.AreSame(d, 42); // Noncompliant
            Assert.AreEqual(d, 42, 1, "message"); // Noncompliant
            Assert.AreNotEqual(d, 42, 1, "message"); // Noncompliant

            Assert.AreEqual("", str);
            Assert.AreSame("", str);
            Assert.AreEqual(42, d, 1, "message");
            Assert.AreNotEqual(42, d, 1, "message");
            Assert.IsNull(str);

            Assert.AreEqual(actual: "", expected: str);
            Assert.AreEqual(expected: "", actual: str);
            Assert.AreEqual(actual: str, expected: ""); // Noncompliant FP
            Assert.AreEqual(expected: str, actual: "");
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6547
namespace Repro_6547
{
    [TestClass]
    class Program
    {
        public enum Seasons
        {
            Spring,
            Summer,
            Autumn,
            Winter
        }

        [TestMethod]
        [DataRow(Seasons.Spring, "Spring")]
        [DataRow(Seasons.Autumn, "Autumn")]
        public void Repro_6547(Seasons enumParameter, string stringParameter)
        {
            const string constString = "Spring";
            Assert.AreEqual(expected: stringParameter, actual: constString); // FN
            Assert.AreEqual(expected: enumParameter, actual: Seasons.Spring); // FN

            Assert.AreEqual(expected: constString, actual: stringParameter); // Compliant
            Assert.AreEqual(expected: Seasons.Spring, actual: enumParameter); // Compliant
        }
    }
}
