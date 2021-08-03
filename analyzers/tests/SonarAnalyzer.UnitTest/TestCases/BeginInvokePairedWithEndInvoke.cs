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

        private static void BeginInvokeOnDelegateWithoutCallback()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke("delegate", /* callback */null, /* state */ null); // Noncompliant {{Pair this "BeginInvoke" with an "EndInvoke".}}
            // https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/calling-synchronous-methods-asynchronously
            // «Important: No matter which technique you use, always call EndInvoke to complete your asynchronous call.»
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithoutCallback()
        {
            Console.WriteLine("BeginInvokeAndEndInvokeOnDelegateWithoutCallback");
            var caller = new AsyncMethodCaller(AsyncMethod);
            IAsyncResult result = caller.BeginInvoke("delegate", /* callback */null, /* state */ null); // Compliant
            caller.EndInvoke(result);
        }

        private static void BeginInvokeOnDelegateWithLambdaCallback1()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke("delegate", result => { }, null); // Noncompliant
        }

        private static void BeginInvokeOnDelegateWithLambdaCallback2()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var callback = new AsyncCallback(result => { });
            caller.BeginInvoke("delegate",  callback, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithLambdaCallback1()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke(name: "delegate", @object: null, callback: result => caller.EndInvoke(result)); // Compliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithLambdaCallback2()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var callback = new AsyncCallback(result => caller.EndInvoke(result));
            caller.BeginInvoke("delegate", callback, null); // Compliant, EndInvoke is called by callback
        }

        private static void BeginInvokeOnDelegateWithDelegateCallback()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke("delegate",  delegate(IAsyncResult result) { Console.WriteLine(); }, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithDelegateCallback()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke("delegate",  delegate(IAsyncResult result) { caller.EndInvoke(result); }, null); // Compliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithVariableCallback()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            AsyncCallback callback = result => { caller.EndInvoke(result); };
            caller.BeginInvoke("delegate",  callback, null); // Compliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithStaticCallback1()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var callback = new AsyncCallback(StaticCallEndInvoke);
            caller.BeginInvoke("delegate",  callback, null); // Compliant, EndInvoke is called by callback and it's StaticCallEndInvoke
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithStaticCallback2()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            caller.BeginInvoke("delegate",  new AsyncCallback(StaticDoNothing), null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithStaticCallback3()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var callback = new AsyncCallback(StaticDoNothing);
            caller.BeginInvoke("delegate",  callback, null); // Noncompliant
        }

        private static void BeginInvokeOnDelegateWithCallbackAssignment()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            AsyncCallback callback;
            callback = new AsyncCallback(StaticDoNothing);
            caller.BeginInvoke("delegate",  callback, null); // false-negative, we only look at the variable initialization and not at all its assignments
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallback1()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapper(caller);
            var callback = new AsyncCallback(wrapper.CallEndInvoke);
            caller.BeginInvoke("delegate",  callback, null); // Compliant, EndInvoke is called by wrapper.CallEndInvoke
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithWrapperCallback2()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var wrapper = new CallerWrapper(caller);
            var callback = new AsyncCallback(wrapper.DoNothing);
            caller.BeginInvoke("delegate",  callback, null); // Noncompliant
        }

        private static void BeginInvokeAndEndInvokeOnDelegateWithNonExistingCallback()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var callback = new AsyncCallback(Console.WriteLine);
            caller.BeginInvoke("delegate",  callback, null); // Compliant
        }

        private static void BeginInvokeOnAnyClassButDelegate()
        {
            var notADelegate = new AnyClassWithOptionalEndInvoke();
            var result = notADelegate.BeginInvoke(new AsyncMethodCaller(AsyncMethod));
            // Compliant, NotADelegate class declared below does not required a call to EndInvoke
            // Same as System.Windows.Forms.Control see
            // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.control.begininvoke?view=netframework-4.6
            // «You can call EndInvoke to retrieve the return value from the delegate, if necessary, but this is not required.»
        }

        private static void AsyncMethod(string msg)
        {
            Console.WriteLine($"AsyncMethod: {msg}");
        }

        private static void StaticCallEndInvoke(IAsyncResult result)
        {
            AsyncMethodCaller caller = (AsyncMethodCaller) ((AsyncResult) result).AsyncDelegate;
            caller.EndInvoke(result);
        }

        private static void StaticDoNothing(IAsyncResult result)
        {
        }

        private static void EndInvokeOfDifferentAction()
        {
            Action a = () => { };
            Action b = () => { };
            AsyncCallback callback = b.EndInvoke;
            a.BeginInvoke(callback, null); // FN
        }

        private static void BeginInvokeAndEndInvokeOnDifferentDelegateWithVariableCallback()
        {
            var caller = new AsyncMethodCaller(AsyncMethod);
            var caller2 = new AsyncMethodCaller(AsyncMethod);
            AsyncCallback callback = result => { caller2.EndInvoke(result); };
            caller.BeginInvoke("delegate", callback, null); // FN
        }

        public class CallerWrapper
        {
            private AsyncMethodCaller caller;

            public  CallerWrapper(AsyncMethodCaller caller)
            {
                this.caller = caller;
            }

            public void CallEndInvoke(IAsyncResult result)
            {
                caller.EndInvoke(result);
            }

            public void DoNothing(IAsyncResult result)
            {
            }

        }

        public class AnyClassWithOptionalEndInvoke
        {

            public IAsyncResult BeginInvoke(AsyncMethodCaller method)
            {
                return method.BeginInvoke("NotADelegate", result => { method.EndInvoke(result); }, null);
            }

            // It's not required to call EndInvoke after BeginInvoke on this class
            public object EndInvoke(IAsyncResult asyncResult)
            {
                return null;
            }

        }

        // Coverage
        public class Foo
        {
            public static AsyncMethodCaller caller = new AsyncMethodCaller(AsyncMethod);
            public IAsyncResult result = caller.BeginInvoke("Foo", null, null); // Noncompliant
            public Action action1 = delegate
            {
                caller.BeginInvoke("Foo", null, null); // Noncompliant
            };
            public Action action2 = () =>
            {
                caller.BeginInvoke("Foo", null, null); // Noncompliant
            };
            public Action<string> action3 = name =>
            {
                caller.BeginInvoke(name, null, null); // Noncompliant
            };

            public int prop
            {
                get
                {
                    caller.BeginInvoke("prop", null, null); // Noncompliant
                    return 0;
                }
            }

            public static implicit operator int(Foo f)
            {
                caller.BeginInvoke("prop", null, null); // Noncompliant
                return 0;
            }

            public static Foo operator +(Foo b, Foo c)
            {
                caller.BeginInvoke("prop", null, null); // Noncompliant
                return new Foo();
            }

            static Foo()
            {
                caller.BeginInvoke("Foo", null, null); // Noncompliant
            }

            public Foo()
            {
                caller.BeginInvoke("Foo", null, null); // Noncompliant
            }

            public void Compliant()
            {
                IAsyncResult result = caller.BeginInvoke("method", null, null); // Compliant
                caller.EndInvoke(result);
            }

            public void Bar()
            {
                caller.BeginInvoke("Foo", null, null); // Noncompliant
            }

            ~Foo()
            {
                caller.BeginInvoke("Foo", null, null); // Noncompliant
            }

            private void InvokeSomethingElse()
            {
                var BeginInvoke = "MemberBinding";
                var EndInvoke = "MemberBinding";
                BeginInvoke.ToString();
                EndInvoke.ToString();
            }

            public struct FooStruct
            {
                public string field;

                public FooStruct(string field)
                {
                    this.field = field;
                    caller.BeginInvoke("FooStruct", null, null); // Noncompliant
                }
            }

            public void Container()
            {
                IAsyncResult result = BeginInvokeHiddenInALocalFunction();
                caller.EndInvoke(result);

                IAsyncResult BeginInvokeHiddenInALocalFunction()
                {
                    return caller.BeginInvoke("method", null, null); // Noncompliant
                }
            }
        }

        public class FakeProperty
        {
            private AsyncMethodCaller caller;

            public int Prop
            {
                get
                {
                    caller.BeginInvoke("prop", null, null); // FN, detection thinks that "EndInvoke" in setter is valid pair for this
                    return 0;
                }
                set
                {
                    caller.EndInvoke(null);
                }
            }
        }
    }

    // Dummy implementation of IAsyncResult compatible with both .Net Framework and .Net Core
    internal class AsyncResult : IAsyncResult
    {
        public object AsyncState { get; }

        public WaitHandle AsyncWaitHandle { get; }

        public bool CompletedSynchronously { get; }

        public bool IsCompleted { get; }

        public virtual object AsyncDelegate { get; }
    }

    class ReproEndinvokeDelegate
    {
        public void BeginInvokeWithEndinvokeDelegate()
        {
            Action a = () => { };
            a.BeginInvoke(a.EndInvoke, null); // Compliant
        }

        public void AsyncCallbackLocalVariable()
        {
            Action a = () => { };
            AsyncCallback callback = a.EndInvoke;
            a.BeginInvoke(callback, null); // Compliant
        }

        public void AsyncCallbackLocalVariableNotEndinvokeAccess()
        {
            Action a = () => { };
            AsyncCallback callback = a.NotEndInvoke; // Error [CS1061]
            a.BeginInvoke(callback, null); // Noncompliant
        }
    }
}
