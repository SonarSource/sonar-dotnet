nint i = 0;
i = i++; // Noncompliant; i is still zero
i = ++i; // Compliant

nuint u = 0, a = 0;
u = u++; // Noncompliant; u is still zero
u = ++u; // Compliant
u += u++;
a = u++;

int x = (int) u++;

int Compute(nint v) =>
    (int) v++; // Noncompliant
//        ^^^

int ComputeArrowBody(int v) =>
    v--; // Noncompliant
//  ^^^

return (int)i++; // Noncompliant
