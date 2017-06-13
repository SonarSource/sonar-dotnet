using System.Reflection;
using System.Resources;

namespace Tests.Diagnostics
{
    class Program
    {
        static int i;
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
}
