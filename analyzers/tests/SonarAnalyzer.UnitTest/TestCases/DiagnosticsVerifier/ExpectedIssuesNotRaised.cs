public class ExpectedIssuesNotRaised
{
    public void Test(bool a, bool b) // Noncompliant [MyId0]
    {
        if (a == b) // Noncompliant
        { } // Secondary [MyId1]
    }
}
