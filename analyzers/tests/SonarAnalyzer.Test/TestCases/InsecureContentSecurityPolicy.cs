using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace SonarAnalyzer.Test.TestCases
{
    public class InsecureContentSecurityPolicy
    {
        public async Task InvokeAsync(HttpContext context, IHeaderDictionary dictionary, A mock)
        {
            const string unsafeConstantValue = "script-src 'self' 'unsafe-inline';";
            const string safeConstantValue = "script-src 'self';";
            const string partiallyUnsafeConstantValue = "unsafe";
            const string contentSecurityPolicy = "Content-Security-Policy";

            context.Response.Headers.ContentSecurityPolicy = "*";                 // Noncompliant {{Content Security Policies should be restrictive to mitigate the risk of content injection attacks.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            context.Response.Headers.ContentSecurityPolicy = "'unsafe-inline'";   // Noncompliant
            context.Response.Headers.ContentSecurityPolicy = "'unsafe-hashes'";   // Noncompliant
            context.Response.Headers.ContentSecurityPolicy = "'unsafe-eval'";     // Noncompliant
            context.Response.Headers.ContentSecurityPolicy = "script-src 'self' 'unsafe-inline' 'unsafe-eval' 'unsafe-hashes';"; // Noncompliant
            context.Response.Headers.ContentSecurityPolicy = "'something-else'";  // Compliant
            context.Response.Headers.AccessControlAllowHeaders = "'unsafe-inline'";   // Compliant
            context.Response.Headers.ContentSecurityPolicy = unsafeConstantValue; // Noncompliant
            context.Response.Headers.ContentSecurityPolicy = safeConstantValue;   // Compliant
            context.Response.Headers.ContentSecurityPolicy = $"{safeConstantValue} 'unsafe-inline'"; // Noncompliant
            context.Response.Headers.ContentSecurityPolicy = $"{safeConstantValue} 'unsafe-inline'"; // Noncompliant
            context.Response.Headers.ContentSecurityPolicy = $"'{partiallyUnsafeConstantValue}-inline'"; // Noncompliant

            context.Response.Headers.ContentSecurityPolicy = "script-src 'self' 'unsafe-inline';"; // Noncompliant
            context.Response.Headers.ContentSecurityPolicy = "script-src 'self';";                 // Compliant
            context.Response.Headers.AcceptCharset = "script-src 'self' 'unsafe-inline';";         // Compliant
            _ = context.Response.Headers.ContentSecurityPolicy; // Compliant

            context.Response.Headers["Content-Security-Policy"] = "script-src 'self' 'unsafe-inline';"; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            context.Response.Headers["Content-Security-Policy"] = "script-src 'self';"; // Compliant
            context.Response.Headers["Content-Security-Policy"] = unsafeConstantValue; // Noncompliant
            context.Response.Headers["AcceptCharset"] = "script-src 'self' 'unsafe-inline';"; // Compliant
            context.Response.Headers[contentSecurityPolicy] = "script-src 'self' 'unsafe-inline';"; // Noncompliant
            context.Response.Headers[contentSecurityPolicy] = unsafeConstantValue; // Noncompliant
            _ = context.Response.Headers[contentSecurityPolicy]; // Compliant
            context.Response.Headers[contentSecurityPolicy].ToString(); // Compliant

            context.Response.Headers.Add("Content-Security-Policy", "default-src 'self' '*.example.com';"); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            context.Response.Headers.Add("Content-Security-Policy", "default-src 'self' 'example.com'"); // Compliant
            context.Response.Headers.Add("Content-Security-Policy", unsafeConstantValue); // Noncompliant
            context.Response.Headers.Add(contentSecurityPolicy, unsafeConstantValue); // Noncompliant
            context.Response.Headers.Add(contentSecurityPolicy, $"'{partiallyUnsafeConstantValue}-inline'"); // Noncompliant
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self' '*.example.com';"); // Noncompliant
            context.Response.Headers.Append("Content-Security-Policy", "script-src 'self' 'example.com';"); // Compliant
            context.Response.Headers.Append("Content-Security-Policy", unsafeConstantValue); // Noncompliant
            context.Response.Headers.Append(contentSecurityPolicy, unsafeConstantValue); // Noncompliant
            context.Response.Headers.Add("AcceptCharset", "default-src 'self' '*.example.com';"); // Compliant
            context.Response.Headers.Append("AcceptCharset", "default-src 'self' '*.example.com';"); // Compliant

            dictionary.Add("Content-Security-Policy", "default-src 'self' '*.example.com';"); // Noncompliant
            dictionary.Add("Content-Security-Policy", "default-src 'self';"); // Compliant
            dictionary.Append("Content-Security-Policy", "default-src 'self' '*.example.com';"); // Noncompliant
            dictionary.Append("Content-Security-Policy", "default-src 'self';"); // Compliant
            dictionary["Content-Security-Policy"] = "default-src 'self' '*.example.com';"; // FN
            dictionary["Content-Security-Policy"] = "default-src 'self';"; // Compliant

            mock.Response.Headers.ContentSecurityPolicy = "script-src 'self' 'unsafe-inline';";            // Compliant
            mock.Response.Headers.Add("Content-Security-Policy", "script-src 'self' 'unsafe-inline';");    // Compliant
            mock.Response.Headers.Append("Content-Security-Policy", "script-src 'self' 'unsafe-inline';"); // Compliant
            mock.Response.Headers["Content-Security-Policy"] = "script-src 'self' 'unsafe-inline';";       // Compliant

            context.Response.Headers.ContentSecurityPolicy = "script-src 'self' 'wasm-unsafe-eval';"; // Compliant: 'wasm-unsafe-eval' should not raise issue as it is required for client-side Blazor apps.

            context.Response.Headers["Content-Security-Policy"] = new StringValues("script-src 'self' 'unsafe-inline';");                          // FN
            context.Response.Headers.Add(new KeyValuePair<string, StringValues>("Content-Security-Policy", "script-src 'self' 'unsafe-inline';")); // FN
            context.Response.Headers.Add("Content-Security-Policy", new StringValues("default-src 'self' '*.example.com';"));                      // FN
            context.Response.Headers.Append("Content-Security-Policy", new StringValues("script-src 'self' 'unsafe-inline';"));                    // FN
        }

        public class A
        {
            public B Response { get; set; }
            public class B
            {
                public C Headers { get; set; }
                public class C
                {
                    public string ContentSecurityPolicy { get; set; }
                    public void Add(string key, string value) { }
                    public void Append(string key, string value) { }
                    public string this[string key]
                    {
                        get => string.Empty;
                        set => ContentSecurityPolicy = value;
                    }
                }
            }
        }
    }
}
