using Microsoft.JSInterop;

namespace RazorClassLib;

public class S6798_CSharpOnly
{
    [JSInvokable] private void JSInvokablePrivateMethodInPartialRazorComponent() { } // Noncompliant
}
