using System;

namespace Tests.Diagnostics
{
    public record struct RecordStruct
    {
        private int a = 42; // Noncompliant {{Remove the unused private field 'a'.}}

        private int b = 42;
        public int B() => b;

        private nint Value { get; init; } = 42;

        private nint UnusedValue { get; init; } = 42; // FN

        public RecordStruct Create() => new() {Value = 1};

        private interface IFoo // Noncompliant
        {
            public void Bar() { }
        }

        private record struct Nested(string Name, int CategoryId);

        public void UseNested()
        {
            Nested d = new("name", 2);
        }

        private record struct Nested2(string Name, int CategoryId);

        public void UseNested2()
        {
            _ = new Nested2("name", 2);
        }

        private record struct UnusedNested1(string Name, int CategoryId); // FN
        internal record struct UnusedNested2(string Name, int CategoryId); // FN
        public record struct UnusedNested3(string Name, int CategoryId);

        private int usedInPatternMatching = 1;

        public int UseInPatternMatching(int val) =>
            val switch
            {
                < 0 => usedInPatternMatching,
                >= 0 => 1
            };

        private class LocalFunctionAttribute : Attribute { }
        private class LocalFunctionAttribute2 : Attribute { }

        public void Foo()
        {
            [LocalFunction]
            static void Bar() { }

            [Obsolete]
            [NotExisting] // Error [CS0246]
                          // Error@-1 [CS0246]
            [LocalFunctionAttribute2]
            [LocalFunction]
            static void Quix() { }

            [Obsolete]
            static void ForCoverage() { }

            static void NoAttribute() { }
        }
    }

    public record struct PositionalRecord(string Value)
    {
        private int a = 42; // Noncompliant
        private int b = 42;
        public int B() => b;

        private record struct UnusedNested(string Name, int CategoryId) { } // FN
    }
}
