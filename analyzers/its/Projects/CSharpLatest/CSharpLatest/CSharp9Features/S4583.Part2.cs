using System;

namespace CSharpLatest.CSharp9Features.S4583;

public partial class CallerWrapperAnotherFile
{
    public partial void CallEndInvoke(IAsyncResult result) =>
        caller.EndInvoke(result);

    public partial void DoNothing(IAsyncResult result) { }
}
