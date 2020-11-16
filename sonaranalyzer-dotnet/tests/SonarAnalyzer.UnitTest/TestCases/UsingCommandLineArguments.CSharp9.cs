using System;

Console.WriteLine(args[0]); // FN

static void Main(string arg) // Compliant, this is a local method named `Main`
{
    Console.WriteLine(arg);
}
