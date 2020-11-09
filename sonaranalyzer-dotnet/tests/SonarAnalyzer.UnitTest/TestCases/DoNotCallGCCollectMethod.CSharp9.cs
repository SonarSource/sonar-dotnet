using System;
using static System.GC;
GC.Collect(); // Noncompliant {{Refactor the code to remove this use of 'GC.Collect'.}}
GC.Collect(2, GCCollectionMode.Optimized); // Noncompliant
Collect(); // Noncompliant
