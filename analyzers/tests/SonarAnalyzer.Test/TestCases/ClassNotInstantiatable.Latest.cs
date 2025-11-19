using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharp9
{
    public record Person(string FirstName, string LastName); // Compliant

    public record EmptyRecord { } // Compliant

    public record Person1 // Noncompliant {{This record can't be instantiated; make its constructor 'public'.}}
    {
        public string FirstName { get; }

        private Person1() { }
    }

    public record Person2 // Noncompliant {{This record can't be instantiated; make at least one of its constructors 'public'.}}
    {
        public string FirstName { get; }

        private Person2() { }
        private Person2(string first) => FirstName = first;
    }

    public sealed record Person3 // Compliant
    {
        public string FirstName { get; init; }

        private Person3() { }
        public static Person3 Instance => new Person3();
    }

    public record Person4(string FirstName) // Compliant
    {
        private Person4() : this("") { }
    }

    public record Person5(string FirstName) // Compliant
    {
        public Person5() : this("") { }
    }

    public record StaticUsage // Noncompliant
    {
        private StaticUsage() { }
        public static void M() { }
    }

    public record OuterRecord // Compliant
    {
        private OuterRecord() { }

        public record Intermediate
        {
            public record Nested : OuterRecord // Noncompliant
            {
                private Nested() { }
            }
        }
    }

    public record MyGenericRecord<T>
    {
        private MyGenericRecord() { }
        public record Nested : MyGenericRecord<int> { }
    }

    public class MyGenericRecord2<T>
    {
        private MyGenericRecord2() { }
        public object Create() => new MyGenericRecord2<int>();
    }

    public class MyAttribute : System.Attribute { }

    [My]
    public record WithAttribute1
    {
        private WithAttribute1() { }
    }

    public record WithAttribute2
    {
        [My]
        private WithAttribute2() { }
    }

    public class Foo
    {
        public static readonly Foo Instance = new();

        public bool IsActive => true;

        private Foo() { }
    }

    public class Baz { }

    public class Bar  // Noncompliant
    {
        public static readonly Baz Instance = new();

        public bool IsActive => true;

        private Bar() { }
    }
}

namespace CSharp11
{
    file record Person(string FirstName, string LastName); // Compliant

    file record Person1 // Noncompliant {{This record can't be instantiated; make its constructor 'public'.}}
    {
        public string FirstName { get; }
        private Person1() { }
    }

    file class Baz { }

    file class Bar  // Noncompliant
    {
        private Bar() { }
    }
}

namespace CSharp14
{
    partial class PartialPublicConstructor
    {
        public partial PartialPublicConstructor();
    }

    partial class PartialPrivateConstructor // Noncompliant {{This class can't be instantiated; make its constructor 'public'.}}
    {
        private partial PartialPrivateConstructor();
    }
}
