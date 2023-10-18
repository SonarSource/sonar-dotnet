using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Tests.Diagnostics
{
    // See https://github.com/SonarSource/sonar-dotnet/issues/4415
    public partial class PartialClass
    {
        public static partial RemoteCertificateValidationCallback FindInvalid()
        {
            return (sender, certificate, chain, SslPolicyErrors) => true;
            //                                                      ^^^^ Secondary {{This function trusts all certificates.}}
        }
    }
}
