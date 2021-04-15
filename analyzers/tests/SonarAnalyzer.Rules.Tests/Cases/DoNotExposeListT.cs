using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Tests.Diagnostics
{
    public class Bar<T>
    {
        public List<T> Method1<T>(T arg) => null;
//             ^^^^^^^ Noncompliant {{Refactor this method to use a generic collection designed for inheritance.}}

        private List<T> Method2<T>(T arg) => null;

        private object Method3(int arg) => null;

        public List<int> Method4(List<string> someParam) => null;
//             ^^^^^^^^^ Noncompliant
//                               ^^^^^^^^^^^^ Noncompliant@-1

        public IList<int> Method5(ICollection<string> someParam) => null;

        public List<T> field, field2;
//             ^^^^^^^ Noncompliant {{Refactor this field to use a generic collection designed for inheritance.}}

        public List<int> Property { get; set; }
//             ^^^^^^^^^ Noncompliant {{Refactor this property to use a generic collection designed for inheritance.}}

        public Bar(List<Bar<int>> bars) { } // Noncompliant  {{Refactor this constructor to use a generic collection designed for inheritance.}}

        protected List<int> ProtectedMethod() => null; // Noncompliant


        protected internal List<int> ProtectedInternalMethod() => null; // Noncompliant

        internal List<int> InternalMethod() => null;

        internal List<int> InternalProperty { get; set; }

        public void Foo()
        {
            Action<List<int>> x = (List<int> y) => { };
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/3728
        [XmlElement(ElementName = "Names")]
        public List<string> _DoNotUse_Names_Property => null;

        [XmlElementAttribute(ElementName = "Names")]
        public List<string> _DoNotUse_Names_FullAttribute => null;

        [XmlElement(ElementName = "Names")]
        public List<string> _DoNotUse_Names_Field = null;

        [XmlAttribute(AttributeName = "Names")]
        public List<string> _DoNotUse_Names_Attribute => null;  // Noncompliant, exception doesn't apply to other Xml attributes

        [XmlIgnore]
        public List<string> _DoNotUse_Names_Ignore => null;     // Noncompliant, if you don't need to serialize it, don't expose it.
    }
}
