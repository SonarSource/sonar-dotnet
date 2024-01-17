using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class HardcodedIpAddress
    {
        public void RawStringLiterals(string unknownPart, string knownPart)
        {
            string ip1 = """192.168.0.1"""; // Noncompliant {{Make sure using this hardcoded IP address '192.168.0.1' is safe here.}}
            var ip2 = "192.168.0.1"u8; // Noncompliant
            var ip3 = "\x31\x39\x32\x2E\x31\x36\x38\x2E\x30\x2E\x31"u8; // Noncompliant - this is 192.168.0.1 in utf-8
            var ip4 = """192.168.0.1"""u8; // Noncompliant
            var ip5 = """
                192.168.0.1
                """u8; // Noncompliant@-2
        }

        public void NewlinesInStringInterpolation()
        {
            const string s1 = "1";
            const string s2 = "";
            string nonCompliant = $"192.168.0.{s1 + // Noncompliant
                s2}";
            string nonCompliantRawString = $$"""192.168.0.{{s1 + // Noncompliant
                s2}}""";
        }
    }
}
