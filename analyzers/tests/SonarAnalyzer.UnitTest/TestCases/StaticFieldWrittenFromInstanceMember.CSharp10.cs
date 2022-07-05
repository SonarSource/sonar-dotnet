using System;

Structure.count++;                                        // Compliant - static field set from static method
RecordStructure.count++;                                  // Compliant - static field set from static method

struct Structure
{
    internal static int count = 0;
    //                  ^^^^^^^^^                            Secondary    [increment]
    //                  ^^^^^^^^^                            Secondary@-1 [compoundAdd]
    //                  ^^^^^^^^^                            Secondary@-2 [deconstruct]

    public void Method() => count++;                      // Noncompliant [increment]

    public static void DoSomethingStatic() => count++;    // Compliant

    public static Action<int> StaticFoo() => static x =>
    {
        count += x;                                       // Compliant because it can only be used in a static context
    };

    public Action<int> Foo() => static x =>
    {
        count += x;                                       // Noncompliant [compoundAdd] {{Make the enclosing instance method 'static' or remove this set on the 'static' field.}}
    };

    public Action<int> Foo2() => static x =>
    {
        (count, var y) = (42, "42");
//       ^^^^^^^^^^^^^^^                                     Noncompliant [deconstruct]
    };

    public void CallFoo() => Foo()(1);                    // 'count' gets incremented from an instance member
}

record struct RecordStructure
{
    internal static int count = 0;
    //                  ^^^^^^^^^                            Secondary    [RecordIncrement]
    //                  ^^^^^^^^^                            Secondary@-1 [RecordCompoundAdd]
    //                  ^^^^^^^^^                            Secondary@-2 [RecordDeconstruct]

    public void Method() => count++;                      // Noncompliant [RecordIncrement]

    public static void DoSomethingStatic() => count++;    // Compliant

    public static Action<int> StaticFoo() => static x =>
    {
        count += x;                                       // Compliant because it can only be used in a static context
    };

    public Action<int> Foo() => static x =>
    {
        count += x;                                       // Noncompliant [RecordCompoundAdd] {{Make the enclosing instance method 'static' or remove this set on the 'static' field.}}
    };

    public Action<int> Foo2() => static x =>
    {
        (count, var y) = (42, "42");                      // Noncompliant [RecordDeconstruct]
    };

    public void CallFoo() => Foo()(1);                    // 'count' gets incremented from an instance member
}
