﻿using System;
using System.Runtime.CompilerServices;


void Local1([CallerFilePath] string callerFilePath = "") { }
void Local2(string other, [CallerFilePath] string callerFilePath = "") { }
void Local3([CallerFilePath] string callerFilePath = "", string other = "") { } // Noncompliant

class TestProgram
{
    public void OuterMethod() {
        Local1();
        Local2("");
        Local3();
        Local4("");
        void Local1([CallerFilePath] string callerFilePath = "") { }
        void Local2(string other, [CallerFilePath] string callerFilePath = "") { }
        void Local3([CallerFilePath] string callerFilePath = "", string other = "") { } // Noncompliant
        static void Local4(string first, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, string other = "") { }
        //                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //                                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^@-1
    }
}
