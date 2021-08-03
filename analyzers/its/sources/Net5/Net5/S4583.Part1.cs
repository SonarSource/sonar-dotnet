using System;

namespace Net5.S4583
{
    public delegate void AsyncMethodCaller(string name, int i);

    class S4583
    {
        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod1()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapper(caller);
            var callback = new AsyncCallback(wrapper.CallEndInvoke);
            caller.BeginInvoke("delegate", 1, callback, null);
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod2()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapper(caller);
            var callback = new AsyncCallback(wrapper.DoNothing);
            caller.BeginInvoke("delegate", 1, callback, null);
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod4()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapperAnotherFile(caller);
            var callback = new AsyncCallback(wrapper.CallEndInvoke);
            caller.BeginInvoke("delegate", 1, callback, null);
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod5()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapperAnotherFile(caller);
            var callback = new AsyncCallback(wrapper.DoNothing);
            caller.BeginInvoke("delegate", 1, callback, null);
        }

        private static void AsyncMethod(string msg, int i) =>
            Console.WriteLine($"AsyncMethod: {msg} {i}");
    }

    partial class CallerWrapper
    {
        private AsyncMethodCaller caller;

        public CallerWrapper(AsyncMethodCaller caller) =>
            this.caller = caller;

        public partial void CallEndInvoke(IAsyncResult result);

        public partial void DoNothing(IAsyncResult result);
    }

    partial class CallerWrapper
    {
        public partial void CallEndInvoke(IAsyncResult result) =>
            caller.EndInvoke(result);

        public partial void DoNothing(IAsyncResult result) { }
    }

    public partial class CallerWrapperAnotherFile
    {
        private AsyncMethodCaller caller;

        public CallerWrapperAnotherFile(AsyncMethodCaller caller) =>
            this.caller = caller;

        public partial void CallEndInvoke(IAsyncResult result);

        public partial void DoNothing(IAsyncResult result);
    }
}
