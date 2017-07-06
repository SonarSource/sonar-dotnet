using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class Bar
    {
        public List<T> Method1<T>(T arg) => null;
//             ^^^^^^^ Noncompliant {{Refactor this method to use a generic collection designed for inheritance.}}

        private List<T> Method2<T>(T arg) => null;

        private void Method3(int arg) => null;

        public List<int> Method4(List<string> someParam) => null;
//             ^^^^^^^^^ Noncompliant
//                               ^^^^^^^^^^^^ Noncompliant@-1

        public IList<int> Method5(ICollection<string> someParam) => null;

        public List<T> field, field2;
//             ^^^^^^^ Noncompliant {{Refactor this field to use a generic collection designed for inheritance.}}

        public List<int> Property { get; set; }
//             ^^^^^^^^^ Noncompliant {{Refactor this property to use a generic collection designed for inheritance.}}

        public Bar(List<Bar> bars) { } // Noncompliant  {{Refactor this constructor to use a generic collection designed for inheritance.}}
    }

    public class InvalidCode
    {
        public List<int> () => null;

        public List<T> { get; set; }

        public List<InvalidType> Method() => null;

        public InvalidType Method2() => null;
    }
}
