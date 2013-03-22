class Foo
{
    void foo(int a)
    {
        a = 42;   // Non-Compliant
    }

    void foo(int a)
    {
        int tmp = a;
        tmp = 42; // Compliant
    }

    void foo(ref int a)
    {
        a = 42;   // Compliant
    }

    void foo(out int a)
    {
        a = 42;   // Compliant
    }

    void foo()
    {
    }

    void foo(int a, int b, int c, int d, int e)
    {
      b = 42;     // Non-Compliant
      e = 0;      // Non-Compliant
    }

    void foo(this int a)
    {
      a = 42;   // Non-Compliant
    }
}
