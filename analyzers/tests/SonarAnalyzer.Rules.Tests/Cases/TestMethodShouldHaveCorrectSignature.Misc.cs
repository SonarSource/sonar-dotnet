namespace Tests.Diagnostics
{
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class TestThatIssuesAreOnlyReportedOnceForPartialClasses
    {
    }

    [TestClass]
    public partial class TestThatIssuesAreOnlyReportedOnceForPartialClasses
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        private void PrivateTestMethod() // Noncompliant {{Make this test method 'public'.}}
//                   ^^^^^^^^^^^^^^^^^
        {
        }
    }

    [TestClass]
    public class MultipleFaultsInEachMethod
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        private async void PrivateVoidGenericTestMethod<T>() // Noncompliant {{Make this test method 'public', non-generic and non-'async' or return 'Task'.}}
//                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        {
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        private async void PrivateVoidTestMethod() // Noncompliant {{Make this test method 'public' and non-'async' or return 'Task'.}}
//                         ^^^^^^^^^^^^^^^^^^^^^
        {
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async void VoidGenericTestMethod<T>() // Noncompliant {{Make this test method non-generic and non-'async' or return 'Task'.}}
//                        ^^^^^^^^^^^^^^^^^^^^^
        {
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        private void PrivateGenericTestMethod<T>() // Noncompliant {{Make this test method 'public' and non-generic.}}
//                   ^^^^^^^^^^^^^^^^^^^^^^^^
        {
        }
    }

    [TestClass]
    public class NonTestAttributesAreIgnored
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        [System.Diagnostics.DebuggerStepThrough]
        private void PrivateTestMethod() // Noncompliant {{Make this test method 'public'.}}
//                   ^^^^^^^^^^^^^^^^^
        {
        }
    }

    [TestClass]
    public class InvalidAttributes
    {
        [UnknownAttribute]      // Shouldn't fail // Error [CS0246, CS0246]
        private void AMethod()
        {
        }
    }
}
