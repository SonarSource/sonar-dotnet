using System;

namespace Tests.Diagnostics
{
    class Program_00
    {
        [SomeNonsense]
        public static void Main() // Noncompliant {{Add the 'STAThread' attribute to this entry point.}}
        {
        }
    }

    class Program_01
    {
        public static void Main() // Noncompliant {{Add the 'STAThread' attribute to this entry point.}}
//                         ^^^^
        {
        }
    }

    class Program_02
    {
        public static int Main(string[] args) // Noncompliant {{Add the 'STAThread' attribute to this entry point.}}
//                        ^^^^
        {
        }
    }

    class Program_03
    {
        [MTAThread]
        public static void Main() // Noncompliant {{Change the 'MTAThread' attribute of this entry point to 'STAThread'.}}
        {
        }
    }

    class Program_04
    {
        [MTAThread]
        public static int Main(string[] args) // Noncompliant {{Change the 'MTAThread' attribute of this entry point to 'STAThread'.}}
        {
        }
    }

    class Program_05
    {
        [System.MTAThreadAttribute]
        public static int Main(string[] args) // Noncompliant {{Change the 'MTAThread' attribute of this entry point to 'STAThread'.}}
        {
        }
    }

    class Program_06
    {
        [STAThread]
        public static void Main()
        {
        }
    }

    class Program_07
    {
        [STAThread]
        public static int Main(string[] args)
        {
        }
    }

    class Program_08
    {
        [System.STAThread]
        public static int Main(string[] args)
        {
        }
    }

    class Program_09
    {
        [STAThread("this is wrong", 1)] // Invalid code
        public static int Main(string[] args)
        {
        }
    }
}
