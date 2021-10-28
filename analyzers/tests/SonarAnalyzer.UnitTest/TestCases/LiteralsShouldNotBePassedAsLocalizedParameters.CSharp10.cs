public struct S
{
    public string Property { get; set; }

    public void M()
    {
        (Property, var b) = ("a", "B"); // FN
    }
}
