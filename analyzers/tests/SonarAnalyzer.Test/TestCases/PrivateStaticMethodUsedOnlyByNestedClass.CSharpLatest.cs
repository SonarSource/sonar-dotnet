namespace CSharp8
{
    class OuterClass
    {
        static void OnlyUsedByNestedInterface() { }                 // Noncompliant
        private protected static void PrivateProtectedMethod() { }  // Compliant - method is not private

        interface INestedInterface
        {
            void Foo()
            {
                OnlyUsedByNestedInterface();
                PrivateProtectedMethod();
            }
        }
    }

    interface IOuterInterface
    {
        static void OnlyUsedByNestedClass() { }                     // Noncompliant

        class NestedClass
        {
            void Foo()
            {
                OnlyUsedByNestedClass();
            }
        }
    }
}

namespace CSharp9
{
    record OuterRecord
    {
        static void UsedOnlyByNestedClass() { }  // Noncompliant
        static void UsedOnlyByNestedRecord() { } // Noncompliant

        class NestedClass
        {
            void Foo()
            {
                UsedOnlyByNestedClass();
            }
        }

        record NestedRecord
        {
            void Foo()
            {
                UsedOnlyByNestedRecord();
            }
        }
    }
}

namespace CSharp10
{
    record class OuterRecordClass
    {
        static void UsedOnlyByNestedClass() { }  // Noncompliant
        static void UsedOnlyByNestedRecord() { } // Noncompliant

        class NestedClass
        {
            void Foo()
            {
                UsedOnlyByNestedClass();
            }
        }

        record class NestedRecord
        {
            void Foo()
            {
                UsedOnlyByNestedRecord();
            }
        }
    }

    record struct OuterRecordStruct
    {
        private static void UsedOnlyByNestedClass() { }  // Noncompliant
        private static void UsedOnlyByNestedRecord() { } // Noncompliant

        class NestedClass
        {
            void Foo()
            {
                UsedOnlyByNestedClass();
            }
        }

        record struct NestedRecord
        {
            void Foo()
            {
                UsedOnlyByNestedRecord();
            }
        }
    }
}

namespace CSharp14
{
    public static class ExtensionMethods
    {
        extension(ExtensionMethods)
        {
            public static void Compliant()
            {
                ThisIsCompliant();
            }
        }
        private static void ThisIsCompliant() { } // Compliant
    }
}

namespace CSharp15
{
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union
    public union UnionRepro(UnionCaseA, UnionCaseB)
    {
        private static void UsedOnlyByNestedClass() { } // FN NET-4957

        public class NestedClass
        {
            void Foo()
            {
                UsedOnlyByNestedClass();
            }
        }

        private static void UsedByBoth() { } // Compliant, also used by the outer type

        public class OtherNestedClass
        {
            void Foo()
            {
                UsedByBoth();
            }
        }

        public static void CallUsedByBoth() => UsedByBoth();
    }

    public record class UnionCaseA(string Name);
    public record class UnionCaseB(string Name);
}
