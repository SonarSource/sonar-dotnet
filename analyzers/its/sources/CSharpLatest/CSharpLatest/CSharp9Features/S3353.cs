namespace CSharpLatest.CSharp9Features;

public class S3353
{
    public void Test(nuint val)
    {
        nint foo = 42; // Noncompliant

        nuint bar = 31; // Noncompliant
        if (bar == val)
        {
        }
    }
}
