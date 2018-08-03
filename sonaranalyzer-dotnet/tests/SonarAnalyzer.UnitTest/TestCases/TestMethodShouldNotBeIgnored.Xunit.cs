using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

namespace Tests.Diagnostics
{
    class XUnitClass
    {
        [Fact]
        [Ignore]
//       ^^^^^^ Noncompliant
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
