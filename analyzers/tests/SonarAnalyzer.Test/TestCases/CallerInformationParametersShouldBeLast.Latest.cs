using System;
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

public static class Debug
{
    public static void AssertCompliant_01(bool condition, [CallerArgumentExpression("condition")] string message = null) { }
    public static void AssertCompliant_02(bool condition = true, [CallerArgumentExpression("condition")] string message = null) { }
    public static void AssertNoncompliant([CallerArgumentExpression("condition")] string message = null, bool condition = true) { } // Noncompliant
}

interface MyInterface
{
    static abstract void Method(string callerFilePath, string other);
}

class DerivedClass : MyInterface
{
    public static void Method([CallerFilePath] string callerFilePath = "", string other = "") // Compliant, method from interface
    {
    }
}

class PrimaryConstructorClassCompliant(string other, [CallerFilePath] string callerFilePath = "");

class PrimaryConstructorClassNoncompliant([CallerFilePath] string callerFilePath = "", string other = ""); // Noncompliant

record PrimaryConstructorRecordNoncompliant([CallerFilePath] string callerFilePath = "", string other = ""); // Noncompliant

record struct PrimaryConstructorRecordStructNoncompliant([CallerFilePath] string callerFilePath = "", string other = ""); // Noncompliant
