using Microsoft;
using Microsoft.JSInterop;
using System;

class Visibilities
{
    [JSInvokable] private protected void PrivateProtectedMethod() { }                 // Noncompliant
    //                                   ^^^^^^^^^^^^^^^^^^^^^^
    [JSInvokable] private protected static void PrivateProtectedStaticMethod1() { }   // Noncompliant
    //                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    [JSInvokable] private static protected void PrivateProtectedStaticMethod2() { }   // Noncompliant
    //                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
}

class LocalFunctions
{
    void Test()
    {
        [JSInvokable] void LocalFunction()
        { }               // Compliant: not a method
        [JSInvokable] static void LocalStaticFunction()
        { }  // Compliant: not a method
    }
}
