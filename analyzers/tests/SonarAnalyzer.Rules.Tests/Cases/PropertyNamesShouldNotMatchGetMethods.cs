using System;

namespace Tests.Diagnostics
{
    public class Base
    {
        public virtual int MyProperty { get; set; }
    }

    public class Program : Base
    {
        public int Foo // Noncompliant {{Change either the name of property 'Foo' or the name of method 'GetFoo' to make them distinguishable.}}
//                 ^^^
        { get; set; }
        public int GetFoo()
//                 ^^^^^^ Secondary
        { return 1; }

        public DateTime Date { get; }
        public string GetDateAsString()
        {
            return Date.ToString();
        }

        public string Bar // Noncompliant {{Change either the name of property 'Bar' or the name of method 'Bar' to make them distinguishable.}}
//                    ^^^
        { get; }
        public int Bar() // Error [CS0102]
//                 ^^^ Secondary
        {
            return 42;
        }

        private string Color { get; } // Compliant - property is private
        public string GetColor() { return ""; }

        public string Day { get; } // Compliant - method is private
        private string GetDay() { return ""; }

        protected string Whatever // Noncompliant
        { get; }

        public string GetWhatever() // Secondary
        {
            return "";
        }

        public string SomeWeirdCase // Noncompliant
        { get; }

        public string SOMEWEIRDCASE() // Secondary
        {
            return "";
        }

        public override int MyProperty { get; set; } // Compliant - override
        public int GetMyProperty() => 42;
    }
}
