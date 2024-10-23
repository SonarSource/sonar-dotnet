using System;

namespace CSharp9
{
    public record Record
    {
        public virtual void MyNotOverriddenMethod() { }
    }

    internal partial record PartialRecordDeclaredOnlyOnce // Noncompliant
    {
        void Method() { }
    }

    internal partial record PartialPositionalRecordDeclaredOnlyOnce(string parameter) // Noncompliant
    {
        void Method() { }
    }

    internal partial record PartialDeclaredMultipleTimes
    {
    }

    internal partial record PartialDeclaredMultipleTimes
    {
    }

    abstract record BaseRecord
    {
        public abstract void MyOverriddenMethod();

        public abstract int Prop { get; set; }
    }

    sealed record SealedRecord : BaseRecord
    {
        public sealed override void MyOverriddenMethod() { } // Noncompliant

        public sealed override int Prop { get; set; } // Noncompliant
    }

    internal record BaseRecord<T>
    {
        public virtual string Process(string input)
        {
            return input;
        }
    }

    internal record SubRecord : BaseRecord<string>
    {
        public override string Process(string input) => "Test";
    }

    internal unsafe record UnsafeRecord // Noncompliant
    {
        int num;

        private unsafe delegate void MyDelegate2(int i); // Noncompliant

        unsafe void M() // Noncompliant
        {
        }

        unsafe ~UnsafeRecord() // Noncompliant
        {
        }
    }

    public record Foo
    {
        public unsafe record Bar // Noncompliant
        {
        }

        unsafe interface MyInterface
        {
            unsafe int* Method(); // Noncompliant
        }

        public static void M()
        {
            checked
            {
                checked // Noncompliant
    //          ^^^^^^^
                {
                    var z = 1 + 4;
                    var y = unchecked(1 +
                        unchecked(4)); // Noncompliant
    //                  ^^^^^^^^^
                }
            }

            checked // Noncompliant {{'checked' is redundant in this context.}}
            {
                var f = 5.5;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = unchecked(5.5 + 4); // Noncompliant
            }

            unchecked
            {
                var f = 5.5;
                var y = unchecked(5 + 4); // Noncompliant
            }

            checked
            {
                var x = (uint)10;
                var y = (int)x;
            }

            checked // Noncompliant
            {
                var x = 10;
                var y = (double)x;
            }

            checked
            {
                var x = 10;
                x += int.MaxValue;
            }

            checked // Noncompliant, FP, nuint/UIntPtr is not a member of KnownType.IntegralNumbers.
            {
                nint x = 42;
                nuint y = 42;

                x += 42;
                y += 42;
            }
        }
    }

    public unsafe record RecordNewSyntax(string Input) // Noncompliant For the class
    {
        private string inputField = Input;
    }
}

namespace CSharp10
{
    internal unsafe record struct UnsafeRecordStruct // Noncompliant
    {
        int num;

        private unsafe delegate void MyDelegate2(int i); // Noncompliant

        unsafe void M() // Noncompliant
        {
        }
    }

    public record struct Foo
    {
        public unsafe record struct Bar // Noncompliant
        {
        }

        unsafe interface MyInterface
        {
            unsafe int* Method(); // Noncompliant
        }

        public static void M()
        {
            checked
            {
                checked // Noncompliant
                {
                    var z = 1 + 4;
                    var y = unchecked(1 +
                        unchecked(4)); // Noncompliant
                }
            }

            checked // Noncompliant
            {
                var f = 5.5;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = unchecked(5.5 + 4); // Noncompliant
            }

            unchecked
            {
                var f = 5.5;
                var y = unchecked(5 + 4); // Noncompliant
            }

            checked
            {
                var x = (uint)10;
                var y = (int)x;
            }

            checked // Noncompliant
            {
                var x = 10;
                var y = (double)x;
            }

            checked
            {
                var x = 10;
                x += int.MaxValue;
            }
        }
    }

    public unsafe record struct RecordNewSyntax(string Input) // Noncompliant
    {
        private string inputField = Input;
    }
}

namespace CSharp11
{
    public class Foo
    {
        public static void Method()
        {
            checked // Noncompliant, FP, nint/IntPtr is not a member of KnownType.IntegralNumbers.
            {
                nint x = 42;
                IntPtr y = 2;
                var _ = x + y;
            }

            checked // Noncompliant, FP, nuint/UIntPtr is not a member of KnownType.IntegralNumbers.
            {
                nuint x = 42;
                UIntPtr y = 2;
                var _ = x + y;
            }
        }
    }

    // file-scoped types cannot use accessibility modifiers and cannot be nested.

    file partial class PartialFoo { } // Noncompliant

    file partial class PartialFooBar { } // Noncompliant

    file partial class PartialFileClass { }
    file partial class PartialFileClass { }


    file unsafe class UnsafeClass
    {
        int* pointer;
    }

    file unsafe class UnsafeClass2 // Noncompliant
//       ^^^^^^
    {
        int num;
    }

    file unsafe interface MyInterface
    {
        unsafe int* Method(); // Noncompliant
    }
}

namespace CSharp13
{
    public class Base
    {
        public virtual int Value { get; }
    }

    public sealed partial class Thingy : Base
    {
        public override sealed partial int Value { get; } // Noncompliant
    }
}
