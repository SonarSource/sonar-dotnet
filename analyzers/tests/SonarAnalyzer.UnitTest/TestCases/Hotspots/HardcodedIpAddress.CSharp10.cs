using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
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
