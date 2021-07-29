using System;

int target = 32; // Noncompliant {{Add the 'const' modifier to 'target'.}}
//  ^^^^^^
const int alreadyConst = 32;

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
