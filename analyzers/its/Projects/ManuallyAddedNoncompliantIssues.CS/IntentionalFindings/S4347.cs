using Org.BouncyCastle.Security;

namespace IntentionalFindings;

public class S4347
{
    void TestCases()
    {
        var sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant {{Set an unpredictable seed before generating random values.}}
    }
}
