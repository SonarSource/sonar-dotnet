
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
            //False Negative - not implemented case
            ServicePointManager.ServerCertificateValidationCallback += FindValidator(false);    //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += FindValidator();         //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += LambdaValidator();       //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += Compliant(true);     
        }

        static RemoteCertificateValidationCallback FindValidator()
        {
            return NoncompilantValidation;                                      //Secondary
        }

        static bool NoncompilantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;                                                        //Secondary
                                                                                //Secondary@-1
        }

        static RemoteCertificateValidationCallback LambdaValidator()
        {
            return (sender, certificate, chain, SslPolicyErrors) => true;        //Secondary
        }

        static RemoteCertificateValidationCallback FindValidator(bool useDelegate)
        {
            if (useDelegate)
            {
                return NoncompilantValidation;                                  //Secondary
            }
            else
            {
                return (sender, certificate, chain, SslPolicyErrors) => true;   //Secondary
            }
        }


        static RemoteCertificateValidationCallback Compliant(bool Compliant)
        {
            if (Compliant)
            {
                return null;
            }
            else
            {
                return (sender, certificate, chain, SslPolicyErrors) => true;
            }
        }


    }
}

