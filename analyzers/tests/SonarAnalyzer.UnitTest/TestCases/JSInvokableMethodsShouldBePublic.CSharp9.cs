using Microsoft;
using Microsoft.JSInterop;
using System;

class LocalFunctions
{
    void Test()
    {
        [JSInvokable] void LocalFunction() { }               // Compliant: not a method
        [JSInvokable] static void LocalStaticFunction() { }  // Compliant: not a method
    }
}
