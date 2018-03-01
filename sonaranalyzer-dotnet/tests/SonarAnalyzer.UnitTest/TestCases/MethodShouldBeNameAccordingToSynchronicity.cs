using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class NonCompliantCases
    {
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
}
