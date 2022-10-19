using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class HardcodedIpAddress
    {
        public HardcodedIpAddress(string unknownPart, string knownPart)
        {
            string ip1 = """192.168.0.1"""; // Noncompliant {{Make sure using this hardcoded IP address '192.168.0.1' is safe here.}}
            var ip2 = "192.168.0.1"u8; // FN
            var ip3 = """192.168.0.1"""u8; // FN
            var ip4 = """
                192.168.0.1
                """u8; // FN
        }
    }
}
