using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class NonCompliantCases
    {
        public ValueTask<T> FooValueTaskT<T>() // Noncompliant
        {
        }

        public Task FooTask() // Noncompliant {{Add the 'Async' suffix to the name of this method.}}
//                  ^^^^^^^
        {
            return Task.Delay(0);
        }

        public Task<int> FooTaskInt() // Noncompliant
        {
            return Task.FromResult(1);
        }

        public Task<T> FooTaskT<T>() // Noncompliant
//                     ^^^^^^^^
        {
            return Task.FromResult(default(T));
        }

        public void BarVoidAsync() // Noncompliant {{Remove the 'Async' suffix to the name of this method.}}
//                  ^^^^^^^^^^^^
        {
        }

        public int BarIntAsync() // Noncompliant
        {
            return 1;
        }

        public object BarObjectAsync() // Noncompliant
        {
            return null;
        }

        public Task MyMethodAsync()
        {
            if (true)
            {
                return OtherSubMethod();
            }

            return SubMethod();

            Task SubMethod() => Task.Delay(0); // Compliant - should not be but requires C# 7 syntax

            Task OtherSubMethod()// Compliant - should not be but requires C# 7 syntax
            {
                return Task.Delay(0);
            }
        }
    }

    public interface IFoo
    {
        Task Do(); // Noncompliant
    }

    public class BaseClass : IFoo
    {
        public virtual Task<string> MyMethod()// Noncompliant
        {
            return Task.FromResult("foo");
        }

        public Task Do() // Compliant - comes from interface so not possible to change
        {
        }
    }

    public class CompliantCases : BaseClass
    {
        public Task FooTaskAsync() 
        {
            return Task.Delay(0);
        }

        public Task<int> FooTaskIntAsync()
        {
            return Task.FromResult(1);
        }

        public Task<T> FooTaskTAsync<T>()
        {
            return Task.FromResult(default(T));
        }

        public void BarVoid()
        {
        }

        public int BarInt()
        {
            return 1;
        }

        public object BarObject()
        {
            return null;
        }

        public override Task<string> MyMethod()
        {
            return Task.FromResult("foo");
        }
    }

    public class InvalidCode
    {
        public Task ()
        {
        }
    }

    public class TestAttributes
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task MSTest_TestMethod() { }

        [Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethod]
        public async Task MSTest_DataTestMethod() { }

        [NUnit.Framework.Test]
        public async Task NUnit_Test() { }

        [NUnit.Framework.TestCase(1)]
        public async Task NUnit_TestCase(int i) { }

        [NUnit.Framework.TestCaseSource()]
        public async Task NUnit_TestCaseSource() { }

        [NUnit.Framework.Theory]
        public async Task NUnit_Theory() { }

        [Xunit.Fact]
        public async Task Xunit_Fact() { }

        [Xunit.Theory]
        public async Task Xunit_Theory() { }

        [System.ComponentModel.Browsable(true)]
        public async Task OtherAttributes() { } // Noncompliant
    }
}
