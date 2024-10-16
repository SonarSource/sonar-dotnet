using System;
using System.Collections.Generic;

namespace CSharp10
{
    public class HardcodedIpAddress
    {
        public HardcodedIpAddress(string unknownPart, string knownPart)
        {
            const string part1 = "192";
            const string part2 = "168";
            const string part3 = "0";
            const string part4 = "1";
            knownPart = "255";

            const string ip1 = $"{part1}.{part2}.{part3}.{part4}"; // Noncompliant
//                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            const string nonIp = $"{part1}:{part2}";

            string ip2 = $"{part1}.{part2}.{part3}.{knownPart}"; // Noncompliant
            string ip3 = $"{part1}.{part2}.{part3}.{unknownPart}";

            const string nestedConstInterpolation = $"{$"{part1}.{part2}"}.{part3}.{part4}"; // Noncompliant
            string nestedInterpolation = $"{$"{part1}.{knownPart}"}.{part3}.{part4}"; // Noncompliant
        }
    }
}

namespace CSharp11
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

namespace CSharp12
{
    class PrimaryConstructor(string ctorParam = "192.168.0.1", string ctorParam2 = $"192.168.{"0"}.1") // Noncompliant
                                                                                                       // Noncompliant@-1
    {
        void Method(string methodParam = "192.168.0.1", string methodParam2 = $"192.168.{"0"}.1") // Noncompliant
                                                                                                  // Noncompliant@-1
        {
            var lambda = (string lambdaParam = "192.168.0.1", string lambdaParam2 = $"192.168.{"0"}.1") => lambdaParam; // Noncompliant
                                                                                                                        // Noncompliant@-1
        }
    }
}

namespace CSharp13
{
    partial class Partial
    {
        partial string MyProperty { get; }
        partial string MyProperty2 { get; }

        public void MyMethod(string unknownPart, string knownPart)
        {
            _ = "\e192.168.0.1";         // Compliant
            _ = "\u001b192.168.0.1";     // Compliant
            _ = "192\e.168.0.1";         // Compliant
            _ = "192.\e168.0.1";         // Compliant
            _ = "192.168.\e0.1";         // Compliant
            _ = "192.168.0.1\e";         // Compliant
            _ = $"{MyProperty2}168.0.1"; // Compliant - MyProperty2 is not a constant
        }
    }

    partial class Partial
    {
        partial string MyProperty => "192.168.0.1"; // Noncompliant
        partial string MyProperty2 => "192.";
    }
}
