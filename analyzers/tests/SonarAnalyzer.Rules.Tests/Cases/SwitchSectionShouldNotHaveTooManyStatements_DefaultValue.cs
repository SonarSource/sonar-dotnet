using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public Program(int myVariable)
        {
            var length = myVariable;
            switch (myVariable)
            {
                case 0:
                    break;
                case 1: // Noncompliant {{Reduce this switch section number of statements from 9 to at most 8, for example by extracting code into a method.}}
//              ^^^^^^^
                    Console.WriteLine("1");
                    Console.WriteLine("2");
                    Console.WriteLine("3");
                    Console.WriteLine("4");
                    Console.WriteLine("5");
                    Console.WriteLine("6");
                    Console.WriteLine("7");
                    Console.WriteLine("8");
                    break;
                case 2:
                    {
                        Console.WriteLine("1");
                        Console.WriteLine("2");
                        Console.WriteLine("3");
                        Console.WriteLine("4");
                        break;
                    }
                case 3: // Noncompliant {{Reduce this switch section number of statements from 10 to at most 8, for example by extracting code into a method.}}
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
                                Console.WriteLine();
                                Console.WriteLine();
                                Console.WriteLine();
                            }
                        }
                    }
                    break;
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
                        Console.WriteLine("6");
                        Console.WriteLine("7");

                        return "";
                    }
                case: // Error [CS0150]
                default:
                    break;
            }
        }
    }
}
