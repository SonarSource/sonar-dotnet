﻿using System;
using System.IO;

namespace Tests.Diagnostics
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
