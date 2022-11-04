using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public interface IInvalidCases
    {
        static virtual async Task<string> FooAsync(string something) // Noncompliant {{Split this method into two, one handling parameters check and the other handling the asynchronous code.}}
        {
            if (something == null) { throw new ArgumentNullException(nameof(something)); } // Secondary

            await Task.Delay(1);
            return something + "foo";
        }
    }
}
