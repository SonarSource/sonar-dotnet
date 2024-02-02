public partial record struct RecordStruct
{
    partial void Method();          //Noncompliant {{Supply an implementation for this partial method.}}
    private partial void M2();      // Error [CS8795]
    internal partial void M3();     // Error [CS8795]
    public partial void M6();       // Error [CS8795]
    public partial void M7();       // Compliant
    public partial int M8();        // Error [CS8795]
    public partial void M9(out string someParam);   // Error [CS8795]
    public partial void M10();      // Compliant
    public partial int M11();       // Compliant
    public partial void M12(out string someParam);  // Compliant
}

public partial record struct RecordStruct
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

public partial record struct Record3
{
    partial void Method();          //Noncompliant {{Supply an implementation for this partial method.}}
    partial void Method2();
    private partial void M2();      // Error [CS8795]
    internal partial void M3();     // Error [CS8795]
    public partial void M6();       // Error [CS8795]
    public partial void M7();       // Compliant
    public partial int M8();        // Error [CS8795]
    public partial void M9(out string someParam);   // Error [CS8795]
    public partial void M10();      // Compliant
    public partial int M11();       // Compliant
    public partial void M12(out string someParam);  // Compliant
}
