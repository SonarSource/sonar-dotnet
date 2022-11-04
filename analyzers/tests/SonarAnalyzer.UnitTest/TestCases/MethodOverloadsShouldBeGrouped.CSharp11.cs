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
