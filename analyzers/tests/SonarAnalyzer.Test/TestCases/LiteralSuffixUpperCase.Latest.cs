public struct S
{
    public long Property { get; set; }

    public void M()
    {
        // Error @+1 [CS0078] - compiler warning "The 'l' suffix is easily confused with the digit '1' -- use 'L' for clarity"
        (Property, var b) = (0l,  // Noncompliant
                             // Error @+1 [CS0078] - compiler warning "The 'l' suffix is easily confused with the digit '1' -- use 'L' for clarity"
                             0l); // Noncompliant
    }

    void Utf8StringLiterals()
    {
        var a = "test"u8; // Compliant
    }
}
