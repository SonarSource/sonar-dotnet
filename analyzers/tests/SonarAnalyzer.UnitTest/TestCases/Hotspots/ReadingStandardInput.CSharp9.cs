using System;

string value;
int code;
ConsoleKeyInfo key;

code = Console.Read(); // Noncompliant {{Make sure that reading the standard input is safe here.}}
value = Console.ReadLine(); // Noncompliant
code = Console.Read(); // Noncompliant
key = Console.ReadKey(); // Noncompliant
key = Console.ReadKey(true); // Noncompliant

(key, code) = (Console.ReadKey(), 42); // Noncompliant

Console.OpenStandardInput(); // Noncompliant
Console.OpenStandardInput(100); // Noncompliant

var x = Console.In; // Noncompliant
Console.In.Read(); // Noncompliant

Console.Read(); // Compliant, value is ignored
Console.ReadLine(); // Compliant, value is ignored
Console.ReadKey(); // Compliant, value is ignored
Console.ReadKey(true); // Compliant, value is ignored

Console.Write(1);
Console.WriteLine(1);

record Record
{
    public Record()
    {
        string line = Console.ReadLine(); // Noncompliant
    }
}
