using System;

public partial class CallerWrapperAnotherFile
{
    public partial void CallEndInvoke(IAsyncResult result) =>
        caller.EndInvoke(result);

    public partial void DoNothing(IAsyncResult result) { }
}

public partial class CrossTreeCallbackField
{
    private AsyncCallback callbackFieldNoncompliant = new AsyncCallback(HandlerWithoutEndInvoke);

    private static void HandlerWithoutEndInvoke(IAsyncResult result) { }
}
