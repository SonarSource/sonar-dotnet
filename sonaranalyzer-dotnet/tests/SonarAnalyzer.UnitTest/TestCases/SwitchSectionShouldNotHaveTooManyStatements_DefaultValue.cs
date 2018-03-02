using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public Program()
        {
            switch (myVariable)
            {
                case 0:
                    break;
                case 1: // Noncompliant {{Reduce this 'switch/case' number of lines from 6 to at most 5, for example by extracting code into a method.}}
//              ^^^^^^^
                    Console.WriteLine("1");
                    Console.WriteLine("2");
                    Console.WriteLine("3");
                    Console.WriteLine("4");
                    Console.WriteLine("5");
                    break;
                case 2:
                    {
                        Console.WriteLine("1");
                        Console.WriteLine("2");
                        Console.WriteLine("3");
                        Console.WriteLine("4");
                        break;
                    }
                case 3: // Noncompliant {{Reduce this 'switch/case' number of lines from 6 to at most 5, for example by extracting code into a method.}}
                case 4:
                    if (true)
                    {
                        if (false)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                Console.WriteLine();
                                Console.WriteLine();
                                Console.WriteLine();
                            }
                        }
                    }
                case 5: // Noncompliant
                    var x = Foo(42);
                    break;

                    string Foo(int value)
                    {
                        Console.WriteLine("1");
                        Console.WriteLine("2");
                        Console.WriteLine("3");
                        Console.WriteLine("4");
                        Console.WriteLine("5");
                    }
                case:
                default:
                    break;
            }
        }
    }
}
