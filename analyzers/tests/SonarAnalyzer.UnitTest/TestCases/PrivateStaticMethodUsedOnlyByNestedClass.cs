using System.Runtime.InteropServices;

class OuterClass
{
    static void OnlyUsedOnceByNestedClass() { }                     // Noncompliant {{Move the method inside 'NestedClass'.}}
    //          ^^^^^^^^^^^^^^^^^^^^^^^^^
    static void OnlyUsedByNestedClassMultipleTimes() { }            // Noncompliant
    static void OnlyUsedByNestedClassWithClassName() { }            // Noncompliant
    static void UsedByMultipleSiblingNestedClasses() { }            // Compliant - it needs to stay in the outer class
    static void UsedByOuterAndNestedClasses() { }                   // Compliant - it's used by the outer class, so it needs to stay there
    static void UsedBySiblingAndDeeperNestedClasses() { }           // Compliant - SiblingNestedClass and DeeperNestedClass both need access to the method, so it must stay in the outer class
    static void OnlyUsedByDeeperNestedClass() { }                   // Noncompliant {{Move the method inside 'DeeperNestedClass'.}}
    static void UsedByNestedClassAndDeeperNestedClass() { }         // Noncompliant {{Move the method inside 'NestedClass'.}}
    static void UnusedMethod() { }                                  // Compliant - no need to move unused method anywhere

    void NotStatic() { }                                            // Compliant - method is not static

    public static void PublicMethod() { }                           // Compliant - method is not private
    protected static void ProtectedMethod() { }                     // Compliant - method is not private
    internal static void InternalMethod() { }                       // Compliant - method is not private
    protected internal static void ProtectedInternalMethod() { }    // Compliant - method is not private
    private static void PrivateMethod() { }                         // Noncompliant

    static T GenericMethod<T>(T arg) => arg;                        // Noncompliant
    //       ^^^^^^^^^^^^^

    static int Recursive(int n) => Recursive(n - 1);                // Noncompliant
    static void MutuallyRecursive1() => MutuallyRecursive2();       // FN - both methods could be moved inisde the nested class
    static void MutuallyRecursive2() => MutuallyRecursive1();

    [DllImport("SomeLibrary.dll")]
    private static extern void ExternalMethod();                    // Noncompliant

    static int UsedOnlyByPropertyInNestedClass() => 42;             // Noncompliant
    static int UsedOnlyByFieldInitializerInNestedClass() => 42;     // Noncompliant
    static void UsedOnlyByConstructorInNestedClass() { }            // Noncompliant

    void Foo()
    {
        UsedByOuterAndNestedClasses();
    }

    class NestedClass
    {
        int _field = UsedOnlyByFieldInitializerInNestedClass();

        int Prop => UsedOnlyByPropertyInNestedClass();

        public NestedClass()
        {
            UsedOnlyByConstructorInNestedClass();
        }

        static void OnlyUsedByDeeperNestedClass() { }                   // Noncompliant {{Move the method inside 'DeeperNestedClass'.}}

        void Bar()
        {
            OnlyUsedOnceByNestedClass();
            OnlyUsedByNestedClassMultipleTimes();
            OuterClass.OnlyUsedByNestedClassWithClassName();
            UsedByMultipleSiblingNestedClasses();
            UsedByOuterAndNestedClasses();
            UsedByNestedClassAndDeeperNestedClass();

            new OuterClass().NotStatic();

            PublicMethod();
            ProtectedMethod();
            InternalMethod();
            ProtectedInternalMethod();
            PrivateMethod();

            GenericMethod(42);

            Recursive(42);
            MutuallyRecursive1();

            ExternalMethod();
        }

        void FooBaz()
        {
            OnlyUsedByNestedClassMultipleTimes();
        }

        class DeeperNestedClass
        {
            void FooBar()
            {
                OnlyUsedByDeeperNestedClass();
                UsedByNestedClassAndDeeperNestedClass();
                UsedBySiblingAndDeeperNestedClasses();
                OnlyUsedByDeeperNestedClass();
            }
        }
    }

    class SiblingNestedClass
    {
        void Baz()
        {
            UsedByMultipleSiblingNestedClasses();
            UsedBySiblingAndDeeperNestedClasses();
        }
    }
}

class ClassContainsStruct
{
    static void OnlyUsedByNestedStruct() { }                            // Noncompliant

    struct NestedStruct
    {
        void Foo()
        {
            OnlyUsedByNestedStruct();
        }
    }
}

struct StructContainsClass
{
    static void OnlyUsedByNestedClass() { }                             // Noncompliant

    class NestedClass
    {
        void Foo()
        {
            OnlyUsedByNestedClass();
        }
    }
}

partial class PartialOuterClass
{
    static void OnlyUsedByNestedClass() { }                             // Noncompliant
    static partial void PartialOnlyUsedByNestedClass() { }              // Noncompliant
}

partial class PartialOuterClass
{
    static partial void PartialOnlyUsedByNestedClass();

    class NestedClass
    {
        void Foo()
        {
            OnlyUsedByNestedClass();
            PartialOnlyUsedByNestedClass();
        }
    }
}

// static classes
// nameof
// top-level methods
