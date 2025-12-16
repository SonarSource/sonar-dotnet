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
