using System.Collections.Generic;
using System.Diagnostics;
using System.Security;

public class Program
{
    private string compliantField = @"C:\file.exe";
    private string noncompliantField = "file.exe";

    private Process field = Process.Start("file.exe");                      // Noncompliant
    public Process PropertyRW { get; set; } = Process.Start("file.exe");    // Noncompliant
    public Process PropertyRO => Process.Start("file.exe");                 // Noncompliant
    public Process PropertyUnused { get; set; }     // For coverage

    public void Invocations(SecureString password)
    {
        var compliantVariable = @"C:\file.exe";
        var noncompliantVariable = @"file.exe";
        string nullVariable = null;
        var startInfo = new ProcessStartInfo("bad.exe");    // Noncompliant {{Make sure the "PATH" variable only contains fixed, unwriteable directories.}}
        //              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        // Compliant
        Process.Start(startInfo);       // Not tracked here, it's already raised on the constructor
        Process.Start("");
        Process.Start(nullVariable);
        Process.Start(@"C:\file.exe");
        Process.Start(@"C:\file.exe", "arguments");
        Process.Start(@"C:\file.exe", "arguments", "userName", password, "domain");
        Process.Start(@"C:\file.exe", "userName", password, "domain");
        Process.Start(compliantField);
        Process.Start(compliantVariable);
        new ProcessStartInfo();
        new ProcessStartInfo(@"C:\file.exe");
        new ProcessStartInfo(@"C:\file.exe", "arguments");

        Process.Start("file.exe");                      // Noncompliant
        Process.Start("file.exe", "arguments");         // Noncompliant
        Process.Start("file.exe", "arguments", "userName", password, "domain"); // Noncompliant
        Process.Start("file.exe", "userName", password, "domain");              // Noncompliant
        Process.Start(noncompliantField);               // Noncompliant
        Process.Start(noncompliantVariable);            // Noncompliant
        new ProcessStartInfo("file.exe");               // Noncompliant
        new ProcessStartInfo("file.exe", "arguments");  // Noncompliant

        // Reassignment
        compliantField = noncompliantVariable;
        Process.Start(compliantField);                  // Noncompliant
        noncompliantVariable = compliantVariable;
        Process.Start(noncompliantVariable);            // Compliant after reassignment
    }

    public void Properties(ProcessStartInfo arg)
    {
        arg.FileName = @"C:\file.exe";
        arg.FileName = "file.exe";              // Noncompliant

        var psi = new ProcessStartInfo(@"C:\file.exe");
        psi.FileName = "file.exe";              // Noncompliant

        psi = new ProcessStartInfo("bad.exe");  // Noncompliant, later assignment is not tracked
        psi.FileName = @"C:\file.exe";

        psi = new ProcessStartInfo() { FileName = @"C:\file.exe" };
        psi = new ProcessStartInfo { FileName = "bad.exe" };        // Noncompliant
        psi = new ProcessStartInfo() { FileName = "bad.exe" };      // Noncompliant
        psi = new ProcessStartInfo() { FileName = "bad.exe" };      // Noncompliant FP, value is reassigned later
        psi.FileName = @"C:\file.exe";

        psi = new ProcessStartInfo() { FileName = "bad.exe" };     // Noncompliant
        Process.Start(psi);
        psi.FileName = @"C:\file.exe";

        psi = new ProcessStartInfo() { FileName = "bad.exe" };     // Noncompliant
        Run(psi);
        psi.FileName = @"C:\file.exe";
    }

    private void Run(ProcessStartInfo psi) => Process.Start(psi);

    public void PathFormat()
    {
        // Compliant prefixes
        Process.Start("/file");
        Process.Start("/File");
        Process.Start("/dir/dir/dir/file");
        Process.Start("//////file");        // Compliant, we don't validate the path format itself
        Process.Start("/file.exe");
        Process.Start("./file.exe");
        Process.Start("../file.exe");
        Process.Start("c:");
        Process.Start("c:/file.exe");
        Process.Start("C:/file.exe");
        Process.Start("D:/file.exe");
        Process.Start("//server/file.exe");
        Process.Start("//server/dir/file.exe");
        Process.Start("//server/c$/file.exe");
        Process.Start("//10.0.0.1/dir/file.exe");
        Process.Start(@"\file.exe");
        Process.Start(@"\\\\\\file");        // Compliant, we don't validate the path format itself
        Process.Start(@".\file.exe");
        Process.Start(@"..\file.exe");
        Process.Start(@"c:\file.exe");
        Process.Start(@"C:\file.exe");
        Process.Start(@"C:file.exe");       // Missing "\" after drive letter: Valid relative path from current directory of the C: drive
        Process.Start(@"C:Dir\file.exe");   // Missing "\" after drive letter: Valid relative path from current directory of the C: drive
        Process.Start(@"C:\dir\file.exe");
        Process.Start(@"D:\file.exe");
        Process.Start(@"z:\file.exe");
        Process.Start(@"Z:\file.exe");
        Process.Start(@"\\server\file.exe");
        Process.Start(@"\\server\dir\file.exe");
        Process.Start(@"\\server\c$\file.exe");
        Process.Start(@"\\10.0.0.1\dir\file.exe");
        Process.Start("http://www.microsoft.com");
        Process.Start("https://www.microsoft.com");
        Process.Start("ftp://www.microsoft.com");
        // Compliant, custom protocols used to start an application
        Process.Start("skype:echo123?call");
        Process.Start("AA:/file.exe");
        Process.Start("Ř:/file.exe");
        Process.Start("ř:/file.exe");
        Process.Start(@"AA:\file.exe");
        Process.Start(@"Ř:\file.exe");
        Process.Start(@"ř:\file.exe");
        Process.Start("0:/file.exe");
        Process.Start(@"0:\file.exe");

        Process.Start("file");          // Noncompliant
        Process.Start("file.exe");      // Noncompliant
        Process.Start("File.bat");      // Noncompliant
        Process.Start("dir/file.cmd");  // Noncompliant
        Process.Start("-file.com");     // Noncompliant
        Process.Start("@file.cpl");     // Noncompliant
        Process.Start("$file.dat");     // Noncompliant
        Process.Start(".file.txt");     // Noncompliant
        Process.Start(".|file.fake");   // Noncompliant
        Process.Start("~/file.exe");    // Noncompliant
        Process.Start("...file.exe");   // Noncompliant
        Process.Start(".../file.exe");  // Noncompliant
        Process.Start(@"...\file.exe"); // Noncompliant
    }
}

namespace CustomType
{
    public class ProcessStartInfo
    {
        public string FileName { get; set; }

        public ProcessStartInfo(string fileName) { }

        public static void Usage()
        {
            var psi = new ProcessStartInfo("bad.exe");  // Compliant with this custom class
            psi.FileName = "file.exe";                  // Compliant
        }
    }
}
