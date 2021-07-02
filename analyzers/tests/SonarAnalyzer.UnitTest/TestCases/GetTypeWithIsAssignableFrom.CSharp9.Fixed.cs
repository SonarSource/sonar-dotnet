Apple apple = new();
Fruit f = apple;

var b = f is Apple;     // Compliant
b = f is not Apple;     // Compliant
b = apple.GetType() == typeof(int?);                // Compliant
b = apple.GetType().IsInstanceOfType(f.GetType());  // Compliant
b = !(f is Apple);         // Fixed
b = !(f is Apple);         // Fixed
b = f is Apple;     // Fixed
b = !(f is Apple); // Fixed
b = f as Apple is Apple { Size: 42 };   // Compliant
b = f is Apple;        // Fixed
b = true && (apple != null);   // Fixed
b = !(apple != null);          // Fixed

if (apple is { }) { }           // Compliant

record Fruit { public int Size { get; } }
sealed record Apple : Fruit { }
