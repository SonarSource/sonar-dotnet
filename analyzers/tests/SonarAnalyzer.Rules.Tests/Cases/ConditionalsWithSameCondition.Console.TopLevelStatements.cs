// version: CSharp9
using System;

int a = 0, b = 1;

if (a == b) { Foo(); }
if (a == b) { Bar(); }

if (args[0] == args[1])
{
    Foo();
}

if (args[0] == args[1]) // FN
{
    Bar();
}

void Foo() { }
void Bar() { }
