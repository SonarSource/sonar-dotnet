using System;

namespace Tests.Diagnostics
{
    public partial class MyClass
    {
        private void PrivateMethodOnlyCalledFromGenerated()
        {
        }

        internal void InternalMethodOnlyCalledFromGenerated()
        {
        }
    }
}
