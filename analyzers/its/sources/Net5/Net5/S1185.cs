namespace Net5
{
    public class S1185
    {
        record Base
        {
            public virtual int MyProperty1 { get; init; }
        }

        record Derived : Base
        {
            public override int MyProperty1 { get => base.MyProperty1; init => base.MyProperty1 = value; }
        }
    }
}
