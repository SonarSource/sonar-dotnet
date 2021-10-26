using System;

namespace Tests.Diagnostics
{
    public record struct RecordStruct
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

        public int GetMyProperty() => 42;
    }

    public record struct PositionalRecordStruct
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

        public int GetMyProperty() => 42;
    }
}
