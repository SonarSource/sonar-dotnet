using System;

namespace Tests.Diagnostics
{
    class Foo
    {
        public void Test()
        {
            bool b = true;

            System.Diagnostics.Debug.Assert(false); // Noncompliant
            System.Diagnostics.Debug.Assert(true); // Noncompliant {{Remove or correct this assertion.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

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

            Xunit.Assert.Equal(true, b); // Noncompliant
            Xunit.Assert.False(true); // Noncompliant
            Xunit.Assert.NotEqual(true, b); // Noncompliant
            Xunit.Assert.Same(true, b); // Noncompliant
            Xunit.Assert.StrictEqual(true, b); // Noncompliant
            Xunit.Assert.NotSame(true, b); // Noncompliant
            Xunit.Assert.Equal(false, b); // Noncompliant
            Xunit.Assert.Equal(b, false); // Noncompliant
            Xunit.Assert.Equal(true, false); // Noncompliant


            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(true, b); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(true, b); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(true, b); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(true); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(true); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(false, b); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(b, false); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(true, false); // Noncompliant

            System.Diagnostics.Debug.Assert(b);
            NUnit.Framework.Assert.AreEqual(b, b);
            Xunit.Assert.Equal(b, b);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(b, b);
            Xunit.Assert.True(true); // There is no Assert.Fail in Xunit. Assert.True(false) is way to simulate it.
            Xunit.Assert.True(false);
        }
    }
}
