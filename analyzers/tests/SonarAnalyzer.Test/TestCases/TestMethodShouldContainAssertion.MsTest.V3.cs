namespace TestMsTest
{
    using System;
    using System.Text;
    using FluentAssertions;
    using NFluent;
    using NSubstitute;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class Program
    {
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestMethod1()
        {
            var x = System.IO.File.Open("", System.IO.FileMode.Open);
        }

        [DataTestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestMethod2()
        {
            var x = System.IO.File.Open("", System.IO.FileMode.Open);
        }
    }
}
