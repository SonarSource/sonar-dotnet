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
        private static void ThisIsCompliant() { }    // Noncompliant {{Move this method inside ''.}} FP https://sonarsource.atlassian.net/browse/NET-2668
    }
}
