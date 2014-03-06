class C
{
    private Foo foo = Foo.a;

    void f()
    {
        int a;                // Noncompliant
        int b;                // Compliant
        Foo foo = new Foo();  // Compliant

        this.a;
        foo.a = b;
        doSomething(foo.a);

        Bar bar = delegate (int x) {
          int y;              // Noncompliant
          return x % 2;
        };

        bar.call((int x) => {int i; return x % 2;});  // Noncompliant
    }

}
