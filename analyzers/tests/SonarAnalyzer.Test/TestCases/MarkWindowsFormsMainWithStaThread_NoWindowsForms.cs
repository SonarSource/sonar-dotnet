using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    class Program_00
    {
        [SomeNonsense] // Error [CS0246,CS0246] - unknown type
        // Error@+2 [CS0017] Program has more than one entry point
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static void Main()
        {
        }
    }

    class Program_01
    {
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static void Main()

        {
        }
    }

    class Program_02
    {
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static int Main(string[] args)
        {
            return 1;
        }
    }

    class Program_03
    {
        [MTAThread]
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static void Main()
        {
        }
    }

    class Program_04
    {
        [MTAThread]
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static int Main(string[] args)
        {
            return 1;
        }
    }

    class Program_05
    {
        [System.MTAThreadAttribute]
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static int Main(string[] args)
        {
            return 1;
        }
    }

    class Program_06
    {
        [STAThread]
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static void Main()
        {
        }
    }

    class Program_07
    {
        [STAThread]
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static int Main(string[] args)
        {
            return 1;
        }
    }

    class Program_08
    {
        [System.STAThread]
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static int Main(string[] args)
        {
            return 1;
        }
    }

    class Program_09
    {
        [STAThread("this is wrong", 1)] // Error [CS1729] - ctor doesn't exist
        // Secondary@+1 [CS0017] Program has more than one entry point
        public static int Main(string[] args)
        {
            return 1;
        }
    }

    class Program_10
    {
        public static async Task Main()
        {
            await Task.CompletedTask;
        }
    }
}
