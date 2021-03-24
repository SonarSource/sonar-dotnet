public class FooBase
{
    public virtual void Method()
    {

    }
}

public class FooImpl: FooBase
{
    public override void Method() // Noncompliant
    {

    }
}
