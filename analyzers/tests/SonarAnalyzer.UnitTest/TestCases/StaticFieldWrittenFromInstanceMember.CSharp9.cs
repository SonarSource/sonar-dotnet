using System;

Record.count++; // Compliant - static field set from static method

record Record
{
    internal static int count = 0;
//                      ^^^^^^^^^ Secondary
//                      ^^^^^^^^^ Secondary@-1

    public void Method() => count++; // Noncompliant

    public static void DoSomethingStatic() => count++; // Compliant

    public Action<int> Foo() => static x =>
    {
        count += x; // Noncompliant FP (?) - static lambda
    };

    public void CallFoo() => Foo()(1); // count gets incremented from a static member
}

class Class
{
    private static string staticVar; // Secondary
    private int var;

    public int MyProperty
    {
        get { return var; }
        init
        {
            staticVar = "42"; // Noncompliant
            staticVar += "42"; // Compliant, already reported on this symbol
            var = value;
        }
    }
}
