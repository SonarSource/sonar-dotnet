var a1 = false;
if (a1)             // Noncompliant
{
    DoSomething();  // Secondary
}

void DoSomething() { }
