
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Tests.Diagnostics
{

    class CertificateValidationChecks   //FIXME: Vyhodit
    {

        HttpWebRequest CreateRQ()
        {
            return (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");
        }


        void Mini()
        {

        }


        void DelegateReturnedByFunction()
        {
            CreateRQ().ServerCertificateValidationCallback += FindNoncompliant(false);      //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += FindNoncompliant();           //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += FindLambdaValidator();        //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += FindCompliant(true);
            CreateRQ().ServerCertificateValidationCallback += FindCompliantRecursive(3);
            CreateRQ().ServerCertificateValidationCallback += FindNoncompliantRecursive(3); //Noncompliant
        }



        #region Find Validators


        static RemoteCertificateValidationCallback FindNoncompliant()
        {
            return NoncompilantValidation;                                      //Secondary
        }

        static RemoteCertificateValidationCallback FindLambdaValidator()
        {
            return (sender, certificate, chain, SslPolicyErrors) => true;        //Secondary
        }

        static RemoteCertificateValidationCallback FindNoncompliant(bool useDelegate)   //All paths return noncompliant
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


        static RemoteCertificateValidationCallback FindCompliant(bool Compliant)    //At least one path returns compliant => there is a logic and it is considered compliant
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

        static RemoteCertificateValidationCallback FindCompliantRecursive(int Index)
        {
            if (Index <= 0)
            {
                return CompilantValidation;
            }
            else
            {
                return FindCompliantRecursive(Index - 1);
            }
        }

        static RemoteCertificateValidationCallback FindNoncompliantRecursive(int Index)
        {
            if (Index <= 0)
            {
                return NoncompilantValidation;                                  //Secondary
            }
            else
            {
                return FindNoncompliantRecursive(Index - 1);
            }
        }

        #endregion


        static bool NoncompilantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;    //Secondary
                            //Secondary@-1

        }

        static bool CompilantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return false; //Compliant
        }

    }
}

