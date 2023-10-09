static class A
{
    public const int I = 1;
}

class PrimaryConstructor(int a1 = A.I) { }

class SubClass(int b1) : PrimaryConstructor(1)
{
    private int Field = b1;
    private int Property { get; set; } = b1;
    private int b1 = b1;

    int Method() => b1;
}

class B(int b1)
{
    private int Field = b1;
    private int Property { get; set; } = b1;
    public B(int b1, int b2) : this(b1)
    {
        var f = (int i = A.I) => i;
    }
    int Method() => b1;
}
