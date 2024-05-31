using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace IntentionalFindings;

public class S6781
{
    private const string HardCodedKey = "SecretSecretSecretSecretSecretSecretSecretSecret";

    public void TestCases()
    {
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(HardCodedKey)); // Noncompliant
    }
}
