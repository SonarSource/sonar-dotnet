using System.Diagnostics;

class PrimaryConstructor(string ctorParam = "file.exe")
{
    void Method(string methodParam = "file.exe")
    {
        Process.Start(ctorParam); // Noncompliant
        Process.Start(methodParam); // Noncompliant
        var lambda = (string lambdaParam = "file.exe") => Process.Start(lambdaParam); // Noncompliant
    }
}
