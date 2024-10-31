using System;
using Person = (string, string);

public class Sample(string str)
{
    public void Method()
    {
        string sample1 = $"{nameof(str)}";    // Noncompliant, compile-time interpolated string 
        string sample2 = nameof(Person);      // Noncompliant, nameof of alias is compile-time
        string sample3 = $"{nameof(Person)}"; // Noncompliant
    }

    //https://sonarsource.atlassian.net/browse/NET-534
    public void Test()
    {
        int a = 42; // Noncompliant
        ref readonly int r = ref a;
        
        int b = 42; // Compliant
        ref int x = ref b;
        x++;

        int c = 42; // Compliant
        ReadonlyRef(ref c);

        int d = 42; // Compliant
        InRef(in d);
    }

    public void ReadonlyRef(ref readonly int number)
    {
        int x = number;
    }

    public void InRef(in int number)
    {
        var z = number;
    }
}

