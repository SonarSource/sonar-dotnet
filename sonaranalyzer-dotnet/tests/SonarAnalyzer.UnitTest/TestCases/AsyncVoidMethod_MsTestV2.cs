using System;
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
}
