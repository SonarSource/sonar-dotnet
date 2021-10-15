using System.Collections.Generic;

class ConditionalsWithSameCondition
{
    public void Foo()
    {
        Fruit f = null;

        if (f is { Property.Count: >= 5 }) { }
        if (f is { Property.Count: >= 5 }) { } // Noncompliant
    }
}

class Fruit
{
    public List<int> Property { get; }
}
