using System;
using System.IO;

namespace Tests.Diagnostics
{
    public interface IFoo { }
    class FooBase : IFoo { }
    class Foo1 : FooBase { }
    public struct MyStruct { }

    public record struct FirstRecordStruct { }
    public record struct SecondRecordStruct // FN
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

    public record struct PositionalRecordStruct(int Parameter) // FN
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

    public record struct OutterRecordStruct
    {
        InnerRecordStruct whatever = new InnerRecordStruct();

        public record struct InnerRecordStruct // FN
        {
            public Stream stream = new FileStream("", FileMode.Open);
        }
    }
}
