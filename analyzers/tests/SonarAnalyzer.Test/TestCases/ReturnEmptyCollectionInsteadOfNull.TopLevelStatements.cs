using System;

string[] LocalFunction() => null;                   // Noncompliant {{Return an empty collection instead of null.}}
string[] LocalFunctionNew() => new string[0];       // Compliant

static string[] StaticLocalFunction() => (null);    // Noncompliant
