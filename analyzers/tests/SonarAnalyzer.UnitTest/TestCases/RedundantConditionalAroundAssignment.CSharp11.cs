public class SomeClass
{
    public void Noncompliant(byte[] bytes)
    {
        if (bytes is [not 1, .., not 3])
        {
            bytes[0] = 1; // FN
            bytes[^1] = 3; // FN
        }
    }
}
