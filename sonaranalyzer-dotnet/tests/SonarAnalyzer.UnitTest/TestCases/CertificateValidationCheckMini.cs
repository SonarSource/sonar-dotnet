
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Tests.Diagnostics
{
    class CertificateValidationChecks   //FIXME: Vyhodit
    {


        void Mini()
        {
            var rq = (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");

            rq.ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => true;    //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += NoncompilantValidation;                          //Noncompliant
        }

        bool NoncompilantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //Noncompliant {{Enable server certificate validation on this SSL/TLS connection}}
        }

    }
}
