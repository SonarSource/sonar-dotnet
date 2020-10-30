using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public record Person(string FirstName, string LastName); // Compliant

    public record EmptyRecord { } // Compliant

    public record Person1 // FN
    {
        public string FirstName { get; }

        private Person1() { }
    }

    public record Person2 // FN
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

    public record StaticUsage // FN, should use a utility class instead
    {
        private StaticUsage() { }
        public static void M() { }
    }

    public record OuterRecord // Compliant
    {
        private OuterRecord() { }

        public record Intermediate
        {
            public record Nested : OuterRecord // FN
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
}

// See https://github.com/dotnet/roslyn/issues/45510
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
