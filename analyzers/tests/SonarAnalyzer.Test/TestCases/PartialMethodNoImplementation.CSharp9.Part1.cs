public partial record Record
{
    partial void Method(); //Noncompliant {{Supply an implementation for this partial method.}}
    private partial void M2();      // Error [CS8795]
    internal partial void M3();     // Error [CS8795]
    protected partial void M4();    // Error [CS8795]
    protected internal partial void M5();           // Error [CS8795]
    public partial void M6();       // Error [CS8795]
    public partial void M7();       // Compliant
    public partial int M8();        // Error [CS8795]
    public partial void M9(out string someParam);   // Error [CS8795]
    public partial void M10();      // Compliant
    public partial int M11();       // Compliant
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
    partial void Method();  //Noncompliant {{Supply an implementation for this partial method.}}
    private partial void M2();              // Error [CS8795]
    internal partial void M3();             // Error [CS8795]
    protected partial void M4();            // Error [CS8795]
    protected internal partial void M5();   // Error [CS8795]
    public partial void M6();               // Error [CS8795]

    public void SomeMethod()
    {
        var record2 = new Record2();

        record2.Method(); // Noncompliant
        record2.M2();
        record2.M3();
        record2.M4();
        record2.M5();
        record2.M6();
    }
}

public partial record Record3
{
    partial void Method(); //Noncompliant {{Supply an implementation for this partial method.}}
    partial void Method2();
    private partial void M2();      // Error [CS8795]
    internal partial void M3();     // Error [CS8795]
    protected partial void M4();    // Error [CS8795]
    protected internal partial void M5();   // Error [CS8795]
    public partial void M6();       // Error [CS8795]
    public partial void M7();       // Compliant
    public partial int M8();        // Error [CS8795]
    public partial void M9(out string someParam);   // Error [CS8795]
    public partial void M10();      // Compliant
    public partial int M11();       // Compliant
    public partial void M12(out string someParam);  // Compliant
}
