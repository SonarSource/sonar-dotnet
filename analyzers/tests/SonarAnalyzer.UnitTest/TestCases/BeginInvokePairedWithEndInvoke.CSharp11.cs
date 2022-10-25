using System;
using Tests.Diagnostics;

namespace Tests.Diagnostics
{
    public delegate void AsyncMethodCaller();

    public interface ITestInterface
    {
        static virtual void StaticVirtualMembersInInterfacesNoncompliant()
        {
            var caller = new AsyncMethodCaller(Method);
            caller.BeginInvoke(null, null); // Noncompliant

            void Method() { }
        }

        static virtual void StaticVirtualMembersInInterfacesCompliant()
        {
            var caller = new AsyncMethodCaller(Method);
            IAsyncResult result = caller.BeginInvoke(null, null); // Compliant
            caller.EndInvoke(result);

            void Method() { }
        }
    }

    public interface ISomeInterface
    {
        static abstract void StaticVirtualMembersInInterfaces();
    }

    public class SomeClass : ISomeInterface
    {
        public static void StaticVirtualMembersInInterfaces()
        {
            return;
        }
    }

    public class SomeOtherClass
    {
        void TestMethodCompliant()
        {
            var caller = new AsyncMethodCaller(SomeClass.StaticVirtualMembersInInterfaces);
            IAsyncResult result = caller.BeginInvoke(null, null); // Compliant
            caller.EndInvoke(result);
        }

        void TestMethodNonCompliant()
        {
            var caller = new AsyncMethodCaller(SomeClass.StaticVirtualMembersInInterfaces);
            caller.BeginInvoke(null, null); // Noncompliant
        }
    }
}
