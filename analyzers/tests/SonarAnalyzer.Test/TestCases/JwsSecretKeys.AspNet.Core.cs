using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class LoginExampleController
{
    public void SymmetricSecurityKey_UnsecureKey(IConfiguration config)
    {
        var key = config["Jwt:Key"] ?? "";                                  // Unsecure configuration
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));          // Noncompliant
        _ = new SymmetricSecurityKey(UnknownProcessing(key));               // Compliant
    }

    public void SymmetricSecurityKey_Compliant()
    {
        var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? "";      // Environment variable is considered secure
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    private static byte[] UnknownProcessing(string key) =>
        Encoding.UTF8.GetBytes(key);
}
