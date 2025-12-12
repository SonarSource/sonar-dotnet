using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public interface IFoo
    {
        static abstract void Foo(__arglist); // Noncompliant
    }

    public class BaseClass : IFoo
    {
        public static void Foo(__arglist) // Compliant - interface implementation
        {
        }

        public virtual void Do(__arglist) // Noncompliant
        {
        }
    }
}

public class BaseClass(__arglist) // Nomcompliant {{Use the 'params' keyword instead of '__arglist'.}}
//           ^^^^^^^^^
{
    void Foo()
    {
        ArgIterator argumentIterator = new ArgIterator(__arglist); // Error [CS0190]
    }
}

public static class Extensions
{
    extension(string)
    {
        public static string StaticExtension(__arglist) => "42"; // Noncompliant
    }
    extension(string s)
    {
        public string InstanceExtension(__arglist) => "42";      // Noncompliant
    }
}

public partial class PartialConstructor
{
    public partial PartialConstructor(__arglist);
}
