using System;

namespace NetCore31
{
    class Program
    {
        static void Main(string[] args)
        {
            // Next line triggers S1481 to make sure issues are raised
            var foo = 42;
			Console.WriteLine("Hello World!");
        }
    }
}
