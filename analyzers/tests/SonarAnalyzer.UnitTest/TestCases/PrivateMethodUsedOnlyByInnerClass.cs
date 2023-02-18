using System.Runtime.InteropServices;

class OuterClass
{
    static void OnlyUsedOnceByNestedClass() { } // Noncompliant {{Move the method inside 'NestedClass'.}}
    //          ^^^^^^^^^^^^^^^^^^^^^^^^^
    static void OnlyUsedByNestedClassMultipleTimes() { } // Noncompliant
    static void OnlyUsedByNestedClassWithClassName() { } // Noncompliant
    static void UsedByMultipleSiblingNestedClasses() { } // Compliant - it needs to stay in the outer class
    static void UsedByOuterAndNestedClasses() { } // Compliant - it's used by the outer class, so it needs to stay there
    static void UsedBySiblingAndChildClasses() { } // Compliant - SiblingNestedClass and DeeperNestedClass both need access to the method, so it must stay in the outer class

    static void OnlyUsedByDeeperNestedClass() { } // Noncompliant {{Move the method inside 'DeeperNestedClass'.}}
    static void UsedByNestedClassAndDeeperNestedClass() { } // Noncompliant {{Move the method inside 'NestedClass'.}}

    void NotStatic() { } // Compliant - not static

    public static void PublicMethod() { } // Compliant - not private
    protected static void ProtectedMethod() { } // Compliant - not private
    internal static void InternalMethod() { } // Compliant - not private
    protected internal static void ProtectedInternalMethod() { } // Compliant - not private
    private static void PrivateMethod() { } // Noncompliant

    static T GenericMethod<T>(T arg) => arg; // Noncompliant
    //       ^^^^^^^^^^^^^

    static int Recursive(int n) => Recursive(n - 1) // Noncompliant
    static void MutuallyRecursive1() => MutuallyRecursive2(); // FN - both methods could be moved inisde the nested class
    static void MutuallyRecursive2() => MutuallyRecursive1();

    [DllImport("SomeLibrary.dll")]
    private static extern void ExternalMethod(); // Noncompliant

    void Foo()
    {
        UsedByOuterAndNestedClasses();
    }

    class NestedClass
    {
        void Bar()
        {
            OnlyUsedOnceByNestedClass();
            OnlyUsedByNestedClassMultipleTimes();
            OuterClass.OnlyUsedByNestedClassWithClassName();
            UsedByMultipleSiblingNestedClasses();
            UsedByOuterAndNestedClasses();
            UsedByNestedClassAndDeeperNestedClass();
            new OuterClass().NotStatic();
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
                UsedBySiblingAndChildClasses();
            }
        }
    }

    class SiblingNestedClass
    {
        void Baz()
        {
            UsedByMultipleSiblingNestedClasses();
            UsedBySiblingAndChildClasses();
        }
    }
}

// C# 11 - file modifier
// extern
// recursion
// partial
// records
// static classes
// generics
