Apple apple = new();
Fruit f = apple;

var b = f is Apple;     // Compliant
b = f is not Apple;     // Compliant
b = apple.GetType() == typeof(int?);                // Compliant
b = apple.GetType().IsInstanceOfType(f.GetType());  // Compliant
b = f as Apple == null;         // Noncompliant
b = f as Apple is null;         // Noncompliant
b = f as Apple is not null;     // Noncompliant
b = f as Apple is not not not not null; // Noncompliant
b = f as Apple is Apple { Size: 42 };   // Compliant
b = f as Apple is Apple;        // Noncompliant
b = true && (apple) is Apple;   // Noncompliant
b = !(apple is Apple);          // Noncompliant

if (apple is { }) { }           // Compliant

b = apple is ("Sweet", "Red");
b = apple is { Taste: "Sweet", Color: "Red" };

record Fruit { public int Size { get; } }
sealed record Apple : Fruit
{
    public string Taste;
    public string Color;
    public void Deconstruct(out string x, out string y) => (x, y) = (Taste, Color);
}
