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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] // Error [CS0601] - so do not raise an issue
        public extern bool RemoveDirectory6(string name);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] // Error [CS0601]
        public static bool RemoveDirectory7(string name); // Error [CS0501] - so do not raise an issue
    }

    internal class Foo
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool RemoveDirectory1(string name); // Compliant because effective accessibility is not public
    }
}
