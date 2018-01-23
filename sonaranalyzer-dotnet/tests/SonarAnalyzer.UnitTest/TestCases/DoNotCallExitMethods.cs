using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Foo()
        {
            Environment.Exit(0); // Noncompliant
            System.Windows.Forms.Application.Exit(); // Noncompliant
            Application.Exit();
        }

        public static void Main()
        {
            Environment.Exit(0); // Compliant - inside Main
        }
        public static int Main(string[] args)
        {
            Environment.Exit(0); // Compliant - inside Main
        }
        public static async Task<int> Main(string[] args)
        {
            Environment.Exit(0); // Compliant - inside Main
        }
    }

    static class Application
    {
        public static void Exit()
        {
        }
    }
}
