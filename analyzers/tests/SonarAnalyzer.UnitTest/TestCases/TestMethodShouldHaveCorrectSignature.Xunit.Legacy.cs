using System;
using Xunit;
using Xunit.Extensions;

namespace Tests.Diagnostics
{
    public class XUnitTest
    {

        [Xunit.Fact]
        private void PrivateTestMethod() // Compliant
        {
        }

        [Fact]
        protected void ProtectedTestMethod() // Compliant
        {
        }

        [Fact]
        internal void InternalTestMethod() // Compliant
        {
        }

        [Fact]
        public async void AsyncTestMethod()  // Compliant
        {
        }

        [Fact]
        public void GenericTestMethod<T>()  // Noncompliant
        {
        }


        [Xunit.Extensions.Theory]
        [InlineData(42)]
        private void PrivateTestMethod_Theory(int arg) // Compliant
        {
        }

        [Theory]
        [InlineData(42)]
        protected void ProtectedTestMethod_Theory(int arg) // Compliant
        {
        }

        [Theory]
        [InlineData(42)]
        internal void InternalTestMethod_Theory(int arg) // Compliant
        {
        }

        [Theory]
        [InlineData(42)]
        public async void AsyncTestMethod_Theory(int arg)  // Compliant
        {
        }

        [Theory]
        [InlineData(42)]
        public void GenericTestMethod_Theory<T>(T arg)  // Compliant - theories can be generic
        {
        }
    }
}
