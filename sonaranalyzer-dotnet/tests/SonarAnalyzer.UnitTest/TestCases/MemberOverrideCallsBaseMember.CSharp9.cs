class Base
{
    public virtual int MyProperty1 { get; init; }
    public virtual int MyProperty2 { get; init; }
    public virtual int MyProperty3 { get; init; }
}

class Derived : Base
{
    bool isInitialized;

    public override int MyProperty1 { get => base.MyProperty1; init => base.MyProperty1 = value; }      //Noncompliant
    public override int MyProperty2 { get => base.MyProperty2; init => base.MyProperty2 = value + 1; }  //Noncompliant, FP
    public override int MyProperty3 { get => base.MyProperty3; init { base.MyProperty2 = value; isInitialized = true; } } //Noncompliant, FP
}
