using System;
using System.Configuration;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class LoginExampleController
{
    private const string HardCodedKey = "SecretSecretSecretSecretSecretSecretSecretSecret";
    private const string HardCodedKeyBase64 = "U2VjcmV0U2VjcmV0U2VjcmV0U2VjcmV0U2VjcmV0U2VjcmV0U2VjcmV0U2VjcmV0";
    private static readonly byte[] HardCodedKeyBytes = Encoding.UTF8.GetBytes(HardCodedKey);

    public void SymmetricSecurityKey_UnsecureKey()
    {
        var key = ConfigurationManager.AppSettings["key"] ?? throw new InvalidOperationException("JWT key is not configured."); // Unsecure configuration
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));                                                              // Noncompliant {{JWT secret keys should not be disclosed.}}
        //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(HardCodedKey));                                                     // Noncompliant
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Constants.ConstantKey));                                            // Noncompliant
        _ = new SymmetricSecurityKey(Convert.FromBase64String(HardCodedKeyBase64));                                             // Noncompliant
        _ = new SymmetricSecurityKey(null);                                                                                     // Noncompliant
        SymmetricSecurityKey x = new(null);                                                                                     // Noncompliant
    }

    public void SymmetricSecurityKey_Compliant(string parameterKey, byte[] parameterKeyBytes)
    {
        var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT key is not configured.");
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(parameterKey));
        _ = new SymmetricSecurityKey(Convert.FromBase64String(key));
        _ = new SymmetricSecurityKey(Convert.FromBase64String(parameterKey));
        _ = new SymmetricSecurityKey(parameterKeyBytes);
        _ = new SymmetricSecurityKey(HardCodedKeyBytes);                                                                        // Compliant FN, GetBytes is called outside the method
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Constants.PropertyKey));                                            // Static property, cannot be evaluated to constant
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(string.Empty));                                                     // Field, cannot be evaluated to constant
        _ = new SymmetricSecurityKey(UnknownProcessing(key));                                                                   // Unknown processing, cannot be evaluated to constant
        _ = new SymmetricSecurityKey(Array.Empty<byte>());                                                                      // Unknown method, it's not evaluated to constant
    }

    public void SymmetricSecurityKey_ControlFlow()
    {
        var key = ConfigurationManager.AppSettings["key"] ?? throw new InvalidOperationException("JWT key is not configured.");
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));                                                                  // Noncompliant

        key = HardCodedKey;
        var a = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));                                                              // Noncompliant
        var b = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));                                                              // Noncompliant

        if (DateTime.Now.Ticks % 2 == 0)
        {
            key = Environment.GetEnvironmentVariable("JWT_KEY");
            _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));                                                              // Compliant
        }
        else
        {
            key = HardCodedKey;
        }
        _ = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));                                                                  // Noncompliant

        var keyBytes = Encoding.UTF8.GetBytes(HardCodedKey);
        keyBytes[0] = 0;
        _ = new SymmetricSecurityKey(keyBytes);                                                                                     // Noncompliant

        for (var i = 0; i < keyBytes.Length; i++)
        {
            keyBytes[i] = 0;
        }
        _ = new SymmetricSecurityKey(keyBytes);                                                                                     // Noncompliant

        keyBytes.Initialize();
        _ = new SymmetricSecurityKey(keyBytes);                                                                                     // Noncompliant
    }

    private static byte[] UnknownProcessing(string key) =>
        Encoding.UTF8.GetBytes(key);

    private static class Constants
    {
        public const string ConstantKey = "Secret";
        public static string PropertyKey => "SecretSecretSecretSecretSecretSecretSecretSecret";
    }
}
