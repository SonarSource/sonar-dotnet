using System;

public class Program
{
    public void Base(string[] myArray)
    {
        Method(new string[] { "s1", "s2" }); // Noncompliant {{Arrays should not be created for params parameters.}}
//             ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Method(new string[] { "s1" }); // Noncompliant
        Method("s1");           // Compliant
        Method("s1", "s2");     // Compliant
        Method(myArray);        // Compliant
        Method(new string[12]); // Compliant

        Method2(1, new string[] { "s1", "s2" }); // Noncompliant {{Arrays should not be created for params parameters.}}
//                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Method2(1, new string[] { "s1" }); // Noncompliant
        Method2(1, "s1");           // Compliant
        Method2(1, "s1", "s2");     // Compliant
        Method2(1, myArray);        // Compliant
        Method2(1, new string[12]); // Compliant

        Method3(new string[] { "s1", "s2" }, "s1"); // Compliant
        Method3(new string[] { "s1", "s2" }, new string[12]); // Compliant
        Method3(new string[] { "s1", "s2" }, new string[] { "s1", "s2" }); // Noncompliant
//                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Method3(null, null);        // Compliant
        Method4(new [] { "s1" });   // Compliant
        Method4(new [] { "s1", "s2" }); // Compliant

        Method2(args: new string[] { "s1", "s2" }, a: 1); // FN
        Method3(args: new string[12], a: new string[] { "s1", "s2" }); // Noncompliant FP

        var s = new MyClass(1, new int[] { 2, 3 }); // Noncompliant
//                             ^^^^^^^^^^^^^^^^^^
        var s1 = new MyClass(1, 2, 3); // Compliant
    }

    public void Method(params string[] args) { }

    public void Method2(int a, params string[] args) { }

    public void Method3(string[] a, params string[] args) { }

    public void Method4(object[] a, params object[] args) { }

    public void Method5(params string[] a, params string[] args) { } // Error [CS0231]
}

public class MyClass
{
    public MyClass(int a, params int[] args) { }
}
