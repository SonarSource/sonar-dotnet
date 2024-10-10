record PropertyWriteOnly
{
    public int Foo1  // Noncompliant {{Provide a getter for 'Foo1' or replace the property with a 'SetFoo1' method.}}
    {
        init { }
    }

    public int Foo2 { set { } } // Noncompliant {{Provide a getter for 'Foo2' or replace the property with a 'SetFoo2' method.}}

    public int Foo3 { get { return 1; } set { } }
}

record PropertyWriteOnlyInPositionalRecord(int Parameter)
{
    private int foo1 = 5;

    public int Foo1  // Noncompliant
    {
        init { foo1 = value; }
    }

    public int Foo2 { set { } } // Noncompliant {{Provide a getter for 'Foo2' or replace the property with a 'SetFoo2' method.}}

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
            init { m = value; }
        }
    }
    public record B : A
    {
        public override int M // Compliant, getter is in base class
        {
            init { m = value + 1; }
        }
    }
}

public interface IPropertyWriteOnly
{
    public static virtual int Foo  // Noncompliant {{Provide a getter for 'Foo' or replace the property with a 'SetFoo' method.}}
    {
        set
        {
            // ... some code ...
        }
    }
}

namespace CSharp13
{
    public partial class PartialProperties
    {
        public partial int Foo  // Noncompliant {{Provide a getter for 'Foo' or replace the property with a 'SetFoo' method.}}
//                         ^^^
        {
            set
            {
                // ... some code ...
            }
        }
        public partial int Foo2
        {
            get
            {
                return 1;
            }
            set
            {
                // ... some code ...
            }
        }
    }
}
