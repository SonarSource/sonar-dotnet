using System;

public class Program
{
    public void Base(string[] myArray)
    {
        Method(new string[] { "s1", "s2" }); // Noncompliant

        Method("s1");           // Compliant
        Method("s1", "s2");     // Compliant
        Method(myArray);        // Compliant
        Method(new string[12]); // Compliant

        Method2(1, new string[] { "s1", "s2" }); // Noncompliant

        Method2(1, "s1");           // Compliant
        Method2(1, "s1", "s2");     // Compliant
        Method2(1, myArray);        // Compliant
        Method2(1, new string[12]); // Compliant
    }

    public void Method(params string[] args) { }

    public void Method2(int a, params string[] args) { }
}
