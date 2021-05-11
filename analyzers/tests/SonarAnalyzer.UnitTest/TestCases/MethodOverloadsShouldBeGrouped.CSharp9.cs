record R1
{
    void a() { } // Noncompliant {{All 'a' method overloads should be adjacent.}}

    void a(int a, char b) { }

    record B { }

    void a(string a) { } // Secondary {{Non-adjacent overload}}
}

record R2
{
    void a() { }

    void a(int a, char b) { }

    void a(string a) { }

    record B { }
}

record R3
{
    R3() { } // Noncompliant {{All 'R3' method overloads should be adjacent.}}

    R3(int a) { }

    R3(char a) { }

    ~R3() { }

    R3(double a) { } // Secondary {{Non-adjacent overload}}
}

record PositionalR1(string Parameter)
{
    void a() { } // Noncompliant {{All 'a' method overloads should be adjacent.}}

    void a(int a, char b) { }

    record B { }

    void a(string a) { } // Secondary {{Non-adjacent overload}}
}

record PositionalR2(string Parameter)
{
    void a() { }

    void a(int a, char b) { }

    void a(string a) { }

    record B { }
}

record PositionalR3(string Parameter)
{
    PositionalR3(): this("a") { } // Noncompliant {{All 'PositionalR3' method overloads should be adjacent.}}

    PositionalR3(int a) : this("a") { }

    PositionalR3(char a) : this("a") { }

    ~PositionalR3() { }

    PositionalR3(double a) : this("a") { } // Secondary {{Non-adjacent overload}}
}
