record Base
{
    public virtual int MyProperty1 { get; init; }
    public virtual int MyProperty2 { get; init; }
    public virtual int MyProperty3 { get; init; }
    public virtual int MyProperty4 { get; init; }
    public virtual int MyProperty5 { get; init; }
    public virtual int MyProperty6 { init; } // Error [CS8051]
    public virtual int MyProperty7 { get; }
    public virtual int MyProperty8 { get; init; }
    public virtual int MyProperty9 { get; init; }
    public virtual int MyProperty10 { get; init; }
    public virtual int MyProperty11 { get; init; }
}

record Derived : Base
{
    bool isInitialized;
    int backingField;
    public override int MyProperty2 { get => base.MyProperty2; init => base.MyProperty2 = value + 1; }
    public override int MyProperty3 { get => base.MyProperty3; init { base.MyProperty2 = value; isInitialized = true; } }
    /// <summary>
    ///  This is some documentation which should disble the issue on this property.
    /// </summary>
    public override int MyProperty4 { get => base.MyProperty1; init => base.MyProperty1 = value; }
    public override sealed int MyProperty5 { get => base.MyProperty5; init => base.MyProperty5 = value; }
    public override int MyProperty6 { get => base.MyProperty6; init => base.MyProperty6 = value; } // Error [CS0154, CS0154]
    public override int MyProperty7 { get => base.MyProperty7; init => base.MyProperty7 = value; } // Error [CS0200, CS0200]
    public sealed int MyProperty10 { get => base.MyProperty10; init => base.MyProperty10 = value; } // Error [CS0238]
    public override int MyProperty11 { get => base.MyProperty11; init => backingField = value; }
}
