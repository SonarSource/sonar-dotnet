nint i = 0;
i = i++; // Noncompliant; i is still zero
i = ++i; // Compliant

nuint u = 0;
u = u++; // Noncompliant; u is still zero
u = ++u; // Compliant

int Compute(nint v) =>
    (int) v++; // Noncompliant

return (int)i++; // Noncompliant
