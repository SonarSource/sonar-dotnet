public class ExpectedIssuesNotRaised2
{
    public void Test(bool a, bool b) // Noncompliant [MyId0]
    {
        if (a == b) // Noncompliant
        { } // Secondary [MyId1]
    }
}
