using Microsoft.JSInterop;

public partial class JSInvokableMethodsShouldBePublic
{
    [JSInvokable] private void JSInvokablePrivateMethodInPartialRazorComponent() { }    // Noncompliant
    [JSInvokable] void JSInvokableImplicitelyPrivateMethodInPartialRazorComponent() { } // Noncompliant
    [JSInvokable] public void JSInvokablePublicMethodInPartialRazorComponent() { }      // Compliant: public

    void SomeMethod()
    {
        [JSInvokable] void JSInvokablePrivateMethodInLocalFunction() { }                // Compliant: local function
    }
}


