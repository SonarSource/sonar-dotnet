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
}
