using System;
using System.ComponentModel;
using System.IO;

namespace CSharp9
{
    public interface IFoo { }
    class FooBase : IFoo { }
    class Foo1 : FooBase { }
    public struct MyStruct { }

    public record FirstRecord { }
    public record SecondRecord // Noncompliant {{Split this record into smaller and more specialized ones to reduce its dependencies on other types from 6 to the maximum authorized 1 or less.}}
    {
        private FirstRecord field1; // +1
        private MyStruct field2; // +1
        private Foo1 field3; // +1
        private nuint field5; // Primitives don't count
        private UIntPtr field6; // Primitives don't count

        public SecondRecord(IFoo interfaceFoo) { } // +1

        private static FooBase Property1 { get; } // +1

        public FirstRecord FooMethod() => field1; // already in field1
        public void BarMethod(Stream s) { } // +1
    }

    public record PositionalRecord(int Parameter) // Noncompliant {{Split this record into smaller and more specialized ones to reduce its dependencies on other types from 6 to the maximum authorized 1 or less.}}
    {
        private FirstRecord field1; // +1
        private MyStruct field2; // +1
        private Foo1 field3; // +1
        private nuint field5; // Primitives don't count
        private UIntPtr field6; // Primitives don't count

        public PositionalRecord(IFoo interfaceFoo) : this(5) { } // +1

        private static FooBase Property1 { get; } // +1

        public FirstRecord FooMethod() => field1; // already in field1
        public void BarMethod(Stream s) { } // +1
    }

    public record OutterRecord
    {
        InnerRecord whatever = new InnerRecord();

        public record InnerRecord // Noncompliant
        {
            public Stream stream = new FileStream("", FileMode.Open);
        }
    }
}

namespace CSharp10
{
    public interface IFoo { }
    class FooBase : IFoo { }
    class Foo1 : FooBase { }
    public struct MyStruct { }

    public record struct FirstRecordStruct { }
    public record struct SecondRecordStruct // Noncompliant {{Split this record into smaller and more specialized ones to reduce its dependencies on other types from 6 to the maximum authorized 1 or less.}}
    //                   ^^^^^^^^^^^^^^^^^^
    {
        private FirstRecordStruct field1 = new FirstRecordStruct(); // +1
        private MyStruct field2 = new MyStruct(); // +1
        private Foo1 field3 = null; // +1
        private nuint field5 = 42; // Primitives don't count

        public SecondRecordStruct(IFoo interfaceFoo) { } // +1

        private static FooBase Property1 { get; } // +1

        public FirstRecordStruct FooMethod() => field1; // already in field1
        public void BarMethod(Stream s) { } // +1
    }

    public record struct PositionalRecordStruct(int Parameter) // Noncompliant
    {
        private FirstRecordStruct field1 = new FirstRecordStruct(); // +1
        private MyStruct field2 = new MyStruct(); // +1
        private Foo1 field3 = null; // +1
        private nuint field5 = 42; // Primitives don't count

        public PositionalRecordStruct(IFoo interfaceFoo) : this(5) { } // +1

        private static FooBase Property1 { get; } // +1

        public FirstRecordStruct FooMethod() => field1; // already in field1
        public void BarMethod(Stream s) { } // +1
    }

    public record struct OutterRecordStruct // Noncompliant
    {
        public OutterRecordStruct() { }

        InnerRecordStruct whatever = new InnerRecordStruct();

        public record struct InnerRecordStruct // Noncompliant
        {
            public InnerRecordStruct() { }

            public Stream stream = new FileStream("", FileMode.Open);
        }
    }
}

namespace CSharp11
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

namespace CSharp13
{
    public interface IFoo { }
    public class FooBase : IFoo { }
    public class Foo1 : FooBase { }
    public class Foo2 : FooBase { }
    public struct MyStruct { }

    public partial class Partial // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 3 to the maximum authorized 1 or less.}}
    {
        public partial FooBase property { get; set; }
        public MyStruct myStruct;
        public Foo2 foo2;
    }

    public partial class Partial // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
    {
        public partial FooBase property
        {
            get => new Foo1();
            set { }
        }
    }
}
