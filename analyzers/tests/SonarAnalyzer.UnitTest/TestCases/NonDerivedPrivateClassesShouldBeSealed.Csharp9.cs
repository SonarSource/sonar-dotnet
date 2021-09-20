using System;

namespace Tests.Diagnostics
{
    public class ClassWithRecord
    {
        private record RecordNotExtended { } // Noncompliant

        private record PrivateRecord { }

        private sealed record PrivateRecordExtension : PrivateRecord { }
    }

    public partial class APartialClass
    {
        private record PrivateRecord { }
    }

    public partial class APartialClass
    {
        private sealed record PrivateRecordExtension : PrivateRecord { }
    }
}
