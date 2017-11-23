using System;

namespace MultiTargetConsoleApp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            int unusedVar = 42;

            Console.WriteLine("Hello World!");
#if NETCOREAPP2_0
            Console.WriteLine("\tfrom .Net Core 2.0"); // FIXME: .Net Core 2
#elif NET46
            Console.WriteLine("\tfrom .Net Fwk 4.6"); // FIXME: .Net fwk 4.6
#else
            Console.WriteLine("\tfrom other"); // FIXME: Other
#endif

            Console.ReadKey();
        }
    }
}
