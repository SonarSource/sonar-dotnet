using System;
using NUnit.Framework;

namespace Tests.Diagnostics
{
    [TestFixture]
    class Program
    {
        void FakeAssert(object a, object b) { }

        [Test]
        public void Simple(string str, double d)
        {
            Assert.AreEqual("", str);       // Compliant
            Assert.AreEqual(str, "");       // Noncompliant {{Make sure these 2 arguments are in the correct order: expected value, actual value.}}
            //              ^^^^^^^
            Assert.AreEqual(42, d);         // Compliant
            Assert.AreEqual(d, 42);         // Noncompliant
            //              ^^^^^
            Assert.AreNotEqual("", str);    // Compliant
            Assert.AreNotEqual(str, "");    // Noncompliant
            //                 ^^^^^^^
            Assert.AreNotEqual(42, d);      // Compliant
            Assert.AreNotEqual(d, 42);      // Noncompliant
            //                 ^^^^^
            Assert.AreSame("", str);        // Compliant
            Assert.AreSame(str, "");        // Noncompliant
            //             ^^^^^^^
            Assert.AreSame(42, d);          // Compliant
            Assert.AreSame(d, 42);          // Noncompliant
            //             ^^^^^
            Assert.AreNotSame("", str);     // Compliant
            Assert.AreNotSame(str, "");     // Noncompliant
            //                ^^^^^^^
            Assert.AreNotSame(42, d);       // Compliant
            Assert.AreNotSame(d, 42);       // Noncompliant
            //                ^^^^^

            Assert.AreEqual("", str, "message");    // Compliant
            Assert.AreEqual(str, "", "message");    // Noncompliant
            Assert.AreNotEqual("", str, "message"); // Compliant
            Assert.AreNotEqual(str, "", "message"); // Noncompliant
            Assert.AreSame("", str, "message");     // Compliant
            Assert.AreSame(str, "", "message");     // Noncompliant
            Assert.AreNotSame("", str, "message");  // Compliant
            Assert.AreNotSame(str, "", "message");  // Noncompliant

            Assert.IsNull(str);
            FakeAssert(d, 42);
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6630
namespace Repro_6630
{
    [TestFixture]
    class Program
    {
        [Test]
        public void Foo()
        {
            var str = "";
            Assert.AreEqual(actual: "", expected: str); // Noncompliant
            Assert.AreEqual(expected: "", actual: str); // Compliant
            Assert.AreEqual(actual: str, expected: ""); // Compliant
            Assert.AreEqual(expected: str, actual: ""); // Noncompliant

            Assert.AreNotEqual(actual: "", expected: str);  // Noncompliant
            Assert.AreSame(actual: "", expected: str);      // Noncompliant
            Assert.AreNotSame(actual: "", expected: str);   // Noncompliant

            Assert.AreEqual(actual: null, expected: new Program()); // Noncompliant

            Assert.AreEqual(message: "", expected: str, actual: "");    // Noncompliant
            //                           ^^^^^^^^^^^^^^^^^^^^^^^^^
            Assert.AreEqual(expected: str, message: "", actual: "");    // Noncompliant
            //              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }

        [Test]
        public void Dynamic()
        {
            dynamic d = 42;
            Assert.AreEqual(d, 35);                    // Noncompliant
            Assert.AreEqual(35, d);                    // Compliant
            Assert.AreEqual(actual: d, expected: 35);  // Compliant
            Assert.AreEqual(actual: 35, expected: d);  // Noncompliant
        }

        [Test]
        public void BrokeSyntax()
        {
            double d = 42;
            Assert.Equual(d, 42);   // Error [CS0117]
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6547
namespace Repro_6547
{
    [TestFixture]
    class Program
    {
        public enum Seasons { Spring, Summer, Autumn, Winter }

        [Test]
        public void TestString()
        {
            string stringToTest = RetrieveString();
            const string constString = "Spring";

            Assert.AreEqual(expected: stringToTest, actual: constString); // Noncompliant
            Assert.AreEqual(expected: constString, actual: stringToTest); // Compliant
        }

        [Test]
        public void TestEnum()
        {
            Seasons seasonToTest = RetrieveSeason();

            Assert.AreEqual(expected: seasonToTest, actual: Seasons.Spring); //Noncompliant
            Assert.AreEqual(expected: Seasons.Spring, actual: seasonToTest); // Compliant
        }

        public Seasons RetrieveSeason() => Seasons.Spring;
        public string RetrieveString() => "Spring";
    }
}
