using Microsoft.JSInterop;

namespace BlazorSample.Pages.S6798;

public class S6798_CSharpOnly
{
    [JSInvokable] void JSInvokableImplicitelyPrivateMethodInPartialRazorComponent() { } // Noncompliant
}
