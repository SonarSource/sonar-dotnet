using System;
using Xunit;

namespace Tests.Diagnostics
{
    class Program
    {
        [Fact]
        public void Foo()
        {
            var str = "";
            Assert.Equal(str, ""); // Noncompliant {{Make sure these 2 arguments are in the correct order: expected value, actual value.}}
//                       ^^^^^^^
            Assert.Same(str, ""); // Noncompliant
//                      ^^^^^^^

            double d = 42;
            Assert.Equal(d, 42); // Noncompliant
//                       ^^^^^
            Assert.Same(d, 42); // Noncompliant
            Assert.Equal(d, 42, 1); // Noncompliant

            Assert.Equal("", str);
            Assert.Same("", str);
            Assert.Equal(42, d, 1);
        }
    }
}
