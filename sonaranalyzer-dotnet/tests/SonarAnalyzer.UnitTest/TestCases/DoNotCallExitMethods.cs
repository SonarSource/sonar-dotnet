using System;
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
    }

    static class Application
    {
        public static void Exit()
        {
        }
    }
}
