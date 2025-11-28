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


record struct RS1
{
    void a() { } // Noncompliant {{All 'a' method overloads should be adjacent.}}
    //   ^

    void a(int a, char b) { }

    record B { }

    void a(string a) { } // Secondary {{Non-adjacent overload}}
    //   ^
}

record struct RS2
{
    void a() { }

    void a(int a, char b) { }

    void a(string a) { }

    record B { }
}

record struct RS3
{
    public RS3() { } // Noncompliant

    public RS3(int a) { }

    public RS3(char a) { }

    void a() { }

    public RS3(double a) { } // Secondary
}

record struct PositionalRS1(string Parameter)
{
    void a() { } // Noncompliant

    void a(int a, char b) { }

    record B { }

    void a(string a) { } // Secondary
}

record struct PositionalRS2(string Parameter)
{
    void a() { }

    void a(int a, char b) { }

    void a(string a) { }

    record B { }
}

record struct PositionalRS3(string Parameter)
{
    public PositionalRS3() : this("a") { } // Noncompliant

    public PositionalRS3(int a) : this("a") { }

    public PositionalRS3(char a) : this("a") { }

    void a() { }

    public PositionalRS3(double a) : this("a") { } // Secondary
}

interface A
{
    static abstract void DoSomething(double b);

    static abstract void DoSomething();

    static abstract void DoSomething(int a);
}

interface B
{
    static abstract void DoSomething(double b); // Noncompliant {{All 'DoSomething' method overloads should be adjacent.}}
//                       ^^^^^^^^^^^

    static abstract void DoSomething();

    static abstract void DoSomethingElse();

    static abstract void DoSomething(int a); // Secondary {{Non-adjacent overload}}
//                       ^^^^^^^^^^^

    static virtual void DoSomething(double a, double b) { }
}

class SomeClass : A, B
{
    public static void DoSomething(double a) { } // Noncompliant

    public void SeparateFromSameInterfaceA() { }

    // Compliant - we dont not raise issues for explicit interface implementation as it is a corner case and it can make sense to group implementation by interface
    static void A.DoSomething() { }

    static public void DoSomethingElse() { }

    static void B.DoSomething() { } // Compliant - explicit interface implementation

    public void SeparateFromSameInterfaceB() { }

    public static void DoSomething(int a) { } // Secondary
}

class PrimaryConstructor(int arg)
{
    public void RandomMethod1() { }         // Noncompliant

    public PrimaryConstructor() : this(5) { }

    public void RandomMethod1(int i) { }    // Secondary

    public void RandomMethod2() { }
}

public static class Extensions
{
    public class Sample { }
    extension (Sample sample)
    {
        void a() { }            // FN - https://sonarsource.atlassian.net/browse/NET-2719
        void a(int a, char b) { }
        void b() { }
        void a(string a) { }    // FN - https://sonarsource.atlassian.net/browse/NET-2719
    }
}
