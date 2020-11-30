using System.Reflection;
using System.Resources;
using System.Globalization;

namespace Tests.Diagnostics
{
    static class RuntimeInformation
    {
        public static bool IsWindows() { return true; }
    }

    class Program
    {
        public static int i;
        static string s;

        static Program() // Noncompliant
        {
            i = 3;
            ResourceManager sm = new ResourceManager("strings", Assembly.GetExecutingAssembly());
            s = sm.GetString("mystring");
            sm = null;
        }
    }

    class Foo
    {
        static Foo()
        {
            System.Console.WriteLine("test");
        }
    }

    class Bar
    {
        static Bar()
        {
            Program.i = 42;
        }
    }

    static class Class1
    {
        public static readonly Foo foo1 = new Foo();
        public static readonly Foo foo2 = new Foo();

        public static readonly Foo someFoo;

        static Class1() // Noncompliant
        {
            if (RuntimeInformation.IsWindows())
                someFoo = foo1;
            else
                someFoo = foo2;
        }
    }

    static class ExpressionBodyCtor
    {
        private static readonly bool IsAvailable;

        static ExpressionBodyCtor() => IsAvailable = Initialize(); // Noncompliant

        static bool Initialize() => true;
    }
}
