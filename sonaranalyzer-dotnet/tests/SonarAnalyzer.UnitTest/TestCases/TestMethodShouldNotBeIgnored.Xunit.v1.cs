using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Xunit.Extensions;

// Legacy xUnit (v1.9.1)

// Note: we're mixing test frameworks here - xUnit and MSTest.
// This is deliberate. See the comments in the rule implementation for more info.

namespace Tests.Diagnostics
{
    class XUnitClass
    {
        [Fact]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore()]
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
        public void Foo1()
        {
        }

        [Fact]
        [Ignore] // This test is ignored because 'blah blah'
        public void Foo2()
        {
        }

        [Fact]
        [Ignore]
        [WorkItem(1234)]
        public void Foo3()
        {
        }

        [Theory]
        [InlineData("")]
        [Ignore]
//       ^^^^^^ Noncompliant
        public void Foo4(string s)
        {
        }

        [Theory]
        [InlineData("")]
        [Ignore] // This test is ignored because 'blah blah'
        public void Foo5(string s)
        {
        }

        [Theory]
        [InlineData("")]
        [Ignore]
        [WorkItem(1234)]
        public void Foo6(string s)
        {
        }
    }
}
