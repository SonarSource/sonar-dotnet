using System;
using Xunit;

namespace Tests.Diagnostics
{
    public class XUnitV3Tests
    {
        [Fact]
        public void Simple(string str)
        {
            Assert.Equivalent("", str);               // Compliant
            Assert.Equivalent(str, "");               // Noncompliant {{Make sure these 2 arguments are in the correct order: expected value, actual value.}}

            Assert.EquivalentWithExclusions("", str); // Compliant
            Assert.EquivalentWithExclusions(str, ""); // Noncompliant

            Assert.StrictEqual("", str);              // Compliant
            Assert.StrictEqual(str, "");              // Noncompliant

            Assert.NotStrictEqual("", str);           // Compliant
            Assert.NotStrictEqual(str, "");           // Noncompliant

            Assert.Contains("", str);                 // Compliant
            Assert.Contains(str, "");                 // Noncompliant

            Assert.DoesNotContain("", str);           // Compliant
            Assert.DoesNotContain(str, "");           // Noncompliant

            Assert.StartsWith("", str);               // Compliant
            Assert.StartsWith(str, "");               // Noncompliant

            Assert.EndsWith("", str);                 // Compliant
            Assert.EndsWith(str, "");                 // Noncompliant
        }
    }
}
