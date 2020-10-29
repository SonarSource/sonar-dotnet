using System;
using System.Threading;

namespace Tests.Diagnostics
{
    public delegate void AsyncMethodCaller(string name, int i);

    class Program
    {
        private static void BeginInvokeOnDelegateWithLambdaCallback_DiscardParam()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            // here the "_" is actually an identifier, not a discard parameter
            caller.BeginInvoke("delegate", 1, (_) => { }, null); // Noncompliant
        }

        private static void AsyncMethod(string msg, int i)
        {
            Console.WriteLine($"AsyncMethod: {msg} {i}");
        }

    }
}
