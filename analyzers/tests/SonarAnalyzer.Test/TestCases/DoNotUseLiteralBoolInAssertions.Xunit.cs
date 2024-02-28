using System;

namespace Tests.Diagnostics
{
    class Foo
    {
        public void Test()
        {
            bool b = true;

            Xunit.Assert.Equal(true, b); // Noncompliant
            Xunit.Assert.NotEqual(true, b); // Noncompliant
            Xunit.Assert.Same(true, b); // Noncompliant
            Xunit.Assert.StrictEqual(true, b); // Noncompliant
            Xunit.Assert.NotSame(true, b); // Noncompliant
            Xunit.Assert.Equal(false, b); // Noncompliant
            Xunit.Assert.Equal(b, false); // Noncompliant
            Xunit.Assert.Equal(true, false); // Noncompliant

            Xunit.Assert.Equal(b, b);
            Xunit.Assert.True(true); // FN
            Xunit.Assert.False(false); // FN

            // There is no Assert.Fail in Xunit. Assert.True(false) or Assert.False(true) are ways to simulate it.
            Xunit.Assert.True(false); // Compliant
            Xunit.Assert.False(true); // Compliant

            bool? x = false;
            Xunit.Assert.Equal(false, x); // Compliant, since the comparison triggers a conversion

        }
    }
}
