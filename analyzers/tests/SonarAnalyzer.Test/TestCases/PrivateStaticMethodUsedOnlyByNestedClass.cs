using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class OuterClass
{
    static void OnlyUsedOnceByNestedClass() { }                     // Noncompliant {{Move this method inside 'NestedClass'.}}
    //          ^^^^^^^^^^^^^^^^^^^^^^^^^
    static void OnlyUsedByNestedClassMultipleTimes() { }            // Noncompliant
    static void OnlyUsedByNestedClassWithClassName() { }            // Noncompliant
    static void UsedByMultipleSiblingNestedClasses() { }            // Compliant - it needs to stay in the outer class
    static void UsedByOuterAndNestedClasses() { }                   // Compliant - it's used by the outer class, so it needs to stay there
    static void UsedBySiblingAndDeeperNestedClasses() { }           // Compliant - SiblingNestedClass and DeeperNestedClass both need access to the method, so it must stay in the outer class
    static void OnlyUsedByDeeperNestedClass() { }                   // Noncompliant {{Move this method inside 'DeeperNestedClass'.}}
    static void UsedByNestedClassAndDeeperNestedClass() { }         // Noncompliant {{Move this method inside 'NestedClass'.}}
    static void UsedByDeeperNestedClassesOnTheSameLevel() { }       // Noncompliant {{Move this method inside 'NestedClass'.}}
    static void UnusedMethod() { }                                  // Compliant - no need to move unused method anywhere

    void NotStatic() { }                                            // Compliant - method is not static
    static int _outerField;                                         // Compliant - not a method
    static int OuterProp { get; set; }                              // Compliant - not a method

    public static void PublicMethod() { }                           // Compliant - method is not private
    protected static void ProtectedMethod() { }                     // Compliant - method is not private
    internal static void InternalMethod() { }                       // Compliant - method is not private
    protected internal static void ProtectedInternalMethod() { }    // Compliant - method is not private
    private static void PrivateMethod() { }                         // Noncompliant
    private static void PrivateMethod(int arg) { }                  // Compliant - overloaded version of the previous method, not used anywhere

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
    static void AssignedToDelegateInNestedClass() { }               // Noncompliant
    static void UsedInNameOfExpressionInNestedClass() { }           // Noncompliant 

    void Foo()
    {
        UsedByOuterAndNestedClasses();
    }

    class NestedClass
    {
        int _nestedField = UsedOnlyByFieldInitializerInNestedClass();
        int NestedProp => UsedOnlyByPropertyInNestedClass();

        public NestedClass()
        {
            UsedOnlyByConstructorInNestedClass();
        }

        static void NestedClassMethodUsedByDeeperNestedClass() { }  // Noncompliant {{Move this method inside 'DeeperNestedClass'.}}

        void Bar()
        {
            OnlyUsedOnceByNestedClass();
            OnlyUsedByNestedClassMultipleTimes();
            OuterClass.OnlyUsedByNestedClassWithClassName();
            UsedByMultipleSiblingNestedClasses();
            UsedByOuterAndNestedClasses();
            UsedByNestedClassAndDeeperNestedClass();

            new OuterClass().NotStatic();
            _outerField = 42;
            OuterProp = 42;

            PublicMethod();
            ProtectedMethod();
            InternalMethod();
            ProtectedInternalMethod();
            PrivateMethod();

            GenericMethod(42);

            Recursive(42);
            MutuallyRecursive1();

            ExternalMethod();

            Action methodDelegate = AssignedToDelegateInNestedClass;
            string methodName = nameof(UsedInNameOfExpressionInNestedClass);
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
                UsedByDeeperNestedClassesOnTheSameLevel();
                UsedBySiblingAndDeeperNestedClasses();
                NestedClassMethodUsedByDeeperNestedClass();
            }
        }

        class AnotherDeeperNestedClass
        {
            void Foo()
            {
                UsedByDeeperNestedClassesOnTheSameLevel();
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
    static void OnlyUsedByNestedStruct() { }                        // Noncompliant

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
    private static void OnlyUsedByNestedClass() { }                 // Noncompliant

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
    static void OnlyUsedByNestedClass() { }                         // Compliant - partial classes are often a result of code generation, so their methods shouldn't be moved
    static partial void PartialOnlyUsedByNestedClass() { }          // Compliant
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

[DebuggerDisplay("{UsedByDebuggerDisplay()}")]
class DebugViewClass
{
    static string UsedByDebuggerDisplay() => "";                    // Noncompliant - FP: should not be moved to nested class, because it's also used by the attribute

    class NestedClass
    {
        void Foo()
        {
            UsedByDebuggerDisplay();
        }
    }
}

public class EdgeCaseWithLongCommonPaths
{
    private static void StaticMethod() { }                           // Noncompliant {{Move this method inside 'InsideMiddleOne'.}}

    public class MiddleOne
    {
        public class InsideMiddleOne
        {
            public class Foo
            {
                public class FooLeaf
                {
                    public void Method() => StaticMethod();
                }
            }

            public class Bar
            {
                public void Method() => StaticMethod();
                public class BarLeaf
                {
                    public void Method() => StaticMethod();
                }
            }
        }
    }
    public class MiddleTwo {
        public void StaticMethod()
        {
        }
        public void Method() => StaticMethod();
    }
}
