using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

// See https://github.com/SonarSource/sonar-dotnet/issues/4415
public partial class PartialClass
{
    public static partial RemoteCertificateValidationCallback FindInvalid()
    {
        return (sender, certificate, chain, SslPolicyErrors) => true;
        //                                                      ^^^^ Secondary {{This function trusts all certificates.}}
    }
}

// Cross-file static method callback test case
public static class CertValidatorCrossFile
{
    public static RemoteCertificateValidationCallback GetNoncompliantCallback()
    {
        return InvalidValidation;  // Secondary [flow-crossfile] {{This function trusts all certificates.}}
    }

    public static RemoteCertificateValidationCallback GetCompliantCallback()
    {
        return CompliantValidation;
    }

    private static bool InvalidValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;  // Secondary [flow-crossfile] {{This function trusts all certificates.}}
    }

    private static bool CompliantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return sslPolicyErrors == SslPolicyErrors.None;
    }
}
