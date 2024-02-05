using System;

Apple a = null, b = null;
int c = 42;
bool condition = false;

a ??= b; // Fixed
a ??= b; // Fixed

Apple x;
if (a is not null) // Compliant
{
    x = a;
}
else
{
    x = c; // Error [CS0029]
}

x = a ?? b;

x = a ?? b;

a ??= b;

a ??= Identity(new()); // Fixed
b = Identity(a ?? new()); // Fixed
a ??= new(); // Fixed
a = a is not null ? Wrong(a, b) : Identity(new()); // Error [CS0103]
a = a is not null ? Identity(new()) : Wrong(a, b); // Error [CS0103]
a = a is not null ? Identity(a) : IdentityOther(a, b);

var p = a is null; // Fixed
var q = a is not null; // Fixed
var r = a is not null; // Fixed
var s = a is null; // Fixed
if (a is null) // Fixed
{
}

int v = 0;
if (v is not 1)
{
    v = 1;
}

switch (a)
{
    case not null:
        break;
    case null: // Fixed
        break;
}

var y = a switch
{
    not null => 1,
    null => 0 // Fixed
};

Fruit elem;
elem = condition ? new Apple() : new Orange();

Apple Identity(Apple o) => o;
Apple IdentityOther(Apple first, Apple second) => second;

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
