using System;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    class Foo
    {
        public void Test()
        {
            bool b = true;

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(true, b); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(true, b); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(true, b); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(true); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(true); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(false, b); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(b, false); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(true, false); // Noncompliant
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(b, b);

            Debug.Assert(false); // Noncompliant
            System.Diagnostics.Debug.Assert(true); // Noncompliant {{Remove or correct this assertion.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            System.Diagnostics.Debug.Assert(b);

            bool? x = false;
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(false, x); // Compliant, since the comparison triggers a conversion

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(); // Error [CS1501] (code coverage)
            Foo(); // Error [CS1955] (code coverage)
        }
    }
}
