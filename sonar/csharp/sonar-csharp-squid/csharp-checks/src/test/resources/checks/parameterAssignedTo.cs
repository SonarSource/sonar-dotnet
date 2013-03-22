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

class Foo
{
    public delegate void FooEventHandler(object sender);

    public event FooEventHandler OnFoo;
}

class Bar
{
    void test(int param)
    {
        Foo a = new Foo();
        a.OnFoo += delegate(out int b, int c, ref int d)
        {
            b = 0;  // Compliant
            c = 0;  // Non-Compliant
            d = 0;  // Compliant
        };

        param = 42; // Non-Compliant
        int b;      // Does not compile
        b = 10;     // Compliant

        a.OnFoo += (out int foo1, int foo2, ref int foo3) => {
            foo1 = 0; // Compliant
            foo2 = 0; // Non-Compliant
        };

        test2((int x) => x = 0); // Non-Compliant
        test2((x) => x = 0);     // Non-Compliant
        test2(                   // Compliant
            (x) =>
            {
                return 0;
            });
        test2(
            (x) =>
            {
                x = 0;            // Non-Compliant
                return 0;
            });
        test2(                    // Compliant
            (int x) =>
            {
                return 0;
            });
        test2(
            (int x) =>
            {
                x = 0;            // Non-Compliant
                return 0;
            });
    }

    void test2(Func<int, int> foo)
    {
    }
}

class Baz
{
    public delegate void BazEventHandler();

    public event BazEventHandler OnBaz;

    public void baz()
    {
        OnBaz += delegate { };
    }
}
