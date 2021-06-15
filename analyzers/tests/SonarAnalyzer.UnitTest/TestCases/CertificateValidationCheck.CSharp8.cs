using System.Net;
using System.Net.Security;

namespace TestCases
{
    interface ICertValidation
    {
        HttpWebRequest CreateRQ()
        {
            return (HttpWebRequest) WebRequest.Create("http://localhost");
        }

        public void InitAsArgument(RemoteCertificateValidationCallback callback)
        {
            CreateRQ().ServerCertificateValidationCallback += callback; // Noncompliant
        }

        static void Execute(ICertValidation certValidation)
        {
            certValidation.InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);
            //                                                                             ^^^^ Secondary
        }
    }
}
