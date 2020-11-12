
record R
{
    void a() { } // FN {{All 'a' method overloads should be adjacent.}}

    void a(int a, char b) { }

    record B { }

    void a(string a) { } // FN
}
