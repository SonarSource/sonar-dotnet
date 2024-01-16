using System;

namespace Tests.Diagnostics
{
    public partial class ClassImplementedInTwoFiles
    {
        private sealed class InnerPrivateClassExtension : InnerPrivateClass { }
    }
}
