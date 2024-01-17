using System;
using System.Collections.Generic;
using System.Linq;

record R
{
    private static string[] staticStrings = new string[] { "a", "b", "c" };
    private string[] stringArray = new string[10];
    public string[] Property1
    {
        get { return (string[])stringArray.Clone(); } // Noncompliant {{Refactor 'Property1' into a method, properties should not copy collections.}}

        init { stringArray = (string[])staticStrings.Clone(); }
    }

    private List<string> foo = new();
    public IEnumerable<string> Property2
    {
        get => foo.ToArray();
    }

    public string[] Property3
    {
        init => stringArray = value.ToArray();
    }
}
