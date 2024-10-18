#nullable enable
using System;

int target = 32; // Noncompliant {{Add the 'const' modifier to 'target'.}}
//  ^^^^^^
int usedTarget = 40; // Compliant
const int alreadyConst = 32;

(int i, usedTarget) = (target, target);
(usedTarget, int k) = (alreadyConst, alreadyConst);

var s1 = $"This is a {nameof(target)}";      // Noncompliant {{Add the 'const' modifier to 's1', and replace 'var' with 'string?'.}}
string s2 = $"This is a {nameof(target)}";   // Noncompliant {{Add the 'const' modifier to 's2'.}}
var s3 = "This is a" + $" {nameof(target)}"; // Noncompliant {{Add the 'const' modifier to 's3', and replace 'var' with 'string?'.}}
var s4 = $@"This is a {nameof(target)}";     // Noncompliant {{Add the 'const' modifier to 's4', and replace 'var' with 'string?'.}}
FormattableString s6 = $"hello";             // Compliant
#nullable disable

if (target == alreadyConst)
{
}

nint foo = 42; // Noncompliant

nuint bar = 31;
bar++;

nint foo1 = IntPtr.Zero; // Compliant - IntPtr.Zero is not compile time constant
nuint bar2 = UIntPtr.Zero; // Compliant - UIntPtr.Zero is not compile time constant

if (target == (int)bar)
{
}

int i1 = 32; // Noncompliant 
const int i2 = 32; // Compliant

IntPtr ptrFoo = 42; // Noncompliant
UIntPtr foo2 = 42; // Noncompliant

const IntPtr constFoo = 42; // Compliant
const UIntPtr constFoo2 = 42; // Compliant

IntPtr ptrBar = 31; // Compliant
ptrBar++;

IntPtr ptrBar2 = 31; // Compliant
ptrBar2++;

IntPtr zero1 = IntPtr.Zero; // Compliant - IntPtr.Zero is not compile time constant
UIntPtr zero2 = UIntPtr.Zero; // Compliant - UIntPtr.Zero is not compile time constant

IntPtr compared = 42; // Noncompliant - (==) does not alter the value, should be const

if (compared == 42)
{
}

Func<nint> staticLambda1 = static () =>
{
    IntPtr v = 41; // Noncompliant
    return v;
};

Func<nuint> staticLamba2 = static () =>
{
    UIntPtr v = 41; // Noncompliant
    return v;
};

Func<int, int> staticLambda = static (t) =>
{
    int v = 41; // Noncompliant
    return v;
};

Func<int, int, int> withDiscard = static (_, _) =>
{
    int v = 41; // Noncompliant
    return v;
};

public record Rec
{
    public Rec()
    {
        int target = 32; // Noncompliant {{Add the 'const' modifier to 'target'.}}
//          ^^^^^^
        if (target == 1)
        {
        }
    }

    public void Test_Shadowing()
    {
        int shadowed = 0;
        shadowed++;

        void LocalFunction()
        {
            int shadowed = 0; // Noncompliant
        }

        Action a1 = () =>
        {
            int shadow = 0; // Noncompliant
        };

        Action<int> a2 = x =>
        {
            int shadow = 0; // Noncompliant
        };

        Action a3 = delegate ()
        {
            int shadow = 0; // Noncompliant
        };
    }
}

public record RecordWithImplicitOperator
{
    public static implicit operator RecordWithImplicitOperator(string value) { return new RecordWithImplicitOperator(); }
}

public class Repro_3271
{
    public RecordWithImplicitOperator ImplicitOperator()
    {
        RecordWithImplicitOperator x = "Lorem";
        return x;
    }

    public Rec NormalRecordSetToNull()
    {
        Rec x = null;   // Noncompliant
        return x;
    }
}
