// Noncompliant {{Make sure that command line arguments are used safely here.}}
using System;

Console.WriteLine(args[0]);

static void Main(string arg) // Compliant, this is a local method named `Main`
{
    Console.WriteLine(arg);
}
