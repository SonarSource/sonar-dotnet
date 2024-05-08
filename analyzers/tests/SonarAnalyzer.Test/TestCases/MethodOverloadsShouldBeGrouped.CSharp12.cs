class SomeClass(int arg)
{
    public void RandomMethod1() { }         // Noncompliant

    public SomeClass() : this(5) { }

    public void RandomMethod1(int i) { }    // Secondary

    public void RandomMethod2() { }
}
