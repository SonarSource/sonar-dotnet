using System;

namespace Tests.Diagnostics
{
    public class ClassWithRecord
    {
        private record RecordNotExtended { } // Noncompliant

        private record PrivateRecord { }

        private sealed record PrivateRecordExtension : PrivateRecord { }
    }

    public record PublicRecord
    {
        private record NonsealedPrivateRecord { } // Noncompliant

        private record NonsealedPrivatePositionalRecord(string Property) { } // Noncompliant
    }

    public record PublicPositionalRecord
    {
        private record NonsealedPrivateRecord { } // Noncompliant

        private record NonsealedPrivatePositionalRecord(string Property) { } // Noncompliant
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
