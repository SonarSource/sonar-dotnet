record PropertyWriteOnly
{
    public int Foo1  // Compliant - FN
    {
        init { }
    }

    public int Foo2 { set { } } //Noncompliant {{Provide a getter for 'Foo2' or replace the property with a 'SetFoo2' method.}}

    public int Foo3 { get { return 1; } set { } }
}
