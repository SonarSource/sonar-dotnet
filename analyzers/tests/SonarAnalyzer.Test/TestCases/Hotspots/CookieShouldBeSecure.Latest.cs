using System;
using Microsoft.AspNetCore.Http;
using Nancy.Cookies;

CookieOptions topLevelStatement1 = new();                // Noncompliant
CookieOptions topLevelStatement2 = new CookieOptions();  // Noncompliant
NancyCookie topLevelStatement3 = new("name", "secure"); // Noncompliant

(CookieOptions topLevelStatement4, int a) = (new CookieOptions(), 42);  // Noncompliant
(NancyCookie topLevelStatement5, var b) = (new("name", "secure"), "42"); // Noncompliant

var c = new CookieOptions() { Secure = true };
(c.Secure, var x1) = (false, 0); // Noncompliant
(c.Secure, var x2) = (true, 0);
(c.Secure, var x3) = ((false), 0); // Noncompliant

class MyClass
{
    CookieOptions field1 = new CookieOptions(); // Noncompliant
    CookieOptions field2;

    CookieOptions Property1 { get; set; } = new CookieOptions(); // Noncompliant
    CookieOptions Property2 { get; set; }

    void CtorSetsAllowedValue()
    {
        // none
    }

    void CtorSetsNotAllowedValue()
    {
        new CookieOptions(); // Noncompliant {{Make sure creating this cookie without setting the 'Secure' property is safe here.}}
    }

    void InitializerSetsAllowedValue()
    {
        new CookieOptions() { Secure = true };
    }

    void InitializerSetsNotAllowedValue()
    {
        new CookieOptions() { Secure = false }; // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        new CookieOptions() { }; // Noncompliant
        new CookieOptions() { HttpOnly = true }; // Noncompliant
    }

    void PropertySetsNotAllowedValue()
    {
        var c = new CookieOptions() { Secure = true };
        c.Secure = false; // Noncompliant
//      ^^^^^^^^^^^^^^^^
        field1.Secure = false; // Noncompliant
        this.field1.Secure = false; // Noncompliant
        Property1.Secure = false; // Noncompliant
        this.Property1.Secure = false; // Noncompliant
    }

    void PropertySetsAllowedValue(bool foo)
    {
        var c1 = new CookieOptions(); // Compliant, Secure is set below
        c1.Secure = true;
        field1 = new CookieOptions(); // Compliant, Secure is set below
        field1.Secure = true;
        this.field2 = new CookieOptions(); // Compliant, Secure is set below
        this.field2.Secure = true;
        Property1 = new CookieOptions(); // Compliant, Secure is set below
        Property1.Secure = true;
        this.Property2 = new CookieOptions(); // Compliant, Secure is set below
        this.Property2.Secure = true;
        var c2 = new CookieOptions(); // Noncompliant, Secure is set conditionally
        if (foo)
        {
            c2.Secure = true;
        }
        var c3 = new CookieOptions(); // Compliant, Secure is set after the if
        if (foo)
        {
            // do something
        }
        c3.Secure = true;
        CookieOptions c4 = null;
        if (foo)
        {
            c4 = new CookieOptions(); // Noncompliant, Secure is not set in the same scope
        }
        c4.Secure = true;
    }
}

class CSharp9
{
    CookieOptions field1 = new(); // Noncompliant
    CookieOptions field2;

    CookieOptions Property0 { get; init; } = new CookieOptions(); // Noncompliant
    CookieOptions Property1 { get; init; } = new(); // Noncompliant
    CookieOptions Property2 { get; init; }

    CSharp9()
    {
        Property2.Secure = false; // Noncompliant
    }

    void InitializerSetsNotAllowedValue(DateTime? expires, string domain, string path)
    {
        CookieOptions c0 = new() { Secure = false };  // Noncompliant
        CookieOptions c1 = new() { HttpOnly = true }; // Noncompliant
        NancyCookie cookie2 = new("name", "secure") { Expires = expires, Domain = domain, Path = path };  // Noncompliant
    }
}

public record struct RecordStruct
{
    public void SetValueAfterObjectInitialization()
    {
        var propertyTrue = new CookieOptions() { Secure = false }; // Compliant, property is set below
        (propertyTrue.Secure, var x1) = (true, 0);

        var propertyFalse = new CookieOptions() { Secure = false }; // Noncompliant
        (propertyFalse.Secure, var x2) = (false, 0); // Noncompliant
    }
}

partial class Partial
{
    partial CookieOptions Property0 { get; }
}

partial class Partial
{
    partial CookieOptions Property0 => new CookieOptions(); // Noncompliant
}
