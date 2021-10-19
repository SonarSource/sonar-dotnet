record struct R1
{
    void a() { } // FN

    void a(int a, char b) { }

    record B { }

    void a(string a) { } // FN
}

record struct R2
{
    void a() { }

    void a(int a, char b) { }

    void a(string a) { }

    record B { }
}

record struct R3
{
    public R3() { } // FN

    public R3(int a) { }

    public R3(char a) { }

    void a() { }

    public R3(double a) { } // FN
}

record struct PositionalR1(string Parameter)
{
    void a() { } // FN

    void a(int a, char b) { }

    record B { }

    void a(string a) { } // FN
}

record struct PositionalR2(string Parameter)
{
    void a() { }

    void a(int a, char b) { }

    void a(string a) { }

    record B { }
}

record struct PositionalR3(string Parameter)
{
    public PositionalR3(): this("a") { } // FN

    public PositionalR3(int a) : this("a") { }

    public PositionalR3(char a) : this("a") { }

    void a() { }

    public PositionalR3(double a) : this("a") { } // FN
}
