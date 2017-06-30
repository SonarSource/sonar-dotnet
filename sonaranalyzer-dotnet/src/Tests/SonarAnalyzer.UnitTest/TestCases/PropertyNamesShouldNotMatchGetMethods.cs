using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public int Foo // Noncompliant {{Change either the name of the property 'Foo' or the name of the method 'GetFoo' to make them distinguishable.}}
//                 ^^^
        { get; set; }
        public int GetFoo()
//                 ^^^^^^ Secondary
        { }

        public DateTime Date { get; }
        public string GetDateAsString()
        {
            return Date.ToString();
        }

        public string Bar // Noncompliant {{Change either the name of the property 'Bar' or the name of the method 'Bar' to make them distinguishable.}}
//                    ^^^
        { get; }
        public int Bar()
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

        public int { get; } // Missing identifier on purpose
        public int () { return 42; } // Missing identifier on purpose
    }
}