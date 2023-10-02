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
            ip = "300.0.0.0";                               // Compliant, not a valid IP
            ip = "127.0.0.1";                               // Compliant, this is an exception in the rule (see: https://github.com/SonarSource/sonar-dotnet/issues/1540)
            ip = "    127.0.0.0    ";
            ip = @"    ""127.0.0.0""    ";
            ip = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff";  // Compliant. Is a documentation IP.
            ip = "2002:db8:1234:ffff:ffff:ffff:ffff:ffff";  // Noncompliant
            ip = "2001:abcd:1234:ffff:ffff:ffff:ffff:ffff"; // Noncompliant
            ip = "2002:db8::ff00:42:8329";                  // Noncompliant
            ip = "::/0";                                    // Compliant, not recognized as IPv6 address
            ip = "::";                                      // Compliant, this is an exception in the rule
            ip = "2";                                       // Compliant, should not be recognized as 0.0.0.2
            ip = "::2";                                     // Noncompliant
            ip = "0:0:0:0:0:0:0:2";                         // Noncompliant
            ip = "0:0::0:2";                                // Noncompliant
            ip = "1623:0000:0000:0000:0000:0000:0000:0001"; // Noncompliant

            new Version("127.0.0.0");
            new Version("192.168.0.1");
            new AnyAssemblyClass("127.0.0.0");

            WriteAssemblyInfo("ProjectWithDependenciesWithContent",
                               "1.2.0.0",
                                 "Thomas",
                                 "Project with content",
                                "Title of Package");

            string broadcastAddress = "255.255.255.255";
            string loopbackAddress1 = "127.0.0.1";
            string loopbackAddress2 = "127.2.3.4";
            string loopbackAddress3 = "::ffff:127.0.0.1";   //Compliant https://www.rfc-editor.org/rfc/rfc4291.html#section-2.5.5.2
            string loopbackAddress4 = "::ffff:127.2.3.4";
            string loopbackAddress5 = "::1";                //Compliant https://www.rfc-editor.org/rfc/rfc4291.html#section-2.5.3
            string loopbackAddress6 = "64:ff9b::127.2.3.4"; // Noncompliant Translated IP4 not supported https://www.rfc-editor.org/rfc/rfc6052.html
            string loopbackAddress7 = "::ffff:0:127.2.3.4"; // Noncompliant Translated IP4 not supported https://www.rfc-editor.org/rfc/rfc2765.html
            string nonRoutableAddress = "0.0.0.0";
            string documentationRange1 = "192.0.2.111";
            string documentationRange2 = "198.51.100.111";
            string documentationRange3 = "203.0.113.111";
            string notAnIp1 = "0.0.0.1234";
            string country_oid = "2.5.6.2";
            string subschema_oid = "2.5.20.1";
            string not_considered_as_an_oid = "2.59.6.2";   // Noncompliant
//                                            ^^^^^^^^^^

            // IPV6 loopback
            string loopbackAddressV6_1 = "::1";
            string loopbackAddressV6_2 = "0:0:0:0:0:0:0:1";
            string loopbackAddressV6_3 = "0:0::0:1";
            string loopbackAddressV6_4 = "0000:0000:0000:0000:0000:0000:0000:0001";

            // IPV6 non routable
            string nonRoutableAddressV6_1 = "::";
            string nonRoutableAddressV6_2 = "0:0:0:0:0:0:0:0";
            string nonRoutableAddressV6_3 = "0::0";
            string nonRoutableAddressV6_4 = "0000:0000:0000:0000:0000:0000:0000:0000";

            // IPV6 documentation range
            string documentationRangeV6_1 = "2001:0db8:0000:0000:0000:ff00:0042:8329";
            string documentationRangeV6_2 = "2001:db8:0:0:0:ff00:42:8329";
            string documentationRangeV6_3 = "2001:db8::ff00:42:8329";

            // IPV6 invalid form
            string invalidIPv6 = "1::2::3";
            invalidIPv6 = "20015555:db8:1234:ffff:ffff:ffff:ffff:ffff";
            invalidIPv6 = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff:1623:2316";
            invalidIPv6 = ":::4";

            string invalidIp = $"192.168.{"0"}.1"; // Noncompliant
        }
    }
}
