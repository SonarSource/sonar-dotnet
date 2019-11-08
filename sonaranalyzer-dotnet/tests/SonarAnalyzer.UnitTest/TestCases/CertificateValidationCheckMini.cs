
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
            RemoteCertificateValidationCallback Callback = null;

            if (true)
            {
                Callback = NoncompilantValidationAsArgument;                //Secondary
            }

            InitAsArgument(Callback);                                       //Secondary

            InitAsArgument(NoncompilantValidationAsArgument);                       // Secondary / extra secondary to link them
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => false);
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  // Secondary
                                                                                    
        }

        bool NoncompilantValidationAsArgument(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;                                                            // Secondary
        }

        void InitAsArgument(RemoteCertificateValidationCallback Callback)
        {
            ServicePointManager.ServerCertificateValidationCallback += Callback;    //Noncompliant
        }


    }
}
