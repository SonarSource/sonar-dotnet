using System;

int b = 0;

if (b == 0)  // Noncompliant {{Remove this conditional structure or edit its code blocks so that they're not all the same.}}
{
    DoSomething();
}
else if (b == 1)
{
    DoSomething();
}
else
{
    DoSomething();
}

void DoSomething()
{
}
