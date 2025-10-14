namespace TestMsTest
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NFluent;
    using NSubstitute;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class Program
    {
        [DerivedTestMethod]
        public void DerivedTestMethod1() // Noncompliant {{Add at least one assertion to this test case.}}
        //          ^^^^^^^^^^^^^^^^^^
        {
            var x = 42;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class DerivedTestMethodAttribute : TestMethodAttribute
    {
        public DerivedTestMethodAttribute([CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1) : base(callerFilePath, callerLineNumber)
        { }

        public override Task<TestResult[]> ExecuteAsync(ITestMethod testMethod) => base.ExecuteAsync(testMethod);
    }
}
