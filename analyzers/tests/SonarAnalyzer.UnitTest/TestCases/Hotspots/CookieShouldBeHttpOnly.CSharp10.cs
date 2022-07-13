using System;
using Microsoft.AspNetCore.Http;
using Nancy.Cookies;

(CookieOptions topLevelStatement1, int a) = (new CookieOptions(), 42); // Noncompliant
(NancyCookie topLevelStatement2, var b) = (new ("name", "secure"), "42"); // Noncompliant

var c = new CookieOptions() { HttpOnly = true };
(c.HttpOnly, var x1) = (false, 0); // Noncompliant
(c.HttpOnly, var x2) = (true, 0);
(c.HttpOnly, var x3) = ((false), 0); // Noncompliant
