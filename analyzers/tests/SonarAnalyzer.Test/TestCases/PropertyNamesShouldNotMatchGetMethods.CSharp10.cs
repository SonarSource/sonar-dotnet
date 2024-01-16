using System;

namespace Tests.Diagnostics
{
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
}
