class PrimaryConstructor(string ctorParam = "Password=123") // Noncompliant
{
    public void Test(string methodParam = "Password=123") // Noncompliant
    {
        var lambda = (string lambdaParam = "Password=123") => lambdaParam; // FN
    }
}
