using System;

namespace Tests.Diagnostics
{
    public delegate void AsyncMethodCaller(string name, int i);

    public record struct FooRecordStruct
    {
        private AsyncMethodCaller caller = null;
        public string field;

        public FooRecordStruct(string field)
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

    public record struct PositionalRecordStruct(string Property)
    {
        private AsyncMethodCaller caller = null;
        public string field = null;

        public PositionalRecordStruct(string field, string property) : this(property)
        {
            this.field = field;
            caller.BeginInvoke("FooStruct", 42, null, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithLambdaCallback1()
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
