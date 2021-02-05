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
        Property2.HttpOnly = false; // Noncompliant
    }

    void InitializerSetsNotAllowedValue()
    {
        CookieOptions c1 = new () { HttpOnly = false }; // FN
        CookieOptions c2 = new () { Secure = true }; // FN, as HttpOnly is not set
    }
}
