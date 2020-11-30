using System;

namespace Tests.Diagnostics
{
    class Foo
    {
        public void Test()
        {
            bool b = true;

            Xunit.Assert.Equal(true, b); // Noncompliant
            Xunit.Assert.False(true); // Noncompliant
            Xunit.Assert.NotEqual(true, b); // Noncompliant
            Xunit.Assert.Same(true, b); // Noncompliant
            Xunit.Assert.StrictEqual(true, b); // Noncompliant
            Xunit.Assert.NotSame(true, b); // Noncompliant
            Xunit.Assert.Equal(false, b); // Noncompliant
            Xunit.Assert.Equal(b, false); // Noncompliant
            Xunit.Assert.Equal(true, false); // Noncompliant

            Xunit.Assert.Equal(b, b);
            Xunit.Assert.True(true);
            // There is no Assert.Fail in Xunit. Assert.True(false) is way to simulate it.
            Xunit.Assert.True(false);

            bool? x = false;
            Xunit.Assert.Equal(false, x); // Compliant, since the comparison triggers a conversion

        }
    }
}
