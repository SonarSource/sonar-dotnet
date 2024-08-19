public class SomeClass
{
    public void Noncompliant(byte[] bytes)
    {
        if (bytes is [not 1, .., not 3]) // FN (is expression not supported yet)
        {
            bytes[0] = 1;
            bytes[^1] = 3;
        }
    }
}
