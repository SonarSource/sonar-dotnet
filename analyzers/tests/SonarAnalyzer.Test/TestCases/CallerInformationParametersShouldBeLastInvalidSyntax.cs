using System;
using System.Runtime.CompilerServices;
namespace Tests.Diagnostics
{
    class InvalidSyntax
    {
        public void Method0() {}
        public void () {}           // Error [CS1519, CS8124, CS1519, CS1519]
        public void Method1( { }    // Error [CS1026]
        public void Method2) { }    // Error [CS0670, CS1002, CS1022, CS1513]
        public void Method3([CallerFilePath]) { }                           // Error [CS0106, CS8107, CS8803, CS8805, CS8107, CS4018, CS1001, CS1031]
        public void Method4([CallerFilePath],string other) { }              // Error [CS0106, CS8107, CS4018, CS1001, CS1031]
        public void Method5([CallerFilePath] string, string other) { }      // Error [CS0106, CS8107, CS4021, CS1001]
        public void Method6([CallerFilePathAttribute string parameter) { }  // Error [CS0106, CS8107, CS1003, CS0246, CS0246, CS1003, CS1001, CS1003, CS1031]
        // Error@+1 [CS0106, CS0128, CS8107, CS4017, CS8107, CS4021]
        public void Method6([CallerLineNumber][CallerFilePath]string callerFilePath, string other) { } // Noncompliant
    }   // Error [CS1022]
}       // Error [CS1022]
