using Microsoft.AspNetCore.Identity;

namespace IntentionalFindings;

public class S5344
{
    public void TestCases()
    {
        _ = new PasswordHasherOptions
            {
                IterationCount = 1 // Noncompliant {{Use at least 100,000 iterations here.}}
            };
    }
}
