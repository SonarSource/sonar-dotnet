using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class HardcodedIpAddress
    {
        public HardcodedIpAddress()
        {
            const string part1 = "192";
            const string part2 = "168";
            const string part3 = "0";
            const string part4 = "1";

            const string ip = $"{part1}.{part2}.{part3}.{part4}"; // FN
            const string nonIp = $"{part1}:{part2}";
        }
    }
}
