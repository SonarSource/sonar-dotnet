using System;

Console.WriteLine("Hello, World!");

class InnerClass { }

class inner_class { } // Noncompliant {{Rename class 'inner_class' to match pascal case naming rules, consider using 'Innerclass'.}}
