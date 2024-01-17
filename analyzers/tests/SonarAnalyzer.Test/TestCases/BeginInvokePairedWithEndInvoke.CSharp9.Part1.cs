using System;

namespace Tests.Diagnostics
{
    public delegate void AsyncMethodCaller(string name, int i);

    class Program
    {
        private static void BeginInvokeOnDelegateWithLambdaCallback_UnderscoreParam()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            // here the "_" is actually an identifier, not a discard parameter
            caller.BeginInvoke("delegate", 1, (_) => { }, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod1()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapper(caller);
            var callback = new AsyncCallback(wrapper.CallEndInvoke);
            caller.BeginInvoke("delegate", 1, callback, null); // Compliant, EndInvoke is called by wrapper.CallEndInvoke
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod2()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapper(caller);
            var callback = new AsyncCallback(wrapper.DoNothing);
            caller.BeginInvoke("delegate", 1, callback, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod3()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapperNoImplementation(caller);
            var callback = new AsyncCallback(wrapper.MissingImplementation);  // Error [CS0762] Cannot create delegate from method 'CallerWrapperNoImplementation.MissingImplementation(IAsyncResult)' because it is a partial method without an implementing declaration
            caller.BeginInvoke("delegate", 1, callback, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod4()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapperAnotherFile(caller);
            var callback = new AsyncCallback(wrapper.CallEndInvoke);
            caller.BeginInvoke("delegate", 1, callback, null); // Compliant, EndInvoke is called by wrapper.CallEndInvoke
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod5()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapperAnotherFile(caller);
            var callback = new AsyncCallback(wrapper.DoNothing);
            caller.BeginInvoke("delegate", 1, callback, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallbackAsPartialMethod6()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapperAnotherFile(caller);
            AsyncCallback callback = new (wrapper.DoNothing);
            caller.BeginInvoke("delegate", 1, callback, null); // Noncompliant
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

    partial class CallerWrapperNoImplementation
    {
        private AsyncMethodCaller caller;

        public CallerWrapperNoImplementation(AsyncMethodCaller caller) =>
            this.caller = caller;

        public partial void MissingImplementation(IAsyncResult result); // Error [CS8795] Partial method 'CallerWrapperNoImplementation.MissingImplementation(IAsyncResult)' must have an implementation part because it has accessibility modifiers.
    }

    public partial class CallerWrapperAnotherFile
    {
        private AsyncMethodCaller caller;

        public CallerWrapperAnotherFile(AsyncMethodCaller caller) =>
            this.caller = caller;

        public partial void CallEndInvoke(IAsyncResult result);

        public partial void DoNothing(IAsyncResult result);
    }

    public record FooRecord
    {
        private AsyncMethodCaller caller = null;
        public string field;

        public FooRecord(string field)
        {
            this.field = field;
            caller.BeginInvoke("FooStruct", 42, null, null); // Noncompliant
        }

        private void BeginInvokeAndEndInvokeOnDelegateWithLambdaCallback1()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke(name: "delegate", 42, @object: null, callback: result => caller.EndInvoke(result)); // Compliant
        }

        private static void AsyncMethod(string msg, int i)
        {
            Console.WriteLine($"AsyncMethod: {msg}");
        }
    }

    public record PositionalRecord(string Property)
    {
        private AsyncMethodCaller caller = null;
        public string field;

        public PositionalRecord(string field, string property) : this(property)
        {
            this.field = field;
            this.Property = property;
            caller.BeginInvoke("FooStruct", 42, null, null); // Noncompliant
        }

        private void BeginInvokeAndEndInvokeOnDelegateWithLambdaCallback1()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke(name: "delegate", 42, @object: null, callback: result => caller.EndInvoke(result)); // Compliant
        }

        private static void AsyncMethod(string msg, int i)
        {
            Console.WriteLine($"AsyncMethod: {msg}");
        }
    }
}
