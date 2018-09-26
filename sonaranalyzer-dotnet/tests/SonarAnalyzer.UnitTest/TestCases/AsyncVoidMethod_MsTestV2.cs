using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    [TestClass]
    public class MyUnitTests // MSTest V2 has proper support for async so people should avoid async void
    {
        [AssemblyCleanup]
        public static async void AssemblyCleanup() { } // Noncompliant

        [AssemblyInitialize]
        public static async void AssemblyInitialize() { } // Noncompliant

        [ClassCleanup]
        public static async void ClassCleanup() { } // Noncompliant

        [ClassInitialize]
        public static async void ClassInitialize() { } // Noncompliant

        [TestCleanup]
        public async void TestCleanup() { } // Noncompliant

        [TestInitialize]
        public async void TestInitialize() { } // Noncompliant

        [TestMethod]
        public async void MyTest() { } // Noncompliant
    }

    [TestClass]
    public class MyOtherUnitTests
    {
        [AssemblyCleanup]
        public static async Task AssemblyCleanup() { }

        [AssemblyInitialize]
        public static async Task AssemblyInitialize() { }

        [ClassCleanup]
        public static async Task ClassCleanup() { }

        [ClassInitialize]
        public static async Task ClassInitialize() { }

        [TestCleanup]
        public async Task TestCleanup() { }

        [TestInitialize]
        public async Task TestInitialize() { }

        [TestMethod]
        public async Task MyTest() { }
    }
}
