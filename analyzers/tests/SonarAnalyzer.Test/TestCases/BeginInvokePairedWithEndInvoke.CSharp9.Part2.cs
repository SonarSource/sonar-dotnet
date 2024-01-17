using System;

namespace Tests.Diagnostics
{
    public partial class CallerWrapperAnotherFile
    {
        public partial void CallEndInvoke(IAsyncResult result) =>
            caller.EndInvoke(result);

        public partial void DoNothing(IAsyncResult result) { }
    }
}
