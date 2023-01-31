using System;

public class Program
{
    public void Base(string[] myArray)
    {
        Method(new string[] { "s1", "s2" }); // Noncompliant {{Remove this array creation and simply pass the elements.}}
//             ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Method(new string[] { "s1" }); // Noncompliant
        Method(new[] { "s1" }); // Noncompliant
        Method(new string[] { }); // Compliant
        Method("s1");           // Compliant
        Method("s1", "s2");     // Compliant
        Method(myArray);        // Compliant
        Method(new string[12]); // Compliant

        Method2(1, new string[] { "s1", "s2" }); // Noncompliant {{Remove this array creation and simply pass the elements.}}
//                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Method2(1, new string[] { "s1" }); // Noncompliant
        Method2(1, "s1");           // Compliant
        Method2(1, "s1", "s2");     // Compliant
        Method2(1, myArray);        // Compliant
        Method2(1, new string[12]); // Compliant

        Method3(new string[] { "s1", "s2" }); // Compliant
        Method3(new string[] { "s1", "s2" }, "s1"); // Compliant
        Method3(new string[] { "s1", "s2" }, new string[12]); // Compliant
        Method3(new string[] { "s1", "s2" }, new string[] { "s1", "s2" }); // Noncompliant
//                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Method3(null, null);        // Compliant
        Method4(new [] { "s1" });   // Compliant
        Method4(new [] { "s1", "s2" }); // Compliant

        Method3(args: new string[] { "s1", "s2" }, a: new string[12]); // Compliant (if you specifically require the arguments to be passed in this order there is no way of making this compliant, thus we shouldn't raise)
        Method3(args: new string[12], a: new string[] { "s1", "s2" }); // Compliant

        var s = new MyClass(1, new int[] { 2, 3 }); // Noncompliant
//                             ^^^^^^^^^^^^^^^^^^
        var s1 = new MyClass(1, 2, 3); // Compliant
        s1 = new MyClass(args: new int[] { 2, 3 }, a: 1); // Compliant (if you specifically require the arguments to be passed in this order there is no way of making this compliant, thus we shouldn't raise)
        var s2 = new MyOtherClass(args: new int[12], a: new int[] { 2, 3 }); // Compliant

        var s3 = new IndexerClass();
        var indexer1 = s3[new int[] { 1, 2 }]; // FN
        var indexer2 = s3?[new int[] { 1, 2 }]; // FN
        var indexer3 = s3[1, 2]; // Compliant
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

public class MyOtherClass
{
    public MyOtherClass(int[] a, params int[] args) { }
}

public class IndexerClass
{
    public int this[params int[] i] => 1;
}
