using System.Diagnostics;

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

class PrimaryConstructor(string ctorParam = "file.exe")
{
    void Method(string methodParam = "file.exe")
    {
        Process.Start(ctorParam); // FN
        Process.Start(methodParam); // FN
        var lambda = (string lambdaParam = "file.exe") => Process.Start(lambdaParam); // FN
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
