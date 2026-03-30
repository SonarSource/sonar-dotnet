using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Tests.Diagnostics
{
    [TestFixture]
    class ProgramNUnit4
    {
        void FakeAssert(object a, object b) { }

        [Test]
        public void Simple(string str, double d)
        {
            ClassicAssert.AreEqual("", str);       // Compliant
            ClassicAssert.AreEqual(str, "");       // Noncompliant {{Make sure these 2 arguments are in the correct order: expected value, actual value.}}
            //                     ^^^^^^^
            ClassicAssert.AreEqual(42, d);         // Compliant
            ClassicAssert.AreEqual(d, 42);         // Noncompliant
            //                     ^^^^^
            ClassicAssert.AreNotEqual("", str);    // Compliant
            ClassicAssert.AreNotEqual(str, "");    // Noncompliant
            //                        ^^^^^^^
            ClassicAssert.AreNotEqual(42, d);      // Compliant
            ClassicAssert.AreNotEqual(d, 42);      // Noncompliant
            //                        ^^^^^
            ClassicAssert.AreSame("", str);        // Compliant
            ClassicAssert.AreSame(str, "");        // Noncompliant
            //                    ^^^^^^^
            ClassicAssert.AreSame(42, d);          // Compliant
            ClassicAssert.AreSame(d, 42);          // Noncompliant
            //                    ^^^^^
            ClassicAssert.AreNotSame("", str);     // Compliant
            ClassicAssert.AreNotSame(str, "");     // Noncompliant
            //                       ^^^^^^^
            ClassicAssert.AreNotSame(42, d);       // Compliant
            ClassicAssert.AreNotSame(d, 42);       // Noncompliant
            //                       ^^^^^

            ClassicAssert.AreEqual("", str, "message");    // Compliant
            ClassicAssert.AreEqual(str, "", "message");    // Noncompliant
            ClassicAssert.AreNotEqual("", str, "message"); // Compliant
            ClassicAssert.AreNotEqual(str, "", "message"); // Noncompliant
            ClassicAssert.AreSame("", str, "message");     // Compliant
            ClassicAssert.AreSame(str, "", "message");     // Noncompliant
            ClassicAssert.AreNotSame("", str, "message");  // Compliant
            ClassicAssert.AreNotSame(str, "", "message");  // Noncompliant

            ClassicAssert.IsNull(str);
            FakeAssert(d, 42);
        }
    }
}

namespace Repro_NUnit4_NamedArgs
{
    [TestFixture]
    class Program
    {
        [Test]
        public void Foo()
        {
            var str = "";
            ClassicAssert.AreEqual(actual: "", expected: str);  // Noncompliant
            ClassicAssert.AreEqual(expected: "", actual: str);  // Compliant
            ClassicAssert.AreEqual(actual: str, expected: "");  // Compliant
            ClassicAssert.AreEqual(expected: str, actual: "");  // Noncompliant

            ClassicAssert.AreNotEqual(actual: "", expected: str); // Noncompliant
            ClassicAssert.AreSame(actual: "", expected: str);     // Noncompliant
            ClassicAssert.AreNotSame(actual: "", expected: str);  // Noncompliant
        }
    }
}
