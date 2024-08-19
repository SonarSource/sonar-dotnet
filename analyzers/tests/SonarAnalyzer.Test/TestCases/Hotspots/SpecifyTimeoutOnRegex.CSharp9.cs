using System;
using System.Text.RegularExpressions;

class Compliant
{
    void ImplicitObject()
    {
        Regex defaultOrder = new("some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
    }
}

class Noncompliant
{
    void ImplicitObject()
    {
        Regex patternOnly = new("some pattern"); // Noncompliant {{Pass a timeout to limit the execution time.}}
        //                  ^^^^^^^^^^^^^^^^^^^
        Regex withOptions = new("some pattern", RegexOptions.None); // Noncompliant
    }
}
