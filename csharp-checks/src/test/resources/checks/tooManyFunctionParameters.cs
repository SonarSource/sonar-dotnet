class C
{
    delegate void Foo(int p1, int p2, int p3);                        // Noncompliant

    public void F1(int p1, int p2, int p3, int p4, int p5, int p6, int p7, params int[] p8)   // Noncompliant
    {
      Foo f = delegate(int p1, int p2, int p3) { Console.WriteLine(p1 + p2 + p3); };
      Foo f = (int p1, int p2, int p3) => Console.WriteLine(p1 + p2 + p3);
      Foo f = (p1, p2, p3) => Console.WriteLine(p1 + p2 + p3);
    }

    public void F2(int p1, int p2, int p3, int p4)                    // Compliant
    {
    }

    public void F3(params int[] p1)
    {
    }

    public void F4()
    {
    }
}

interface I
{
    void F(int p1, int p2, int p3, int p4);
}
