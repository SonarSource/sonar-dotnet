using System.Runtime.CompilerServices;

namespace Tests.Diagnostics
{
    public interface ISomeInterface
    {
        static abstract bool StaticVirtualMembersInInterfaces(bool flag, [CallerFilePath] string filePath = null);
    }

    public class SomeTestClass : ISomeInterface
    {
        public static bool StaticVirtualMembersInInterfaces(bool flag, [CallerFilePath] string filePath = null) => true;
    }

    public class SomeTestClass2 : ISomeInterface
    {
        public static bool StaticVirtualMembersInInterfaces(bool flag, [CallerFilePath] string filePath = null) => false;
    }

    public class SomeOtherClass
    {
        void MyMethod<T>(T someTestClass) where T: ISomeInterface
        {
            T.StaticVirtualMembersInInterfaces(true, "C:");                     // Noncompliant
            T.StaticVirtualMembersInInterfaces(false, filePath: "Something");   // Noncompliant
            T.StaticVirtualMembersInInterfaces(true);                           // Compliant
        }
    }
}
