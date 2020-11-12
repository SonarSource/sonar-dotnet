decimal dec = 3 / 2;    // Noncompliant
dec = 3L / 2;           // Noncompliant
Method(3 / 2);          // Noncompliant
dec = (decimal)3 / 2;
Method(3.0F / 2);

decimal ndec = (nint)3 / 2; // FN
ndec = (nuint)3 / 2;        // FN
Method((nint)3 / 2);        // FN

void Method(float f) { }

static double Calc()
{
    return (nint)3 / 2;     // FN
}

