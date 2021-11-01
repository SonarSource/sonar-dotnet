using System;
using Microsoft.AspNetCore.Http;

namespace Tests.Diagnostics
{
    class Program
    {
        void Responses(HttpResponse response)
        {
            const string set = "Set";
            const string cookie = "Cookie";
            const string compliant = $"{cookie}-{set}";
            const string noncompliant = $"{set}-{cookie}";

            response.Headers.Add(compliant, "");
            response.Headers.Add(noncompliant, ""); // Noncompliant
        }
    }
}
