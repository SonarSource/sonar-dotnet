using System;
using System.IO;

public class Sample
{
    private string tempFileName;

    public string TempFileName
    {
        set => tempFileName = Path.Combine(value, Path.GetTempFileName()); // Noncompliant
        get => tempFileName;
    }

    public void Method() => Path.GetTempFileName(); // Noncompliant

    public void Safe() => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()); // Compliant
}

public class Class
{
    public Func<string> GetTempFileNameGenerator() => Path.GetTempFileName; // Noncompliant
//                                                    ^^^^^^^^^^^^^^^^^^^^

    public void Test()
    {
        Consume(Path.GetTempFileName); // Noncompliant

        _ = Custom.Path.GetTempFileName(); // Compliant, method from other namespace
    }

    private static void Consume(Func<string> generator) => generator();
}

namespace Custom
{
    public static class Path
    {
        public static string GetTempFileName() => string.Empty;
    }
}
