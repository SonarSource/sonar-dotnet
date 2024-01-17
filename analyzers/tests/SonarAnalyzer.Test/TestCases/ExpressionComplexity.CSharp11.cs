using System;

_ = new[] { 1, 2, 3 } is [1 or 2 or 3 or 4 or 5 or 6, _, _]; // Noncompliant
_ = new[] { 1, 2, 3 } is [1 or 2 or 3, 1 or 2 or 3, _]; // Compliant
