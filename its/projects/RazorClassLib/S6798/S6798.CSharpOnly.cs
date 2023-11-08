using Microsoft.JSInterop;

namespace RazorClassLib;

public class S6798_CSharpOnly
{
    [JSInvokable] private void JSInvokablePrivateMethodInPartialRazorComponent() { }
    [JSInvokable] void JSInvokableImplicitelyPrivateMethodInPartialRazorComponent() { }
    [JSInvokable] public void JSInvokablePublicMethodInPartialRazorComponent() { }

    void SomeMethod()
    {
        [JSInvokable] void JSInvokablePrivateMethodInLocalFunction() { }
    }
}
