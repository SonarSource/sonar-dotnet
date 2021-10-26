using System;

namespace Tests.Diagnostics
{
    public record Base
    {
        public virtual int MyProperty { get; set; }
    }

    public record Record : Base
    {
        public int Foo // FN
        { get; set; }
        public int GetFoo()
        { return 1; }

        public DateTime Date { get; }
        public string GetDateAsString()
        {
            return Date.ToString();
        }

        public string Bar // FN
        { get; }
        public int Bar() // Error [CS0102]
        {
            return 42;
        }

        private string Color { get; } // Compliant - property is private
        public string GetColor() { return ""; }

        public string Day { get; } // Compliant - method is private
        private string GetDay() { return ""; }

        protected string Whatever // FN
        { get; }

        public string GetWhatever() // FN
        {
            return "";
        }

        public string SomeWeirdCase // FN
        { get; }

        public string SOMEWEIRDCASE() // FN
        {
            return "";
        }

        public override int MyProperty { get; set; } // Compliant - override
        public int GetMyProperty() => 42;
    }

    public record PositionalRecord : Base
    {
        public int Foo // FN
        { get; set; }
        public int GetFoo()
        { return 1; }

        public DateTime Date { get; }
        public string GetDateAsString()
        {
            return Date.ToString();
        }

        public string Bar // FN
        { get; }
        public int Bar() // Error [CS0102]
        {
            return 42;
        }

        private string Color { get; } // Compliant - property is private
        public string GetColor() { return ""; }

        public string Day { get; } // Compliant - method is private
        private string GetDay() { return ""; }

        protected string Whatever // FN
        { get; }

        public string GetWhatever() // FN
        {
            return "";
        }

        public string SomeWeirdCase // FN
        { get; }

        public string SOMEWEIRDCASE() // FN
        {
            return "";
        }

        public override int MyProperty { get; set; } // Compliant - override
        public int GetMyProperty() => 42;
    }
}
