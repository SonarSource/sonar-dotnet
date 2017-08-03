using System;
using Xunit;
using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    class MsTestTest
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        private void PrivateTestMethod() // Noncompliant {{Make this test method 'public'.}}
//                   ^^^^^^^^^^^^^^^^^
        {
        }

        [TestMethod]
        protected void ProtectedTestMethod() // Noncompliant
        {
        }

        [TestMethod]
        internal void InternalTestMethod() // Noncompliant
        {
        }

        [TestMethod]
        public async void AsyncTestMethod()  // Noncompliant {{Make this test method non-'async'.}}
        {
        }

        [TestMethod]
        public void GenericTestMethod<T>()  // Noncompliant {{Make this test method non-generic.}}
        {
        }

        [TestMethod]
        private async void MultiErrorsMethod<T>() // Noncompliant {{Make this test method 'public', non-'async' and non-generic.}}
        {
        }

        [TestMethod]
        public async Task DoSomethingAsync() // Compliant
        {
        }
    }

    class XUnitTest
    {

        [Xunit.Fact]
        private void PrivateTestMethod() // Noncompliant
        {
        }

        [Fact]
        protected void ProtectedTestMethod() // Noncompliant
        {
        }

        [Fact]
        internal void InternalTestMethod() // Noncompliant
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
