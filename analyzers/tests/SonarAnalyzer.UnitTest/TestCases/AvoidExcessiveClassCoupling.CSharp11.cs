using System;
using System.ComponentModel;
using System.IO;

namespace Tests.Diagnostics
{
    public interface IFoo { }
    class FooBase : IFoo { }
    class Foo : FooBase { }
    public struct MyStruct { }

    public class ZeroDependencies { } // Compliant

    public class ZeroNonPrimitiveDependencies // Compliant
    {
        private nint nativeInt; // Primitives don't count
        private nuint nativeUint; // Primitives don't count

        private IntPtr intPtr; // Primitives don't count
        private UIntPtr uIntPtr; // Primitives don't count

        private void Method_IntPtr(IntPtr arg) { } // Primitives don't count
        private void Method_UIntPtr(UIntPtr arg) { } // Primitives don't count
        private void Method_nint(nint arg) { } // Primitives don't count
        private void Method_nuint(nuint arg) { } // Primitives don't count
    }

    public class OneDependency // Compliant
    {
        private Foo foo; // +1

        private nint nativeInt; // Primitives don't count
        private nuint nativeUint; // Primitives don't count

        private IntPtr intPtr; // Primitives don't count
        private UIntPtr uIntPtr; // Primitives don't count

        private class NestedClass // Noncompliant
//                    ^^^^^^^^^^^
        {
            private Foo nestedFoo; // +1
            private FooBase GetFooBase() => default; // +1
        }
    }

    public class TwoDependencies // Noncompliant
//               ^^^^^^^^^^^^^^^
    {
        private Foo foo; // +1
        private MyStruct myStruct; // +1

        private nint nativeInt; // Primitives don't count
        private nuint nativeUint; // Primitives don't count

        private IntPtr intPtr; // Primitives don't count
        private UIntPtr uIntPtr; // Primitives don't count

        private class NestedClass // Compliant
        {
            private IFoo nestedIFoo; // +1

            private nint nativeInt; // Primitives don't count
            private nuint nativeUint; // Primitives don't count

            private IntPtr intPtr; // Primitives don't count
            private UIntPtr uIntPtr; // Primitives don't count

            private void DoWork(IFoo iFoo) { } // Already counted in private field
        }

        private class NestedEmptyClass // Compliant
        {
        }
    }

    // file-scoped types

    file interface IFooFile { }
    file class FooFileBase : IFooFile { }
    file class FooFileClass1 : FooFileBase { }

    file class FooSecond // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 3 to the maximum authorized 1 or less.}}
//             ^^^^^^^^^
    {
        private FooFileClass1 field2 = new FooFileClass1(); // +1
        private FooFileBase field3 = null; // +1
        private static FooBase Property1 { get; } // +1
    }
}
