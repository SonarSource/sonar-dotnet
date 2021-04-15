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

        [Test]
        public void ValidTest()
        {
        }
    }

    class NUnitTest_TestCase
    {
        [NUnit.Framework.TestCase(42)]
        private void PrivateTestMethod(int data) // Noncompliant
        {
        }

        [TestCase(42)]
        protected void ProtectedTestMethod(int data) // Noncompliant
        {
        }

        [TestCase(42)]
        internal void InternalTestMethod(int data) // Noncompliant
        {
        }

        [TestCase(42)]
        public async void AsyncTestMethod(int data)  // Noncompliant
        {
        }

        [TestCase(42)]
        public void GenericTestMethod<T>(T data)  // valid
        {
        }

        [TestCase(42)]
        public void ValidTest(int data)
        {
        }
    }

    class NUnitTest_TestCaseSource
    {
        public static object[] DataProvider = { 42 };

        [NUnit.Framework.TestCaseSource("DataProvider")]
        private void PrivateTestMethod(object data) // Noncompliant
        {
        }

        [TestCaseSource("DataProvider")]
        protected void ProtectedTestMethod(object data) // Noncompliant
        {
        }

        [TestCaseSource("DataProvider")]
        internal void InternalTestMethod(object data) // Noncompliant
        {
        }

        [TestCaseSource("DataProvider")]
        public async void AsyncTestMethod(object data)  // Noncompliant
        {
        }

        [TestCaseSource("DataProvider")]
        public void GenericTestMethod<T>(T data)  // compliant
        {
        }

        [TestCaseSource("DataProvider")]
        public void ValidTest(object data)
        {

        }
    }

    public class NUnitTest_Theories
    {
        [NUnit.Framework.Theory]
        private void PrivateTestMethod(int data) // Noncompliant
        {
        }

        [Theory]
        protected void ProtectedTestMethod(int data) // Noncompliant
        {
        }

        [Theory]
        internal void InternalTestMethod(int data) // Noncompliant
        {
        }

        [Theory]
        public async void AsyncTestMethod(int data)  // Noncompliant
        {
        }

        [Theory]
        public void GenericTestMethod<T>(T[] data)  // Noncompliant
        {
        }

        [Theory]
        public void ValidTest(int data)
        {
        }
    }
}
