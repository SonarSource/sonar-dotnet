public partial record struct Record3
{
    partial void Method3(); // Noncompliant

    partial void Method2()
    {

    }

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
