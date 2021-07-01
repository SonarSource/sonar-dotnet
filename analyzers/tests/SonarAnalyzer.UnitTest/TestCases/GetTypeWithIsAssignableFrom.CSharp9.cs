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
//FIXME: b = f as Apple is Apple;        // Non-compliant
b = true && (apple) is Apple;   // Noncompliant
b = !(apple is Apple);          // Noncompliant

if (apple is { }) { }           // Compliant

record Fruit { public int Size { get; } }
sealed record Apple : Fruit { }
