using System;

namespace Tests.Diagnostics
{
    public record Base
    {
        public virtual int MyProperty { get; set; }
    }

    public record Record : Base
    {
        public int Foo // Noncompliant
        { get; set; }
        public int GetFoo() // Secondary
        { return 1; }

        public DateTime Date { get; }
        public string GetDateAsString()
        {
            return Date.ToString();
        }

        public string Bar // Noncompliant
        { get; }
        // Error@+1 [CS0102]
        public int Bar()    // Secondary
        //         ^^^
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

    public record PositionalRecord : Base
    {
        public int Foo // Noncompliant
        { get; set; }
        public int GetFoo() // Secondary
        { return 1; }

        public DateTime Date { get; }
        public string GetDateAsString()
        {
            return Date.ToString();
        }

        public string Bar // Noncompliant
        { get; }
        public int Bar() // Error [CS0102]
                         // Secondary@-1
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

    public record struct RecordStruct
    {
        public int Foo // Noncompliant
        { get; set; }
        public int GetFoo() // Secondary
        { return 1; }

        public DateTime Date { get; }
        public string GetDateAsString()
        {
            return Date.ToString();
        }

        public string Bar   // Noncompliant
        { get; }
        public int Bar()    // Error [CS0102]
                            // Secondary@-1
        {
            return 42;
        }

        private string Color { get; }   // Compliant - property is private
        public string GetColor() { return ""; }

        public string Day { get; }      // Compliant - method is private
        private string GetDay() { return ""; }

        public string GetWhatever()
        {
            return "";
        }

        public string SomeWeirdCase // Noncompliant
        { get; }

        public string SOMEWEIRDCASE() // Secondary
        {
            return "";
        }

        public int GetMyProperty() => 42;
    }

    public record struct PositionalRecordStruct
    {
        public int Foo // Noncompliant
        { get; set; }
        public int GetFoo() // Secondary
        { return 1; }

        public DateTime Date { get; }
        public string GetDateAsString()
        {
            return Date.ToString();
        }

        public string Bar   // Noncompliant
        { get; }
        public int Bar()    // Error [CS0102]
                            // Secondary@-1
        {
            return 42;
        }

        private string Color { get; } // Compliant - property is private
        public string GetColor() { return ""; }

        public string Day { get; } // Compliant - method is private
        private string GetDay() { return ""; }

        public string GetWhatever()
        {
            return "";
        }

        public string SomeWeirdCase // Noncompliant
        { get; }

        public string SOMEWEIRDCASE() // Secondary
        {
            return "";
        }

        public int GetMyProperty() => 42;
    }

    // https://sonarsource.atlassian.net/browse/NET-543
    public partial class PartialProperties
    {
        public partial int Foo { get; }       // Noncompliant {{Change either the name of property 'Foo' or the name of method 'GetFoo' to make them distinguishable.}}

        public void GetFoo() { }              // Secondary
    }

    public partial class PartialProperties
    {
        public partial int Foo { get => 42; } // Noncompliant {{Change either the name of property 'Foo' or the name of method 'GetFoo' to make them distinguishable.}}
    }
}

public partial class PartialCrossFile
{
    public partial int Foo { get; } // Noncompliant {{Change either the name of property 'Foo' or the name of method 'GetFoo' to make them distinguishable.}}
}
