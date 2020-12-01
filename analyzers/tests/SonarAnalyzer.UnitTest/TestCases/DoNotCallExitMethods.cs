using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Foo()
        {
            Environment.Exit(0); // Noncompliant {{Remove this call to 'Environment.Exit' or ensure it is really required.}}
//                      ^^^^
            System.Windows.Forms.Application.Exit(); // Noncompliant {{Remove this call to 'Application.Exit' or ensure it is really required.}}
//                                           ^^^^
            Application.Exit();
        }

        public int FooProperty
        {
            get
            {
                Environment.Exit(0); // Noncompliant
                return 0;
            }
            set
            {
                Environment.Exit(0); // Noncompliant
            }
        }

        public static void Bar()
        {
            Baz();

            int x = 1;

            void Baz()
            {
                Environment.Exit(0); // Noncompliant
            }
        }
    }

    class MyProgram
    {
        public static void Main()
        {
            Environment.Exit(0); // Compliant - inside Main

            Foo();

            int x = 1;

            void Foo()
            {
                Environment.Exit(0); // Compliant - inside Main
            }
        }
    }

    class MyProgram1
    {
        public static async Task<int> Main(string[] args)
        {
            Environment.Exit(0); // Compliant - inside Main

            return 1;
        }
    }

    class MyProgram2
    {
        public static int Main(string[] args)
        {
            Environment.Exit(0); // Compliant - inside Main

            return 0;
        }
    }

    static class Application
    {
        public static void Exit()
        {
        }
    }
}
