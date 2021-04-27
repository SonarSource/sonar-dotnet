using System;

var x = 100; // FN
x = 1;

void TargetTypedNew()
{
    Decimal d = new(100f); // FN
    d = new(2f);
}

void NativeInts(nuint param)
{
    param = 1; // Noncompliant

    nuint zero = 0; // ignored value
    zero = 1;
    Foo(zero);

    nint minusOne = -1; // ignored value
    minusOne = 1;
    Foo(minusOne);

    nint one = 1; // ignored value
    one = 2;
    Foo(one);

    nint two = 2; // Noncompliant
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
        a = 1;
    }
    else if (b is 5)
    {
        a = 2; // Compliant - FN, the parameter value is overwritten on all possible paths
    }

    var c = 5;
    switch (c)
    {
        case < 5:
            c = 6; // Compliant, FN
            break;
        case >= 5:
            c = 7; // Compliant, FN
            break;
    }
}



Action<int, int, int> StaticLambda() =>
    static (int a, int _, int _) =>
    {
        a = 100;        // FN, the outer statement is a local function and that is muted
        int b = 100;    // FN, the outer statement is a local function and that is muted
        b = 1;          // FN, the outer statement is a local function and that is muted
    };

void Foo(object o) { }

public class C
{

    public static void Log() { }
    unsafe void FunctionPointer()
    {
        delegate*<void> ptr1 = &C.Log; // Noncompliant
        ptr1 = &C.Log; // Noncompliant
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
            value = 1; // FN
            int a = 100;
            a = 2; // FN
        }
    }
}
