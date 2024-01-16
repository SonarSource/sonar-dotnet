Apple apple = new();
Fruit f = apple;

var b = f as Apple is Apple { Prop.Length: 42 };   // Compliant
b = !(f is Apple);         // Fixed

record Fruit
{
    public int[] Prop { get; }
}

sealed record Apple : Fruit
{
}
