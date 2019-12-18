using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public Program(char choice, int value)
        {
            switch (choice)
            {
                case 'Y':
                    Console.WriteLine("Yes");
                    break;
                case 'M':
                    Console.WriteLine("Maybe");
                    break;
                case 'N':
                    Console.WriteLine("No");
                    break;
                default:
                    switch (value) // Noncompliant
//                  ^^^^^^
                    {
                        case 0:
                            break;
                        default:
                            break;
                    }
                    Console.WriteLine("Invalid response");
                    break;
            }


            int i = 1;
            switch (i)
            {
                case 1:
                case 2:
                    Console.WriteLine("One or Two");
                    break;
                default:
                    Console.WriteLine("Other");
                    break;
            }

            string first = "foo", second = "bar";

            var result = first switch
            {
                "a" => second switch // Compliant
                {
                    "x" => 9,
                    _ => 1
                },
                "b" => 2,
                _ => 3
            };
        }
    }
}
