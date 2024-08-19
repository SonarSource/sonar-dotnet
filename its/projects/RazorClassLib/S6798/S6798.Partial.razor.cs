using Microsoft.JSInterop;

namespace RazorClassLib;

public partial class S6798_Partial
{
    [JSInvokable] private void JSInvokablePrivateMethodInPartialRazorComponent() { } // Noncompliant

    void SomeMethod()
    {
        [JSInvokable] void JSInvokablePrivateMethodInLocalFunction() { }             // Compliant: local function
    }
}
