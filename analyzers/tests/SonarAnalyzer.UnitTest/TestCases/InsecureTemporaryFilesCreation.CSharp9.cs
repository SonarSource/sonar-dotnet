using System;
using System.IO;

var tempPath = Path.GetTempFileName(); // Noncompliant {{'Path.GetTempFileName()' is insecure. Use 'Path.GetRandomFileName()' instead.}}
//             ^^^^^^^^^^^^^^^^^^^^

_ = Path.GetTempFileName(); // Noncompliant

string Get() => Path.GetTempFileName(); // Noncompliant

public record Record
{
    private readonly string tempFileName;

    public string TempFileName
    {
        init => tempFileName = Path.Combine(value, Path.GetTempFileName()); // Noncompliant
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
