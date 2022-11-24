using System;
using System.Numerics;

nint nInt = 2;
nuint nUInt = 3;
IntPtr intPtr = 4;
UIntPtr uIntPtr = 5;

bool result;

result = nInt % 2 == 42; // Noncompliant {{The result of this modulus operation may not be positive.}}
result = nInt % 2 != 42; // Noncompliant {{The result of this modulus operation may not be positive.}}
result = nInt % 2 == -42; // Noncompliant {{The result of this modulus operation may not be negative.}}
result = nInt % 2 != -42; // Noncompliant {{The result of this modulus operation may not be negative.}}
result = nInt % 2 != 0; // Compliant

result = intPtr % 2 == 42; // Noncompliant {{The result of this modulus operation may not be positive.}}
result = intPtr % 2 != 42; // Noncompliant {{The result of this modulus operation may not be positive.}}
result = intPtr % 2 == -42; // Noncompliant {{The result of this modulus operation may not be negative.}}
result = intPtr % 2 != -42; // Noncompliant {{The result of this modulus operation may not be negative.}}
result = intPtr % 2 != 0; // Compliant

result = nUInt % 2 == 42; // Compliant
result = nUInt % 2 != 42; // Compliant

result = uIntPtr % 2 == 42; // Compliant
result = uIntPtr % 2 != 42; // Compliant

bool ModulusOperator<TSelf>(TSelf parameter) where TSelf : IModulusOperators<TSelf, int, int>
    => parameter % 2 == 42; // Noncompliant {{The result of this modulus operation may not be positive.}}

