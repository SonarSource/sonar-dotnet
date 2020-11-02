using System;

int target = 32; // Noncompliant {{Add the 'const' modifier to 'target'.}}
//  ^^^^^^
const int alreadyConst = 32;

if (target == alreadyConst)
{
}

nint foo = 42; // Noncompliant

nuint bar = 31; // Noncompliant - FP
bar++;

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

record Rec
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

