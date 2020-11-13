decimal dec = 3 / 2;    // Noncompliant
dec = 3L / 2;           // Noncompliant
Method(3 / 2);          // Noncompliant
dec = (decimal)3 / 2;
Method(3.0F / 2);

nint a = 3;
nint b = 2;
decimal both = a / b;           // FN
decimal left = (nint)3 / 2;     // FN
decimal right = 3 / (nint)2;    // FN
left = (nuint)3 / 2;            // FN
Method((nint)3 / 2);            // FN

right = (decimal)3 / b;         // Compliant
Method(3.0F / b);               // Compliant

void Method(float f) { }

static double Calc()
{
    return (nint)3 / 2;     // FN
}

