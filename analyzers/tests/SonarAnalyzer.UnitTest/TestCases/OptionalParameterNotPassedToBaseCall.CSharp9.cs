record Base
{
    public virtual void MyMethod(int j, int i = 1) { }
    public virtual void MyMethod2(int i = 1) { }
}

record Derived : Base
{
    public override void MyMethod(int j, int i = 1)
    {
        base.MyMethod(1); // Noncompliant {{Pass the missing user-supplied parameter value to this 'base' call.}}
//      ^^^^^^^^^^^^^^^^
        base.MyMethod(j); // Noncompliant {{Pass the missing user-supplied parameter value to this 'base' call.}}
        base.MyMethod2();
        this.MyMethod(1);
    }
}
