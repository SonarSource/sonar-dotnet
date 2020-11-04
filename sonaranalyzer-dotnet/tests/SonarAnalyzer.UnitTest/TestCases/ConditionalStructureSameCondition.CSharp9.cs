object o;
int i;

void SimpleTest()
{
    if (o is not null) // Secondary [line11]
//      ^^^^^^^^^^^^^
    {
        // foo
    }
    else if (o is not null) // Noncompliant [line11] {{This branch duplicates the one on line 6.}}
    {
        var x = 1;
    }
}

void Test(Apple a, Orange b, bool cond)
{
    if (i is > 0 and < 100) // Secondary [line23,line31]
    {

    }
    else if (i is > 0 and < 100) // Noncompliant [line23]
    {

    }
    else if (i is < 0 or > 100) // Secondary [line35]
    {

    }
    else if (i is > 0 and < 100) // Noncompliant [line31]
    {

    }
    else if (i is < 0 or > 100) // Noncompliant [line35]
    {

    }

    Fruit f;
    if ((f = cond ? a : b) is Orange) // Secondary [line44]
    {
    }
    else if ((f = cond ? a : b) is Orange) // Noncompliant [line44]
    {
    }
}

abstract class Fruit { }
class Apple : Fruit { }
class Orange : Fruit { }
