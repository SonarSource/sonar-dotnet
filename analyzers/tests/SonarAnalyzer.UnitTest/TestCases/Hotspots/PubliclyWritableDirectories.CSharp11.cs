using System;
using System.IO;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Examples()
        {
            var tmp = Environment.GetEnvironmentVariable("""TMPDIR"""); // Noncompliant 
        }
    }
}
