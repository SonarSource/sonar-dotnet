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

    public void Invocations(SecureString password)
    {
        var compliantVariable = @"C:\file.exe";
        var noncompliantVariable = @"file.exe";
        string nullVariable = null;
        var startInfo = new ProcessStartInfo("bad.exe");    // FN {{Make sure the "PATH" used to find this command includes only what you intend.}}
        //FIXME: Mark location

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
        new ProcessStartInfo("file.exe");               // FN
        new ProcessStartInfo("file.exe", "arguments");  // FN

        // Reassignment
        compliantField = noncompliantVariable;
        Process.Start(compliantField);                  // Noncompliant
        noncompliantVariable = compliantVariable;
        Process.Start(noncompliantVariable);            // Compliant after reassignment
    }

    public void Properties(ProcessStartInfo arg)
    {
        arg.FileName = @"C:\file.exe";
        arg.FileName = "file.exe";              // FN
        //FIXME: Mark location

        var psi = new ProcessStartInfo(@"C:\file.exe");
        psi.FileName = "file.exe";              // FN

        psi = new ProcessStartInfo("bad.exe");  // Compliant, safe value is assigned later
        psi.FileName = @"C:\file.exe";

        psi = new ProcessStartInfo("bad.exe");  // FN, safe value is assigned later, but we don't track the intermediate usage
        Process.Start(psi);
        psi.FileName = @"C:\file.exe";

        psi = new ProcessStartInfo("bad.exe");  // FN, safe value is assigned later, but we don't track the intermediate usage
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
        Process.Start("AA:/file.exe");  // Noncompliant
        Process.Start("0:/file.exe");   // Noncompliant
        Process.Start("Ř:/file.exe");   // Noncompliant
        Process.Start("ř:/file.exe");   // Noncompliant
        Process.Start(@"...\file.exe"); // Noncompliant
        Process.Start(@"AA:\file.exe"); // Noncompliant
        Process.Start(@"0:\file.exe");  // Noncompliant
        Process.Start(@"Ř:\file.exe");  // Noncompliant
        Process.Start(@"ř:\file.exe");  // Noncompliant
    }
}
