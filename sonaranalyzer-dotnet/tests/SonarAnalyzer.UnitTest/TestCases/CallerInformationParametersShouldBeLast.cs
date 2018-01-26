using System;
using System.Runtime.CompilerServices;
namespace Tests.Diagnostics
{
    class Program
    {
        public void Method1(string callerFilePath) { }
        public void Method2([CallerFilePath]string callerFilePath) { }
        public void Method3(string other, [CallerFilePath]string callerFilePath) { }
        public void Method4([CallerFilePath]string callerFilePath, string other) { } // Noncompliant {{Move 'callerFilePath' to the end of the parameter list.}}
//                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public void Method5([CallerFilePath]string callerFilePath, string other, [CallerLineNumber]int callerLineNumber) { }
//                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public void Method6(string first, [CallerFilePath]string callerFilePath, [CallerLineNumber]int callerLineNumber, string other) { }
//                                                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
//                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1

        public void Method7([CallerFilePath]string callerFilePath,
//                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            [CallerLineNumber]int callerLineNumber,
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            [CallerMemberName]string callerMemberName, string other) { }
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        public Program([CallerFilePath]string callerFilePath, string other) { } // Noncompliant
        public Program(string other, [CallerFilePath]string callerFilePath) { }
    }

    class InvalidSyntax
    {
        public void Method1( { }
        public void Method2) { }
        public void Method3([CallerFilePath) { }
        public void Method4([CallerFilePathAttribute string parameter) { }
    }
}
