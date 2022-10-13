using System;
using System.IO;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Examples()
        {
            tmp = Environment.GetEnvironmentVariable("""TMPDIR"""); // Noncompliant 
        }
    }
}
