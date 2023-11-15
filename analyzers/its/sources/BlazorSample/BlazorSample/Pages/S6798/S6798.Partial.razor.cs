using Microsoft.JSInterop;

namespace BlazorSample.Pages.S6798;

public partial class S6798_Partial
{
    [JSInvokable] private void JSInvokablePrivateMethodInPartialRazorComponent() { }
    [JSInvokable] void JSInvokableImplicitelyPrivateMethodInPartialRazorComponent() { }
    [JSInvokable] public void JSInvokablePublicMethodInPartialRazorComponent() { }

    void SomeMethod()
    {
        [JSInvokable] void JSInvokablePrivateMethodInLocalFunction() { }
    }
}
