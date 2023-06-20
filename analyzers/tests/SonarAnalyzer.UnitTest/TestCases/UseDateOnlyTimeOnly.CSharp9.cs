using System;
using System.Globalization;

public class Program
{
    public void TargetTypedObjectCreation()
    {
        DateTime date1 = new(1993, 1, 6); // Noncompliant
        DateTime date2 = new(1, 1, 1, 12, 13, 14); // Noncompliant
        DateTime date3 = new(1993, 1, 6, 1, 1, 1); // Compliant
    }
}
