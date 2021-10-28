public struct S
{
    public decimal Property { get; set; }

    public void M()
    {
        (Property, var _) = (3 / 2, // FN
                             3 / 2);
    }
}
