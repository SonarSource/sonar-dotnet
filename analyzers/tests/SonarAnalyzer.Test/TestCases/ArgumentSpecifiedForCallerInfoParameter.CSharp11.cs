using System.Runtime.CompilerServices;
using System.Diagnostics;

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

    // https://sonarsource.atlassian.net/browse/NET-1295
    public class Repro_NET1252
    {
        void MyMethod()
        {
            Debug.Assert(true, "message");  // Compliant, Debug.Assert is excluded
        }
    }
}
