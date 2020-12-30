using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class AnyAssemblyClass
    {
        public AnyAssemblyClass(string s)
        {
        }
    }

    public class SomeAttribute : Attribute
    {
        public SomeAttribute(string s)
        {

        }
    }

    public class HardcodedIpAddress
    {
        private static void WriteAssemblyInfo(string assemblyName, string version, string author, string description, string title)
        {
        }

        [SomeAttribute("127.0.0.1")] // this is mainly for assembly versions
        public HardcodedIpAddress()
        {
            string ip = "192.168.0.1"; // Noncompliant {{Make sure using this hardcoded IP address '192.168.0.1' is safe here.}}
//                      ^^^^^^^^^^^^^
            ip = "300.0.0.0"; // Compliant, not a valid IP
            ip = "127.0.0.1"; // Compliant, this is an exception in the rule (see: https://github.com/SonarSource/sonar-dotnet/issues/1540)
            ip = "    127.0.0.0    "; // Compliant
            ip = @"    ""127.0.0.0""    "; // Compliant
            ip = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff"; // Noncompliant
            ip = "::/0"; // Compliant, not recognized as IPv6 address
            ip = "::"; // Compliant, this is an exception in the rule
            ip = "2"; // Compliant, should not be recognized as 0.0.0.2
            ip = "::2"; // Noncompliant
            ip = "0:0:0:0:0:0:0:2"; // Noncompliant
            ip = "0:0::0:2"; // Noncompliant
            ip = "1623:0000:0000:0000:0000:0000:0000:0001"; // Noncompliant

            new Version("127.0.0.0"); // Compliant
            new AnyAssemblyClass("127.0.0.0"); // Compliant

            WriteAssemblyInfo("ProjectWithDependenciesWithContent",
                               "1.2.0.0", // Compliant
                                 "Thomas",
                                 "Project with content",
                                "Title of Package");

            string broadcastAddress = "255.255.255.255"; //Compliant
            string loopbackAddress1 = "127.0.0.1"; //Compliant
            string loopbackAddress2 = "127.2.3.4"; //Compliant
            string nonRoutableAddress = "0.0.0.0"; //Compliant
            string notAnIp1 = "0.0.0.1234"; //Compliant
            string country_oid = "2.5.6.2"; //Compliant 
            string subschema_oid = "2.5.20.1";
            string not_considered_as_an_oid = "2.59.6.2"; // Noncompliant
//                                            ^^^^^^^^^^

            // IPV6 loopback
            string loopbackAddressV6_1 = "::1"; //Compliant
            string loopbackAddressV6_2 = "0:0:0:0:0:0:0:1"; //Compliant
            string loopbackAddressV6_3 = "0:0::0:1"; //Compliant
            string loopbackAddressV6_4 = "0000:0000:0000:0000:0000:0000:0000:0001"; //Compliant

            // IPV6 non routable
            string nonRoutableAddressV6_1 = "::"; //Compliant
            string nonRoutableAddressV6_2 = "0:0:0:0:0:0:0:0"; //Compliant
            string nonRoutableAddressV6_3 = "0::0"; //Compliant
            string nonRoutableAddressV6_4 = "0000:0000:0000:0000:0000:0000:0000:0000"; //Compliant

            // IPV6 invalid form
            string invalidIPv6 = "1::2::3"; //Compliant
            invalidIPv6 = "20015555:db8:1234:ffff:ffff:ffff:ffff:ffff"; // Compliant
            invalidIPv6 = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff:1623:2316"; // Compliant
            invalidIPv6 = ":::4"; // Compliant
        }
    }
}
