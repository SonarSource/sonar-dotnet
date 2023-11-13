using Microsoft.JSInterop;

namespace BlazorSample.Pages;

public class S6798_CSharpOnly
{
    private void JSInvokablePrivateMethodInPartialRazorComponent() { }
    [JSInvokable] void JSInvokableImplicitelyPrivateMethodInPartialRazorComponent() { }
    [JSInvokable] public void JSInvokablePublicMethodInPartialRazorComponent() { }

    void SomeMethod()
    {
        [JSInvokable] void JSInvokablePrivateMethodInLocalFunction() { }
    }
}
