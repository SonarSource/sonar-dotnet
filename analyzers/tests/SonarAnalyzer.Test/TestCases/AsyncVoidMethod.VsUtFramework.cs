
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    [TestClass]
    public class MyUnitTests // MSTest V1 doesn't have proper support for async so people are forced to use async void
    {
        [AssemblyCleanup]
        public static async void AssemblyCleanup() { }

        [AssemblyInitialize]
        public static async void AssemblyInitialize() { }

        [ClassCleanup]
        public static async void ClassCleanup() { }

        [ClassInitialize]
        public static async void ClassInitialize() { }

        [TestCleanup]
        public async void TestCleanup() { }

        [TestInitialize]
        public async void TestInitialize() { }

        [TestMethod]
        public async void MyTest() { } // Noncompliant
    }

    internal class MsTestCases
    {
        public void Method()
        {
            [TestMethod] async void Get1() => await Task.FromResult(1);
            [TestMethod] async Task Get1s() => await Task.FromResult(1);
            async void Get2() => await Task.FromResult(2); // Compliant - FN
            async Task Get2s() => await Task.FromResult(2);

            Action a = [TestMethod] async () => { };
            Action b = async () => { };  // Compliant - FN
            Action c = [TestMethod] async () => { };  // Compliant - FN
            Func<Task> d = [TestMethod] async () => await Task.Delay(0);
            Func<Task> e = async () => await Task.Delay(0);
        }
    }
}
