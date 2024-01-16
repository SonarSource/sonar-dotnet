int LocalMethod(B b) // FN
{
    return b.Property;
}

public record A
{
    public A(A a) { }

    public void Method() { }

    public int Property { get; init; }
}

public record B : A
{
    public B(B b) : base(b) // FN - only base class property is copied
    {
        Property = b.Property;
    }

    public int Test_01(B b) // Noncompliant
    {
        return b.Property;
    }

    public void Test_02(B foo) // Noncompliant {{Consider using more general type 'A' instead of 'B'.}}
//                        ^^^
    {
        foo.Method();
    }
}
