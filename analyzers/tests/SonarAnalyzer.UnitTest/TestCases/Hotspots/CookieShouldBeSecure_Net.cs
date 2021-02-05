using System;
using Microsoft.AspNetCore.Http;

CookieOptions topLevelStatement1 = new(); // FN
CookieOptions topLevelStatement2 = new CookieOptions(); // Noncompliant

class Program
{
    CookieOptions field1 = new(); // FN
    CookieOptions field2;

    CookieOptions Property0 { get; init; } = new CookieOptions(); // Noncompliant
    CookieOptions Property1 { get; init; } = new (); // FN
    CookieOptions Property2 { get; init; }

    Program()
    {
        Property2.Secure = false; // Noncompliant
    }

    void InitializerSetsNotAllowedValue()
    {
        CookieOptions c0 = new () { Secure = false }; // FN
        CookieOptions c1 = new () { HttpOnly = true }; // FN, as Secure is not set
    }
}
