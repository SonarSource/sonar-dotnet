using System;

Structure.count++; // Compliant - static field set from static method

struct Structure
{
    internal static int count = 0;
//                      ^^^^^^^^^ Secondary
//                      ^^^^^^^^^ Secondary@-1

    public void Method() => count++; // Noncompliant

    public static void DoSomethingStatic() => count++; // Compliant

    public static Action<int> StaticFoo() => static x =>
    {
        count += x; // compliant because it can only be used in a static context
    };

    public Action<int> Foo() => static x =>
    {
        count += x; // Noncompliant {{Make the enclosing instance method 'static' or remove this set on the 'static' field.}}
    };

    public void CallFoo() => Foo()(1); // 'count' gets incremented from an instance member
}
