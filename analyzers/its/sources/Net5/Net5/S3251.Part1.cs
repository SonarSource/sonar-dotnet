namespace Net5
{
    public partial record Record3
    {
        partial void Method(); //Noncompliant {{Supply an implementation for this partial method.}}
        partial void Method2();
        public partial void M7(); // Compliant
        public partial void M10(); // Compliant
        public partial int M11(); // Compliant
        public partial void M12(out string someParam); // Compliant
    }
}
