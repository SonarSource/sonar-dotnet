using System;

class Program
{
    static void Main(string[] args)
    {
        int i = 0;
        while (true)
        {
            if (i == 10)
            {
                break;     // Non-Compliant
            }

            Console.WriteLine(i);
            i++;
        }

        int j = 0;
        while (j != 10)    // Compliant
        {
            Console.WriteLine(j);
            j++;
        }

        int foo = 0;
        switch (foo)
        {
            case 0:
                Console.WriteLine("foo = 0");
                break;     // Compliant
            case 1:
                do {
                    break; // Non-Compliant
                } while (true);
                break;     // Compliant
        }

        break;             // Non-Compliant
    }
}
