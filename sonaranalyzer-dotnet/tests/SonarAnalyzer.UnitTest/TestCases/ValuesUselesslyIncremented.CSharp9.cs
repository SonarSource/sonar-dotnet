nint i = 0;
i = i++; // Noncompliant; i is still zero
i = ++i; // Compliant

nuint u = 0;
u = u++; // Noncompliant; u is still zero
u = ++u; // Compliant

return (int)i++; // Compliant FN
