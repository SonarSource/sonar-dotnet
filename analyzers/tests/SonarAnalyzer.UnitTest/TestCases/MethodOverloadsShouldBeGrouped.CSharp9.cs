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
