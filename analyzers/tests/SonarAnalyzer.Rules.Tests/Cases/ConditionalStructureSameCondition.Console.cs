// version: CSharp9
object o;
int i;

void SimpleTest()
{
    if (o is not null) // Secondary [flow1]
//      ^^^^^^^^^^^^^
    {
        // foo
    }
    else if (o is not null) // Noncompliant [flow1] {{This branch duplicates the one on line 7.}}
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
}

void AnotherTest(object o)
{
    if (o is not null) { }
    else if (o != null) { } // FN, same as above

    if (o is null) { }
    else if (o == null) { } // FN, same as above
}

abstract class Fruit { }
class Apple : Fruit { }
class Orange : Fruit { }
