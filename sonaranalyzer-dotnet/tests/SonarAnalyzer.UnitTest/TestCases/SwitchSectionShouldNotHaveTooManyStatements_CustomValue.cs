using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public Program(int myVariable)
        {
            switch (myVariable)
            {
                case 0:
                    break;
                case 1: // Noncompliant {{Reduce this switch section number of statements from 6 to at most 1, for example by extracting code into a method.}}
//              ^^^^^^^
                    Console.WriteLine("1");
                    Console.WriteLine("2");
                    Console.WriteLine("3");
                    Console.WriteLine("4");
                    Console.WriteLine("5");
                    break;
                default:
                    break;
            }
        }

        public int Test(string type)
        {
            return type switch // Compliant - FN
            {
                _ => 1
                       + 2
                       + 3
                       + 4
                       + 5
                       + 6
                       + 7
                       + 8
                       + 9
            };
        }
    }
}
