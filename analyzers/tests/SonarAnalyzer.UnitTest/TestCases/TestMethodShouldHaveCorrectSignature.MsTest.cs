namespace Tests.Diagnostics
{
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MsTestTest
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        private void PrivateTestMethod() // Noncompliant {{Make this test method 'public'.}}
//                   ^^^^^^^^^^^^^^^^^
        {
        }

        [Ignore][TestMethod]
        protected void ProtectedTestMethod() // Noncompliant
        {
        }

        [Ignore][TestMethod]
        internal void InternalTestMethod() // Noncompliant
        {
        }

        [Ignore][TestMethod]
        public async void AsyncTestMethod()  // Noncompliant {{Make this test method non-'async' or return 'Task'.}}
        {
        }

        [Ignore][TestMethod]
        public void GenericTestMethod<T>()  // Noncompliant {{Make this test method non-generic.}}
        {
        }

        [Ignore][TestMethod]
        private void MultiErrorsMethod1<T>() // Noncompliant {{Make this test method 'public' and non-generic.}}
        {
        }

        [Ignore][TestMethod]
        private async void MultiErrorsMethod2<T>() // Noncompliant {{Make this test method 'public', non-generic and non-'async' or return 'Task'.}}
        {
        }

        [Ignore][TestMethod]
        public async Task DoSomethingAsync() // Compliant
        {
            return;
        }
    }
}

namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;

    [TestClass]
    public class MsTestTest_DataTestMethods
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethod]
        private void PrivateTestMethod() // Noncompliant {{Make this test method 'public'.}}
//                   ^^^^^^^^^^^^^^^^^
        {
        }

        [DataTestMethod]
        protected void ProtectedTestMethod() // Noncompliant
        {
        }

        [DataTestMethod]
        internal void InternalTestMethod() // Noncompliant
        {
        }

        [DataTestMethod]
        public async void AsyncTestMethod()  // Noncompliant {{Make this test method non-'async' or return 'Task'.}}
        {
        }

        [DataTestMethod]
        public void GenericTestMethod<T>()  // Noncompliant {{Make this test method non-generic.}}
        {
        }

        [DataTestMethod]
        private void MultiErrorsMethod1<T>() // Noncompliant {{Make this test method 'public' and non-generic.}}
        {
        }

        [DataTestMethod]
        private async void MultiErrorsMethod2<T>() // Noncompliant {{Make this test method 'public', non-generic and non-'async' or return 'Task'.}}
        {
        }

        [DataTestMethod]
        public async Task DoSomethingAsync() // Compliant
        {
            return;
        }
    }
}
