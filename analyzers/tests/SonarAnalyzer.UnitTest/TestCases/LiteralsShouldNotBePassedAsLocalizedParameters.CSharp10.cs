using System.ComponentModel;

public struct S
{
    [Localizable(true)]
    public string Property { get; set; }

    public void M()
    {
        (Property, var b) = ("a", "B"); // FN
    }
}
