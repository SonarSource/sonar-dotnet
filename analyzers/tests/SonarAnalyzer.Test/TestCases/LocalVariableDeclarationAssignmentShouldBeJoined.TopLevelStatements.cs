using System;

int x;      // Noncompliant {{Join the declaration and the assignment of this local variable.}}
x = 42;

int y = 0;  // Compliant - already initialized

int z;      // Compliant - intervening statement
Console.WriteLine(y);
z = 1;

string s;   // Noncompliant
s = "hello";

Console.WriteLine(x + z + s);

public class MyClass
{
    public void Method()
    {
        int a;  // Noncompliant
        a = 1;
    }
}
