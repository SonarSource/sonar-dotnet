public partial record Record
{
    partial void Method(); //Noncompliant {{Supply an implementation for this partial method.}}
    public partial void M7(); // Compliant
    public partial void M10(); // Compliant
    public partial int M11(); // Compliant
    public partial void M12(out string someParam); // Compliant
}

public partial record Record
{
    public partial void M7()
    {

    }

    public partial void M10()
    {

    }

    public partial int M11()
    {
        return 42;
    }

    public partial void M12(out string someParam)
    {
        someParam = "";
    }
}

public partial record Record2
{
    partial void Method(); //Noncompliant {{Supply an implementation for this partial method.}}

    public void SomeMethod()
    {
        var record2 = new Record2();

        record2.Method(); // Noncompliant
    }
}

public partial record Record3
{
    partial void Method(); //Noncompliant {{Supply an implementation for this partial method.}}
    partial void Method2();
    public partial void M7(); // Compliant
    public partial void M10(); // Compliant
    public partial int M11(); // Compliant
    public partial void M12(out string someParam); // Compliant
}
