using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class LoginExampleController
{
    const string KEY = "KEY";

    public void SymmetricSecurityKey_UnsecureKey(IConfiguration config, bool condition)
    {
        var key = config["Jwt:Key"] ?? "";                                          // Unsecure configuration
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));                  // Noncompliant {{JWT secret keys should not be disclosed.}}
        //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("exploding whale"));    // Noncompliant
        _ = new SymmetricSecurityKey(new byte[] { (byte)1, (byte)'a' });            // Noncompliant

        byte[] bs;

        bs = condition ? Convert.FromBase64String(Environment.GetEnvironmentVariable(KEY)) : Convert.FromBase64String(config["Jwt:Key"]);
        _ = new SymmetricSecurityKey(bs);                                           // Noncompliant - false-branch is unsecure

        bs = null;
        if (condition)
        {
            bs = Convert.FromBase64String(KEY);
        }
        else
        {
            bs = Convert.FromBase64String(Environment.GetEnvironmentVariable(KEY));
        }
        _ = new SymmetricSecurityKey(bs);                                           // Noncompliant - true-branch is unsecure
    }

    public void SymmetricSecurityKey_Compliant(byte b)
    {
        var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? "";              // Environment variable is considered secure
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));                  // Compliant
        _ = new SymmetricSecurityKey(UnknownProcessing("key"));                     // Compliant
        _ = new SymmetricSecurityKey(new byte[] { (byte)1, b });                    // Compliant
    }

    private static byte[] UnknownProcessing(string key) =>
        Encoding.UTF8.GetBytes(key);
}
