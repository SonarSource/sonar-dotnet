using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

void Empty()
{
    // Method intentionally left empty.
} // Fixed

void WithComment()
{
    // because
}

void WithTrailingComment()
{// because

}

void NotEmpty()
{
    Console.WriteLine();
}

int Lambda(int x) => x;

record EmptyMethod
{
    void F2()
    {
        // Do nothing because of X and Y.
    }

    void F3()
    {
        Console.WriteLine();
    }

    [Conditional("DEBUG")]
    void F4()    // Fixed
    {
        // Method intentionally left empty.
    }

    protected virtual void F5()
    {
    }

    extern void F6();

    [DllImport("avifil32.dll")]
    private static extern void F7();

    void F8()
    {
        void F9() // Fixed
        {
            // Method intentionally left empty.
        }
    }
}

abstract record MyR
{
    void F1()
    {
        // Method intentionally left empty.
    } // Fixed
    public abstract void F2();
}

record MyR2 : MyR
{
    public override void F2()
    {
    }
}

class M
{
    [ModuleInitializer]
    internal static void M1() // Fixed
    {
        // Method intentionally left empty.
    }

    [ModuleInitializer]
    internal static void M2()
    {
        // reason
    }

    [ModuleInitializer]
    internal static void M3()
    {
        Console.WriteLine();
    }
}

namespace D
{
    partial class C
    {
        public partial void Foo();
        public partial void Bar();
        public partial void Qix();
    }

    partial class C
    {
        public partial void Foo()
        {
            // Method intentionally left empty.
        } // Fixed

        public partial void Bar()
        {
            // comment
        }

        public partial void Qix()
        {
            Console.WriteLine();
        }
    }
}

class PropertyAccessors
{
    int NonEmptyInitProp { init { int x; } }
    int EmptyInitProp {
        init
        {
            // Method intentionally left empty.
        }
    }                   // Fixed
    int EmptyInitPropWithGet { get => 42; init
        {
            // Method intentionally left empty.
        }
    } // Fixed
    int AutoInitPropWithGet { get; init; }           // Compliant, auto-implemented, so not-empty

    int NonEmptySetProp { set { int x; } }
    int EmptySetProp {
        set
        {
            // Method intentionally left empty.
        }
    }                     // Fixed
    int EmptySetPropWithGet { get => 42; set
        {
            // Method intentionally left empty.
        }
    }   // Fixed
    int AutoSetPropWithGet { get; set; }             // Compliant, auto-implemented, so not-empty

    class Base
    {
        protected virtual int VirtualEmptyInitProp { init { } }  // Compliant, virtual
    }

    class Inherited : Base
    {
        protected override int VirtualEmptyInitProp {
            init
            {
                // Method intentionally left empty.
            }
        } // Fixed
    }

    class Hidden : Base
    {
        protected new int VirtualEmptyInitProp {
            init
            {
                // Method intentionally left empty.
            }
        }      // Fixed
    }
}

class EmptyProperty
{
    int EmptyProp { } // Error [CS0548] property or indexer must have at least one accessor
}

class LocalFunction
{
    void FirstLevelInMethod()
    {
        void NonEmpty() { int i; }              // Compliant
        void Empty()
        {
            // Method intentionally left empty.
        }                        // Fixed
        static void EmptyStatic()
        {
            // Method intentionally left empty.
        }           // Fixed
        extern static void EmptyExternStatic(); // Compliant, no body
        unsafe void EmptyUnsafe()
        {
            // Method intentionally left empty.
        }           // Fixed
        async void EmptyAsync()
        {
            // Method intentionally left empty.
        }             // Fixed
    }

    void NestedInMethod()
    {
        void FirstLevelLocalFunction()
        {
            void NonEmpty() { int i; }          // Compliant
            void Empty()
            {
                // Method intentionally left empty.
            }                    // Fixed

            void SecondLevelLocalFunction()     // Compliant, contains a local functions
            {
                void NonEmpty() { int i; }      // Compliant
                void Empty()
                {
                    // Method intentionally left empty.
                }                // Fixed
            }
        }
    }

    int FirstLevelInAccessor
    {
        set
        {
            void NonEmpty() { int i; }              // Compliant
            void Empty()
            {
                // Method intentionally left empty.
            }                        // Fixed
            static void EmptyStatic()
            {
                // Method intentionally left empty.
            }           // Fixed
            extern static void EmptyExternStatic(); // Compliant, no body
            unsafe void EmptyUnsafe()
            {
                // Method intentionally left empty.
            }           // Fixed
            async void EmptyAsync()
            {
                // Method intentionally left empty.
            }             // Fixed
        }
    }

    int NestedInAccessor
    {
        init
        {
            void FirstLevelLocalFunction()
            {
                void NonEmpty() { int i; }          // Compliant
                void Empty()
                {
                    // Method intentionally left empty.
                }                    // Fixed

                void SecondLevelLocalFunction()     // Compliant, contains local functions
                {
                    void NonEmpty() { int i; }      // Compliant
                    void Empty()
                    {
                        // Method intentionally left empty.
                    }                // Fixed
                }
            }
        }
    }
}
