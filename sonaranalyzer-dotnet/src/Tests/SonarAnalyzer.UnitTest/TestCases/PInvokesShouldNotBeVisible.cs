using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool RemoveDirectory1(string name); // Noncompliant {{Make this 'P/Invoke' method private or internal.}}
//                                ^^^^^^^^^^^^^^^^

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        protected static extern bool RemoveDirectory2(string name); // Noncompliant

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal protected static extern bool RemoveDirectory3(string name); // Noncompliant

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern bool RemoveDirectory4(string name);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool RemoveDirectory5(string name);
    }
}
