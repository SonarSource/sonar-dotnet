using System;
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
}
