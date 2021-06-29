Apple apple = new();
Fruit f = apple;

var b = f is Apple; // Compliant
b = f is not Apple; // Compliant
b = apple.GetType() == typeof(int?); // Compliant
b = apple.GetType().IsInstanceOfType(f.GetType()); // Compliant
b = !(f is Apple); // Fixed
b = f as Apple is null; // FN
b = f as Apple is not null; // FN
b = true && (apple != null); // Fixed
b = !(apple != null); // Fixed

if (apple is { }) { } // Compliant

record Fruit { }
sealed record Apple : Fruit { }
