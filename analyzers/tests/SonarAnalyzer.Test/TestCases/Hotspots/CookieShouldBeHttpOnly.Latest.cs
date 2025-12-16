using System;
using Microsoft.AspNetCore.Http;
using Nancy.Cookies;

class Tests
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
        new CookieOptions(); // Noncompliant {{Make sure creating this cookie without the "HttpOnly" flag is safe.}}
    }

    void InitializerSetsAllowedValue()
    {
        new CookieOptions() { HttpOnly = true };
    }

    void InitializerSetsNotAllowedValue()
    {
        new CookieOptions() { HttpOnly = false }; // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        new CookieOptions() { }; // Noncompliant
        new CookieOptions() { Secure = true }; // Noncompliant
    }
 
    void PropertySetsNotAllowedValue()
    {
        var c = new CookieOptions() { HttpOnly = true };
        c.HttpOnly = false; // Noncompliant
//      ^^^^^^^^^^^^^^^^^^

        field1.HttpOnly = false; // Noncompliant
        this.field1.HttpOnly = false; // Noncompliant

        Property1.HttpOnly = false; // Noncompliant
        this.Property1.HttpOnly = false; // Noncompliant
    }

    void PropertySetsAllowedValue(bool foo)
    {
        var c1 = new CookieOptions(); // Compliant, HttpOnly is set below
        c1.HttpOnly = true;

        field1 = new CookieOptions(); // Compliant, HttpOnly is set below
        field1.HttpOnly = true;

        this.field2 = new CookieOptions(); // Compliant, HttpOnly is set below
        this.field2.HttpOnly = true;

        Property1 = new CookieOptions(); // Compliant, HttpOnly is set below
        Property1.HttpOnly = true;

        this.Property2 = new CookieOptions(); // Compliant, HttpOnly is set below
        this.Property2.HttpOnly = true;

        var c2 = new CookieOptions(); // Noncompliant, HttpOnly is set conditionally
        if (foo)
        {
            c2.HttpOnly = true;
        }

        var c3 = new CookieOptions(); // Compliant, HttpOnly is set after the if
        if (foo)
        {
            // do something
        }
        c3.HttpOnly = true;

        CookieOptions c4 = null;
        if (foo)
        {
            c4 = new CookieOptions(); // Noncompliant, HttpOnly is not set in the same scope
        }
        c4.HttpOnly = true;
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
        Property2.HttpOnly = false; // Noncompliant
    }

    void InitializerSetsNotAllowedValue(DateTime? expires, string domain, string path)
    {
        CookieOptions c1 = new() { HttpOnly = false }; // Noncompliant
        CookieOptions c2 = new() { Secure = true };    // Noncompliant
        NancyCookie cookie2 = new("name", "secure") { Expires = expires, Domain = domain, Path = path };  // Noncompliant
    }
}

public record struct RecordStruct
{
    public void SetValueAfterObjectInitialization()
    {
        var propertyTrue = new CookieOptions() { HttpOnly = false }; // Compliant, property is set below
        (propertyTrue.HttpOnly, var x1) = (true, 0);

        var propertyFalse = new CookieOptions() { HttpOnly = false }; // Noncompliant
        (propertyFalse.HttpOnly, var x2) = (false, 0); // Noncompliant
    }
}

partial class Partial
{
    partial CookieOptions Property2 { get; }
}

partial class Partial
{
    partial CookieOptions Property2 => new CookieOptions(); // Noncompliant
}

public class NullConditionalAssignment
{
    public void TestMethod(CookieOptions cookie)
    {
        cookie.HttpOnly = false;    // Noncompliant
        cookie.HttpOnly = true;     // Compliant

        cookie?.HttpOnly = false;   // FN https://sonarsource.atlassian.net/browse/NET-2857
        cookie?.HttpOnly = true;    // Compliant
    }
}

public static class Extension
{
    extension(CookieOptions c)
    {
        public void SetHttpOnlyTrue() { c.HttpOnly = true; }    // Compliant
        public void SetHttpOnlyFalse() { c.HttpOnly = false; }  // Noncompliant
    }
}
