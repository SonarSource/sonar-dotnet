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
}
