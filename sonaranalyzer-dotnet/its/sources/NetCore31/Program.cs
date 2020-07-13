using System;

namespace NetCore31
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            if (args[0] == null)
            {
                Console.WriteLine(args[0].ToString());
            }
        }
    }
}
