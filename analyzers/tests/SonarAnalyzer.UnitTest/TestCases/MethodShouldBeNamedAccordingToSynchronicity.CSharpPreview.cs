using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Tests.Diagnostics
{
    public interface IFoo
    {
        static abstract Task Do(); // Noncompliant
    }

    public class BaseClass : IFoo
    {
        public virtual Task<string> MyMethod() // Noncompliant
        {
            return Task.FromResult("foo");
        }

        public static Task Do() // Compliant - comes from interface so not possible to change
        {
            return null;
        }
    }
}
