using System;

namespace Tests.Diagnostics
{
    class Foo
    {
        public void Test()
        {
            bool b = true;

            NUnit.Framework.Assert.AreEqual(true, b); // Noncompliant
            NUnit.Framework.Assert.AreNotEqual(true, b); // Noncompliant
            NUnit.Framework.Assert.AreNotSame(true, b); // Noncompliant
            NUnit.Framework.Assert.AreSame(true, b); // Noncompliant
            NUnit.Framework.Assert.False(true); // Noncompliant
            NUnit.Framework.Assert.IsFalse(true); // Noncompliant
            NUnit.Framework.Assert.IsTrue(true); // Noncompliant
            NUnit.Framework.Assert.That(true); // Noncompliant
            NUnit.Framework.Assert.True(true); // Noncompliant
            NUnit.Framework.Assert.AreEqual(false, b); // Noncompliant
            NUnit.Framework.Assert.AreEqual(b, false); // Noncompliant
            NUnit.Framework.Assert.AreEqual(true, false); // Noncompliant

            NUnit.Framework.Assert.AreEqual(b, b);

            bool? x = false;
            NUnit.Framework.Assert.AreEqual(false, x); // Compliant, since the comparison triggers a conversion

        }
    }
}
