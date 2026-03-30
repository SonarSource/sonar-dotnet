using System;
using NUnit.Framework.Legacy;

namespace Tests.Diagnostics
{
    class FooNUnit4
    {
        public void Test()
        {
            bool b = true;

            ClassicAssert.AreEqual(true, b);    // Noncompliant
            ClassicAssert.AreNotEqual(true, b); // Noncompliant
            ClassicAssert.AreNotSame(true, b);  // Noncompliant
            ClassicAssert.AreSame(true, b);     // Noncompliant
            ClassicAssert.False(true);          // Noncompliant
            ClassicAssert.IsFalse(true);        // Noncompliant
            ClassicAssert.IsTrue(true);         // Noncompliant
            ClassicAssert.True(true);           // Noncompliant
            ClassicAssert.AreEqual(false, b);   // Noncompliant
            ClassicAssert.AreEqual(b, false);   // Noncompliant
            ClassicAssert.AreEqual(true, false); // Noncompliant

            ClassicAssert.AreEqual(b, b);

            bool? x = false;
            ClassicAssert.AreEqual(false, x); // Compliant, since the comparison triggers a conversion
            ClassicAssert.AreEqual(x, false); // Compliant, since the comparison triggers a conversion

            int i = 1;
            ClassicAssert.AreEqual(i, false); // Noncompliant
            ClassicAssert.AreEqual(false, i); // Noncompliant
            ClassicAssert.AreEqual();         // Error [CS1501] (code coverage)

            FooBar(); // Error [CS0103] (code coverage)
        }
    }
}
