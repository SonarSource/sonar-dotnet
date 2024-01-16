using System.Diagnostics;

class PrimaryConstructor(string ctorParam = "file.exe")
{
    void Method(string methodParam = "file.exe")
    {
        Process.Start(ctorParam); // FN
        Process.Start(methodParam); // FN
        var lambda = (string lambdaParam = "file.exe") => Process.Start(lambdaParam); // FN
    }
}
