using System;
using NUnit.Framework;

namespace Tests.Diagnostics
{

    class NUnitTest
    {


        [NUnit.Framework.Test]
        private void PrivateTestMethod() // Noncompliant
        {
        }

        [Test]
        protected void ProtectedTestMethod() // Noncompliant
        {
        }

        [Test]
        internal void InternalTestMethod() // Noncompliant
        {
        }

        [Test]
        public async void AsyncTestMethod()  // Noncompliant
        {
        }

        [Test]
        public void GenericTestMethod<T>()  // Noncompliant
        {
        }
    }
}
