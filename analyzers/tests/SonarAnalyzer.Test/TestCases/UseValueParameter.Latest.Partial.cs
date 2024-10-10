namespace Tests.Diagnostics
{
    public partial class PartialProperty
    {
        public partial int Property1 { get => 42; set { count = 42; } } // Noncompliant
        public partial int Property2 { get => 42; init { count = 42; } } // Noncompliant
        private partial int this[int index] { set { count = 42; } } // Noncompliant
    }
}
