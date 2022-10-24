using System.Diagnostics;

public class Program
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
