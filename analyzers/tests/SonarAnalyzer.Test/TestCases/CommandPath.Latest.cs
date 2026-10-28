using System.Diagnostics;
using System.Threading.Tasks;

public class Program
{
    private const string fileName = "file";
    private const string extension = "exe";
    private const string compliantField = @$"C:\{fileName}.{extension}";
    private const string noncompliantField = @$"{fileName}.{extension}";

    public void SomeMethod()
    {
        Process.Start(noncompliantField); // Noncompliant
        Process.Start(compliantField);
    }
}

public class Program2
{
    private const string fileName = """file""";
    private const string extension = """exe""";
    private const string compliantField = $"""C:\{fileName}.{extension}""";
    private const string noncompliantField = $"""{fileName}.{extension}""";
    private const string noncompliantNonInterpolatedField = """file.exe""";

    public void SomeMethod()
    {
        Process.Start(noncompliantField); // Noncompliant
        Process.Start(compliantField);
        Process.Start(noncompliantNonInterpolatedField); // Noncompliant
    }
}

// NET-1673
class PrimaryConstructor(string ctorParam = "file.exe")
{
    void Method(string methodParam = "file.exe")
    {
        Process.Start(ctorParam); // Noncompliant
        Process.Start(methodParam); // Noncompliant
        var lambda = (string lambdaParam = "file.exe") => Process.Start(lambdaParam); // Noncompliant
    }
}

class NewEscapeSequence
{
    private const string fileName = ".\efile";

    void EscapeSequence()
    {
        Process.Start(".\efile.fake");   // Noncompliant
        Process.Start("\e/file.exe");    // Noncompliant
        Process.Start($"{fileName}.exe");   // Noncompliant
    }
}

class ProcessRunMethods
{
    async Task Methods(ProcessStartInfo startInfo)
    {
        Process.Run("file.exe");                                                // FN NET-4878
        Process.Run("file.exe", ["arg"], silent: true);                         // FN NET-4878
        await Process.RunAsync("file.exe");                                     // FN NET-4878
        Process.RunAndCaptureText("file.exe");                                  // FN NET-4878
        await Process.RunAndCaptureTextAsync("file.exe");                       // FN NET-4878
        Process.StartAndForget("file.exe");                                     // FN NET-4878
        Process.Run(@"C:\file.exe");
        Process.StartAndForget("/usr/bin/file");
        Process.Run(startInfo);
        Process.Run(new ProcessStartInfo("file.exe"));                          // Noncompliant
        Process.StartAndForget(new ProcessStartInfo { FileName = "file.exe" }); // Noncompliant
    }
}
