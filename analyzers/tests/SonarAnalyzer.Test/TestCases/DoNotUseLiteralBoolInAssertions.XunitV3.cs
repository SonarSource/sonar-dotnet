using System;
using Xunit;

namespace Tests.Diagnostics
{
    class XUnitV3Tests
    {
        public void Test()
        {
            bool b = true;

            Assert.Equal(true, b);          // Noncompliant
            Assert.NotEqual(true, b);       // Noncompliant
            Assert.Same(true, b);           // Noncompliant
            Assert.NotSame(true, b);        // Noncompliant
            Assert.StrictEqual(true, b);    // Noncompliant
            Assert.NotStrictEqual(true, b); // Noncompliant
            Assert.Equivalent(true, b);     // Noncompliant
        }
    }
}
