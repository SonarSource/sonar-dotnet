using System;
using Microsoft.AspNetCore.Http;

namespace Tests.Diagnostics
{
    class Program
    {
        void Responses(HttpResponse response)
        {
            var value = "";

            // Response headers
            response.Headers.Add("Set-Cookie", ""); // Noncompliant
            response.Headers["Set-Cookie"] = ""; // Noncompliant
            value = response.Headers["Set-Cookie"]; // Compliant

            // Not the Set-Cookie header
            response.Headers.Add("something", "");
            response.Headers["something"] = value;
            value = response.Headers["something"];

            // Response headers as variable
            var responseHeaders = response.Headers;
            responseHeaders.Add("Set-Cookie", ""); // Noncompliant
            responseHeaders["Set-Cookie"] = ""; // Noncompliant
            value = responseHeaders["Set-Cookie"]; // Compliant

            responseHeaders.Remove("Set-Cookie"); // Compliant
            responseHeaders.Remove(""); // Compliant

            // Response cookies as property
            response.Cookies.Append("", ""); // Noncompliant
            response.Cookies.Append("", "", new CookieOptions { }); // Noncompliant

            // Response cookies as variable
            var responseCookies = response.Cookies;
            responseCookies.Append("", ""); // Noncompliant
            responseCookies.Append("", "", new CookieOptions { }); // Noncompliant

            responseCookies.Delete(""); // Compliant
        }

        void Requests(HttpRequest request)
        {
            var value = "";

            // Request headers
            request.Headers.Add("Set-Cookie", ""); // Noncompliant
            request.Headers["Set-Cookie"] = value; // Noncompliant
            value = request.Headers["Set-Cookie"]; // Compliant

            // Not the Set-Cookie header
            request.Headers.Add("something", "");
            request.Headers["something"] = value;
            value = request.Headers["something"];

            // Request headers as variable
            var requestHeaders = request.Headers;
            requestHeaders.Add("Set-Cookie", ""); // Noncompliant
            requestHeaders["Set-Cookie"] = value; // Noncompliant
            value = requestHeaders["Set-Cookie"]; // Compliant

            requestHeaders.Remove("Set-Cookie"); // Compliant
            requestHeaders.Remove(""); // Compliant

            // Request cookies as property
            value = request.Cookies[""]; // Compliant
            request.Cookies.TryGetValue("", out value); // Compliant

            // Request cookies as variable
            var requestCookies = request.Cookies;
            value = requestCookies[""]; // Compliant
            requestCookies.TryGetValue("", out value); // Compliant
        }
    }
}
