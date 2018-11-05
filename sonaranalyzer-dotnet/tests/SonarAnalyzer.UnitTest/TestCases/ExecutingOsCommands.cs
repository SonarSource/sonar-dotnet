using System;
using System.Security;
using System.Diagnostics;
using P = System.Diagnostics.Process;
using PSI = System.Diagnostics.ProcessStartInfo;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Foo(string fileName, ProcessStartInfo startInfo, Process process)
        {
            process.Start(); // Compliant, FileName is not passed here
            Process.Start(fileName); // Noncompliant
            Process.Start(fileName, null); // Noncompliant
            Process.Start(fileName, null, null, null); // Noncompliant
            Process.Start(fileName, null, null, null, null); // Noncompliant

            Process.Start(startInfo); // Compliant, the ProcessStartInfo's FileName has already been highlighted elsewhere

            startInfo.FileName = fileName; // Noncompliant
            process.StartInfo.FileName = fileName; // Noncompliant, setting ProcessStartInfo.FileName here.

            fileName = startInfo.FileName; // Compliant, the FileName is not set here
            fileName = process.StartInfo.FileName; // Compliant, the FileName is not set here

            new ProcessStartInfo(); // Compliant, the FileName is set elsewhere
            new ProcessStartInfo(fileName); // Noncompliant
            new ProcessStartInfo(fileName, null); // Noncompliant

            // Different ways to specify the types
            System.Diagnostics.Process.Start(fileName); // Noncompliant
            P.Start(fileName); // Noncompliant
            new System.Diagnostics.ProcessStartInfo(fileName); // Noncompliant
            new PSI(fileName); // Noncompliant
        }
    }
}
