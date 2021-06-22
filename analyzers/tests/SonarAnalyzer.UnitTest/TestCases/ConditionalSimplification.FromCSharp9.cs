using System;

Apple a = null, b = null;
bool condition = false;

a = a is not null ? (a) : b; // Noncompliant a ??= b;
a = a is null ? (b) : a; // Noncompliant a ??= b;

Apple x;
if (a is not null) // Noncompliant {{Use the '??' operator here.}}
{
    x = a;
}
else
{
    x = b;
}

if (a is null) // Noncompliant {{Use the '??=' operator here.}}
{
    a = b;
}

a = (a is not null) ? a : Identity(new()); // Noncompliant {{Use the '??=' operator here.}}
b = (a is not null) ? Identity(a) : Identity(new()); // Noncompliant {{Use the '??' operator here.}}
a = a ?? new(); // Noncompliant {{Use the '??=' operator here.}}

Fruit elem;
if (condition) // FN, C# 9 has target typed conditionals
{
    elem = new Apple();
}
else
{
    elem = new Orange();
}

Apple Identity(Apple o) => o;

// we ignore lambdas because of type resolution for conditional expressions, see CS0173
Action<int, int> LambdasAreIgnored(bool condition)
{
    Action<int, int> myAction;
    if (condition)
    {
        myAction = static (int i, int j) => { };
    }
    else
    {
        myAction = static (_, _) => { Console.WriteLine(); };
    }
    return myAction;
}

int NestedExpressionWithSwitch(bool condition, int a, int b)
{
    if (condition) // Compliant, changing this will lead to nested conditionals
    {
        return a switch
        {
            > 10 => 1,
            < 10 => 2,
            10 => 3
        };
    }
    else
    {
        return b;
    }
}

abstract class Fruit { }
class Apple : Fruit { }
class Orange : Fruit { }
