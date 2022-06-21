using System.ComponentModel;

public struct S
{
    [Localizable(true)]
    public string Property { get; set; }

    public void M()
    {
        (Property, var b) = ("a", "B");  // Noncompliant
        //                   ^^^

        (this.Property, b) = ("a", "B"); // Noncompliant

        var s = new S();
        (s.Property, b) = ("a", "B");    // Noncompliant
    }
}
