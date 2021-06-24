using System;

Apple a = null, b = null;
bool condition = false;

a ??= b; // Fixed
a ??= b; // Fixed

Apple x;
x = a ?? b;

a ??= b;

a ??= Identity(new()); // Fixed
b = Identity(a ?? new()); // Fixed
a ??= new(); // Fixed

var p = a is null; // Fixed
var q = a is not null; // Fixed
var r = a is not null; // Fixed
var s = a is null; // Fixed
if (a is null) // Fixed
{
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
