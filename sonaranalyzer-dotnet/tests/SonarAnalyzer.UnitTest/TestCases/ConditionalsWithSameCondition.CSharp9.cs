string c = null;
if (c is null)
{
    doTheThing(c);
}
if (c is null) // FN
{
    doTheThing(c);
}
if (c is not null) // Compliant
{
    doTheThing(c);
}

if (c is "banana") // Compliant, c might be updated in the previous if
{
    c += "a";
}
if (c is "banana") // Compliant, c might be updated in the previous if
{
    c = "";
}
if (c is "banana") // Compliant, c might be updated in the previous if
{
    c = "";
}

int i = 0;
if (i is > 0 and < 100)
{
    doTheThing(i);
}
if (i is > 0 and < 100) // FN
{
    doTheThing(i);
}

Fruit f = null;
bool cond = false;
if (f is Apple)
{
    f = cond ? new Apple() : new Orange();
}
if (f is Apple) // Compliant as f may change
{
    f = cond ? new Apple() : new Orange();
}

void doTheThing(object o) { }

abstract class Fruit { }
class Apple : Fruit { }
class Orange : Fruit { }
