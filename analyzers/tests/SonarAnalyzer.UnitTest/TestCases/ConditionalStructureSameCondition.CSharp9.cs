using System.Collections.Generic;

object o;
int i;

void SimpleTest()
{
    if (o is not null) // Secondary [flow1]
//      ^^^^^^^^^^^^^
    {
        // foo
    }
    else if (o is not null) // Noncompliant [flow1] {{This branch duplicates the one on line 8.}}
    {
        var x = 1;
    }
}

void Test(Apple a, Orange b, bool cond)
{
    if (i is > 0 and < 100) // Secondary [flow2,flow3]
    {

    }
    else if (i is > 0 and < 100) // Noncompliant [flow2]
    {

    }
    else if (i is < 0 or > 100) // Secondary [flow4]
    {

    }
    else if (i is > 0 and < 100) // Noncompliant [flow3]
    {

    }
    else if (i is < 0 or > 100) // Noncompliant [flow4]
    {

    }

    Fruit f;
    if ((f = cond ? a : b) is Orange) // Secondary [flow5]
    {
    }
    else if ((f = cond ? a : b) is Orange) // Noncompliant [flow5]
    {
    }

    if (f is { Sweetness: 42 }) // Secondary [flow6]
    {
    }
    else if (f is { Sweetness: 42 }) // Noncompliant [flow6]
    {
    }

    if (f is { Checks: { Count: 42 } }) // Secondary [flow7]
    {
    }
    else if (f is { Checks: { Count: 42 } }) // Noncompliant [flow7]
    {
    }
}

void AnotherTest(object o)
{
    if (o is not null) { }  // Secondary
    else if (o != null) { } // Noncompliant

    if (o is null) { }      // Secondary
    else if (o == null) { } // Noncompliant
}

abstract class Fruit
{
    public int Sweetness;
    public List<int> Checks;
}
class Apple : Fruit { }
class Orange : Fruit { }
