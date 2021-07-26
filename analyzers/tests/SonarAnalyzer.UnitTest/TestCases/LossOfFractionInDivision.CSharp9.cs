decimal dec = 3 / 2;    // Noncompliant
dec = 3L / 2;           // Noncompliant
Method(3 / 2);          // Noncompliant
dec = (decimal)3 / 2;
Method(3.0F / 2);

nint a = 3;
nint b = 2;
nuint c = 5;
decimal both = a / b;           // Noncompliant
decimal left = (nint)3 / 2;     // Noncompliant
decimal right = 3 / (nint)2;    // Noncompliant
left = c / 2;                   // Noncompliant
left = (nuint)3 / 2;            // Noncompliant
Method((nint)3 / 2);            // Noncompliant

right = (decimal)3 / b;         // Compliant
Method(3.0F / b);               // Compliant

left = (UnknownType)3 / 2;      // Error [CS0246]

void Method(float f) { }

static double Calc()
{
    return (nint)3 / 2;     // Noncompliant
}
