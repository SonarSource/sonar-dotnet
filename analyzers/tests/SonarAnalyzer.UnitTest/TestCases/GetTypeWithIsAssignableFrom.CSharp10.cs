Apple apple = new();
Fruit f = apple;

var b = f as Apple is Apple { Prop.Length: 42 };   // Compliant
b = f as Apple == null;         // Noncompliant

record Fruit { public int[] Prop { get; } }
sealed record Apple : Fruit
{
    public string Taste;
    public string Color;
    public void Deconstruct(out string x, out string y) => (x, y) = (Taste, Color);
}
