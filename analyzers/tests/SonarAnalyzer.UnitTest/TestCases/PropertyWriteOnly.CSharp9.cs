record PropertyWriteOnly
{
    public int Foo1  // Compliant - FN
    {
        init { }
    }

    public int Foo2 { set { } } //Noncompliant {{Provide a getter for 'Foo2' or replace the property with a 'SetFoo2' method.}}

    public int Foo3 { get { return 1; } set { } }
}

record PropertyWriteOnlyInPositionalRecord()
{
    public int Foo1  // Compliant - FN
    {
        init { }
    }

    public int Foo2 { set { } } //Noncompliant {{Provide a getter for 'Foo2' or replace the property with a 'SetFoo2' method.}}

    public int Foo3 { get { return 1; } set { } }
}

namespace ReproIssue2390
{
    public record A
    {
        protected int m = 5;
        public virtual int M
        {
            get { return m; }
            set { m = value; }
        }
    }
    public record B : A
    {
        public override int M // Compliant, getter is in base class
        {
            set { m = value + 1; }
        }
    }
}
