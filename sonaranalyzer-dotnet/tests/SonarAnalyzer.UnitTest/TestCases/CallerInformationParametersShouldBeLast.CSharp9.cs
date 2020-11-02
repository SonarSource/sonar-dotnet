using System;
using System.Runtime.CompilerServices;


void Local1([CallerFilePath] string callerFilePath = "") { }
void Local2(string other, [CallerFilePath] string callerFilePath = "") { }
void Local3([CallerFilePath] string callerFilePath = "", string other = "") { } // FN

class Program
{
    public void OuterMethod() {
        void Local1([CallerFilePath] string callerFilePath = "") { }
        void Local2(string other, [CallerFilePath] string callerFilePath = "") { }
        void Local3([CallerFilePath] string callerFilePath = "", string other = "") { } // FN
        static void Local4(string first, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, string other = "") { }
    }
}
