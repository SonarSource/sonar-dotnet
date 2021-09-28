using System;

var x = 100; // FIXME: Parent symbol defining local scope is not correct here. Non-compliant
x = 1;       // FIXME: Parent symbol defining local scope is not correct here. Non-compliant

void TargetTypedNew()
{
    Decimal d = new(100f);  // Noncompliant
    d = new(2f);            // Noncompliant
}

void NativeInts(nuint param)
{
    param = 1;      // Noncompliant

    nuint zero = 0; // Compliant, ignored value
    zero = 1;
    Foo(zero);

    nint minusOne = -1; // Compliant, ignored value
    minusOne = 1;
    Foo(minusOne);

    nint one = 1;       // Compliant, ignored value
    one = 2;
    Foo(one);

    nint two = 2;       // Noncompliant
    two = 3;
    Foo(two);
}

void PatternMatch(object param)
{
    object a = param;
    if (a is not null)
    {
        a = null; // Compliant
        Foo(a);
    }

    int i = 100;
    if (i is not > 50 and < 200)
    {
        i = 2;
        Foo(i);
    }
}

void PatternMatchFalseNegative(int a, int b)
{
    if (b is not 5)
    {
        a = 1;  // Noncompliant
    }
    else if (b is 5)
    {
        a = 2;  // Noncompliant
    }

    var c = 5;
    switch (c)
    {
        case < 5:
            c = 6; // Noncompliant
            break;
        case >= 5:
            c = 7; // Noncompliant
            break;
    }
}



Action<int, int, int> StaticLambda() =>
    static (int a, int _, int _) =>
    {
        a = 100;        // FN
        int b = 100;    // Noncompliant
        b = 1;          // FN
    };

void Foo(object o) { }

public class C
{

    public static void Log() { }
    unsafe void FunctionPointer()
    {
        delegate*<void> ptr1 = &C.Log;  // Noncompliant
        ptr1 = &C.Log;                  // Noncompliant
    }

    Action<int, int, int> StaticLambda() =>
        static (int a, int _, int _) =>
        {
            a = 100;        // Noncompliant
            int b = 100;    // Noncompliant
            b = 1;          // Noncompliant
        };

}

record R
{
    public R(int x)
    {
        x = 1; // Noncompliant
    }

    int x;
    public int InitProperty
    {
        init
        {
            value = 1;      // Noncompliant
            int a = 100;    // Noncompliant
            a = 2;          // Noncompliant
        }
    }
}
