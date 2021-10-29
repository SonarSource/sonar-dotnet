using System;
using Microsoft.AspNetCore.Http;
using Nancy.Cookies;

(CookieOptions topLevelStatement1, int a) = (new CookieOptions(), 42);  // Noncompliant
(NancyCookie topLevelStatement2, var b) = (new ("name", "secure"), "42"); // Noncompliant
