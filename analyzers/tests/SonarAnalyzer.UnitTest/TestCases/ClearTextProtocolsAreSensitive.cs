using System;

namespace Tests.Diagnostics
{
    class ClearTextProtocols
    {
        private const string a = "http://example.com"; // Noncompliant
        private const string c = @"telnet://anonymous@example.com"; // Noncompliant
        private readonly string b = $"ftp://anonymous@example.com"; // Noncompliant
    }
}
