using System;
using Point2D = (int, int);

Point2D p = new(1, 2);

if (p.GetType().IsInstanceOfType(typeof(Point2D))) // Noncompliant
{ /* ... */ }

typeof(Point2D).IsInstanceOfType("42"); // Compliant
typeof(Point2D).IsInstanceOfType(typeof(Point2D)); // Noncompliant
