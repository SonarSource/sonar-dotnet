using System;
using Microsoft.AspNetCore.Http;

CookieOptions topLevelStatement1 = new(); // Noncompliant
CookieOptions topLevelStatement2 = new CookieOptions(); // Noncompliant

class Program
{
    CookieOptions field1 = new(); // Noncompliant
    CookieOptions field2;

    CookieOptions Property0 { get; init; } = new CookieOptions(); // Noncompliant
    CookieOptions Property1 { get; init; } = new (); // Noncompliant
    CookieOptions Property2 { get; init; }

    Program()
    {
        Property2.HttpOnly = false; // Noncompliant
    }

    void InitializerSetsNotAllowedValue()
    {
        CookieOptions c1 = new () { HttpOnly = false }; // Noncompliant
        CookieOptions c2 = new () { Secure = true };    // Noncompliant
    }
}
