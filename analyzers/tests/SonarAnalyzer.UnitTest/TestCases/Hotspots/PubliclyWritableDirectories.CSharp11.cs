using System;
using System.IO;

namespace Tests.Diagnostics
{
    public class Program
    {
        void RawStringLiterals()
        {
            var tmp = Environment.GetEnvironmentVariable("""TMPDIR"""); // Noncompliant 
        }

        void NewlinesInStringInterpolation(string firstPartOfPath, string secondPartOfPath)
        {
            string dir = $"/tmp/{firstPartOfPath + // Noncompliant
                secondPartOfPath}";
            string dirRawString = $$"""/tmp/{{firstPartOfPath + // Noncompliant
                            secondPartOfPath}}""";
        }
    }
}
