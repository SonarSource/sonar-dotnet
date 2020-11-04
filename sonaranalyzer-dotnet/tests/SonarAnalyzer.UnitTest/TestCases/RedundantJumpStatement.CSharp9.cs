using System;

var a = new Action(() =>
{
    return; // Noncompliant
//  ^^^^^^^
});

goto A; // Compliant - FN
A:
return; // Compliant - FN

Action<int> b = static i =>
{
    return;  // Noncompliant
};

static void LocalMethod()
{
    return; // Noncompliant
};

record Record
{
    int Prop
    {
        init
        {
            goto A; // Compliant - FN
A:
            return; // Compliant - FN
        }
    }
}
