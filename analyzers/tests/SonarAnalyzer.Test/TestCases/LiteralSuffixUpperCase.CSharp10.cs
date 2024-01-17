public struct S
{
    public long Property { get; set; }

    public void M()
    {
        (Property, var b) = (0l,  // Noncompliant
                             0l); // Noncompliant
    }
}
