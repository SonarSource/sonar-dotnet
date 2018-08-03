using System;
using Xunit;

namespace Tests.Diagnostics
{
    class XUnitTest
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
        public async void AsyncTestMethod()  // Noncompliant
        {
        }

        [Fact]
        public void GenericTestMethod<T>()  // Noncompliant
        {
        }
    }
}
