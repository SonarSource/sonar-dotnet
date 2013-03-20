using System;

class Program
{
    static void Main(string[] args)
    {
        int foo = 0;
        switch (foo)
        {
            case 0:                                   // Non-Compliant
                Console.WriteLine("foo = 0");
            case 1:                                   // Non-Compliant
                Console.WriteLine("foo = 1");
            case 2:                                   // Compliant
                Console.WriteLine("foo = 2");
                break;
            case 3:                                   // Compliant
            case 4:                                   // Compliant
            case 5:                                   // Compliant
                Console.WriteLine("foo = 3, 4 or 5");
                break;
            case 6:                                   // Compliant
            case 7:                                   // Non-Compliant
                Console.WriteLine("foo = 6 or 7");
        }

        switch (foo)
        {
            case 0:                                   // Compliant
                Console.WriteLine("foo = 0");
                break;
            case 1:                                   // Compliant
                Console.WriteLine("foo = 1");
                break;
            case 2:                                   // Compliant
                Console.WriteLine("foo = 2");
                break;
            case 3:                                   // Compliant
            case 4:                                   // Compliant
            case 5:                                   // Compliant
                Console.WriteLine("foo = 3, 4 or 5");
                break;
        }
    }
}
