Apple apple = new();
Fruit f = apple;

var b = f as Apple is Apple { Prop.Length: 42 };   // Compliant
b = f as Apple == null;         // Noncompliant

record Fruit
{
    public int[] Prop { get; }
}

sealed record Apple : Fruit
{
}
