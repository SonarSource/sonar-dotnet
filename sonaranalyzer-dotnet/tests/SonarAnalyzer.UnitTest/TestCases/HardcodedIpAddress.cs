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
            ip = "127.0.0.1"; // Compliant, this is an exception in the rule (see: https://github.com/SonarSource/sonar-csharp/issues/1540)
            ip = "    127.0.0.0    "; // Compliant
            ip = @"    ""127.0.0.0""    "; // Compliant

            ip = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff"; // Noncompliant
            ip = "::/0"; // Compliant, not recognized as IPv6 address
            ip = "::"; // Compliant, this is an exception in the rule

            ip = "2"; // Compliant, should not be recognized as 0.0.0.2

            new Version("127.0.0.0"); //Compliant
            new AnyAssemblyClass("127.0.0.0"); //Compliant

            WriteAssemblyInfo("ProjectWithDependenciesWithContent",
                               "1.2.0.0", //Compliant
                                 "Thomas",
                                 "Project with content",
                                "Title of Package");
        }
    }
}
