using System;
using System.Runtime.CompilerServices;


public static class Debug
{
    public static void AssertCompliant_01(bool condition, [CallerArgumentExpression("condition")] string message = null) { }
    public static void AssertCompliant_02(bool condition = true, [CallerArgumentExpression("condition")] string message = null) { }
    public static void AssertNoncompliant([CallerArgumentExpression("condition")] string message = null, bool condition = true) { } // Noncompliant
}
