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
            //                                                                             ^^^^ Secondary {{This function trusts all certificates.}}
        }
    }

    class StaticLocalFunctionCase
    {
        public void Foo()
        {
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  // Secondary

            static HttpWebRequest CreateRQ() => (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");

            static void InitAsArgument(RemoteCertificateValidationCallback callback) =>
                CreateRQ().ServerCertificateValidationCallback += callback; // Noncompliant
        }
    }
}
