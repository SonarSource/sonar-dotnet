Apple apple = new();
Fruit f = apple;

var b = f is Apple; // Compliant
b = f is not Apple; // Compliant
b = apple.GetType() == typeof(int?); // Compliant
b = apple.GetType().IsInstanceOfType(f.GetType()); // Compliant
b = f as Apple == null; // Noncompliant
b = f as Apple is null; // Compliant - FN
b = f as Apple is not null; // Compliant - FN
b = true && (apple) is Apple; // Noncompliant
b = !(apple is Apple); // Noncompliant

record Fruit { }
sealed record Apple : Fruit { }
