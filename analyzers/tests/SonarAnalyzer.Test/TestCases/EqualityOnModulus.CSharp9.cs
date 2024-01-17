using System;
using System.Collections.Generic;

nint x = 100;
var y = x % 2 == 1; // Noncompliant; if x is negative, x % 2 == -1
y = x % 2 != -1; // Noncompliant {{The result of this modulus operation may not be negative.}}
y = 1 == x % 2; // Noncompliant {{The result of this modulus operation may not be positive.}}

nuint unsignedX = 100;
var xx = unsignedX % 4 == 1;
