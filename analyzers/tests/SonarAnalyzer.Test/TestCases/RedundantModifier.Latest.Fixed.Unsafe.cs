using System;

namespace CSharp9
{
    public record Record
    {
        public virtual void MyNotOverriddenMethod() { }
    }

    internal partial record PartialRecordDeclaredOnlyOnce // Fixed
    {
        void Method() { }
    }

    internal partial record PartialPositionalRecordDeclaredOnlyOnce(string parameter) // Fixed
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
        public sealed override void MyOverriddenMethod() { } // Fixed

        public sealed override int Prop { get; set; } // Fixed
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

    internal record UnsafeRecord // Fixed
    {
        int num;

        private delegate void MyDelegate2(int i); // Fixed

        void M() // Fixed
        {
        }

        ~UnsafeRecord() // Fixed
        {
        }
    }

    public record Foo
    {
        public record Bar // Fixed
        {
        }

        unsafe interface MyInterface
        {
            int* Method(); // Fixed
        }

        public static void M()
        {
            checked
            {
                checked // Fixed
                {
                    var z = 1 + 4;
                    var y = unchecked(1 +
                        unchecked(4)); // Fixed
                }
            }

            checked // Fixed
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
                var y = unchecked(5.5 + 4); // Fixed
            }

            unchecked
            {
                var f = 5.5;
                var y = unchecked(5 + 4); // Fixed
            }

            checked
            {
                var x = (uint)10;
                var y = (int)x;
            }

            checked // Fixed
            {
                var x = 10;
                var y = (double)x;
            }

            checked
            {
                var x = 10;
                x += int.MaxValue;
            }

            checked // Fixed
            {
                nint x = 42;
                nuint y = 42;

                x += 42;
                y += 42;
            }
        }
    }

    public record RecordNewSyntax(string Input) // Fixed
    {
        private string inputField = Input;
    }
}

namespace CSharp10
{
    internal record struct UnsafeRecordStruct // Fixed
    {
        int num;

        private delegate void MyDelegate2(int i); // Fixed

        void M() // Fixed
        {
        }
    }

    public record struct Foo
    {
        public record struct Bar // Fixed
        {
        }

        unsafe interface MyInterface
        {
            int* Method(); // Fixed
        }

        public static void M()
        {
            checked
            {
                checked // Fixed
                {
                    var z = 1 + 4;
                    var y = unchecked(1 +
                        unchecked(4)); // Fixed
                }
            }

            checked // Fixed
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
                var y = unchecked(5.5 + 4); // Fixed
            }

            unchecked
            {
                var f = 5.5;
                var y = unchecked(5 + 4); // Fixed
            }

            checked
            {
                var x = (uint)10;
                var y = (int)x;
            }

            checked // Fixed
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

    public record struct RecordNewSyntax(string Input) // Fixed
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
            checked // Fixed
            {
                nint x = 42;
                IntPtr y = 2;
                var _ = x + y;
            }

            checked // Fixed
            {
                nuint x = 42;
                UIntPtr y = 2;
                var _ = x + y;
            }
        }
    }

    // file-scoped types cannot use accessibility modifiers and cannot be nested.

    file partial class PartialFoo { } // Fixed

    file partial class PartialFooBar { } // Fixed

    file partial class PartialFileClass { }
    file partial class PartialFileClass { }


    file unsafe class UnsafeClass
    {
        int* pointer;
    }

    file class UnsafeClass2 // Fixed
    {
        int num;
    }

    file unsafe interface MyInterface
    {
        int* Method(); // Fixed
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
        public override sealed partial int Value { get; } // Fixed
    }
}
