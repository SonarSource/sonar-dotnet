using System;
using System.Runtime.CompilerServices;
namespace Tests.Diagnostics
{
    class InvalidSyntax
    {
        public void Method0() {}
        public void () {}
        public void Method1( { }
        public void Method2) { }
        public void Method3([CallerFilePath]) { }
        public void Method4([CallerFilePath],string other) { }
        public void Method5([CallerFilePath] string, string other) { }
        public void Method6([CallerFilePathAttribute string parameter) { }
        public void Method6([CallerLineNumber][CallerFilePath]string callerFilePath, string other) { } // Noncompliant
    }
}
