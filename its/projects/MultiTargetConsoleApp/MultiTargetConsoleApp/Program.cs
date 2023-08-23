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
            Console.WriteLine("\tfrom .Net 7"); // FIXME: .Net 7
#elif NET48
            Console.WriteLine("\tfrom .Net 4.8"); // FIXME: .Net Framework 4.8
#else
            Console.WriteLine("\tfrom other"); // FIXME: Other
#endif

            Console.ReadKey();
        }
    }
}
