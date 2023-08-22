using System;

namespace MultiTargetConsoleApp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            int unusedVar = 42;

            Console.WriteLine("Hello World!");
#if NET7_0
            Console.WriteLine("\tfrom .Net Core 2.0"); // FIXME: .Net 7
#elif NET48
            Console.WriteLine("\tfrom .Net Fwk 4.6"); // FIXME: .Net fwk 4.8
#else
            Console.WriteLine("\tfrom other"); // FIXME: Other
#endif

            Console.ReadKey();
        }
    }
}
