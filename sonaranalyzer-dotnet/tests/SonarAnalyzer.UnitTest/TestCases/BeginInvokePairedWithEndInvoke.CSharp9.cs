using System;
using System.Threading;

namespace Tests.Diagnostics
{
    public delegate void AsyncMethodCaller(String name);

    class Program
    {
        static void Main(string[] args)
        {
            BeginInvokeAndEndInvokeOnDelegateWithoutCallback();
        }

        private static void BeginInvokeOnDelegateWithLambdaCallback_DiscardParam()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke("delegate", (_, _) => { }, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithLambdaCallback1_DiscardParam()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke(name: "delegate", @object: null, callback: (_, _) => caller.EndInvoke(null)); // Compliant
        }

        private static void BeginInvokeOnDelegateWithDelegateCallback_DiscardParam()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke("delegate", delegate (IAsyncResult _, int _) { Console.WriteLine(); }, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithVariableCallback_DiscardParam()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            AsyncCallback callback = (_, _) => { caller.EndInvoke(null); };
            caller.BeginInvoke("delegate", callback, null); // Compliant
        }
    }
}
