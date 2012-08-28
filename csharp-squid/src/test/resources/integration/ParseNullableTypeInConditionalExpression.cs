using System;

class Program
{
    static void Main(string[] args)
    {
        int? answer = 42;

        int test = answer is int ? 1 : 0;
        test = answer is int? ? 1 : 0;
        test = answer is int   ? ? 1 : 0;

        Console.WriteLine("test is " + test);
        Console.ReadLine();
    }
}
