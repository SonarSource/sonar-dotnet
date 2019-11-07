
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Tests.Diagnostics
{
    class CertificateValidationChecks
    {
        
        void Main()
        {
            //Inline version
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => true;   //Noncompliant  {{Enable server certificate validation on this SSL/TLS connection}}
//                                                                                                             ^^^^
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => false;
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => certificate.Subject == "Test";

            //Without variable
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => true;   //Noncompliant  

            //Multiple handlers
            ServicePointManager.ServerCertificateValidationCallback += FalseValidation;
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationPositiveA;
            ServicePointManager.ServerCertificateValidationCallback += NoncompilantValidation;
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationPositiveB;
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationNegative;
            ServicePointManager.ServerCertificateValidationCallback += AdvNoncompilantTry;
            ServicePointManager.ServerCertificateValidationCallback += AdvNoncompilantWithTryObstacles;
            ServicePointManager.ServerCertificateValidationCallback += AdvCompilantWithTryObstacles;
            ServicePointManager.ServerCertificateValidationCallback += AdvNoncompilantWithObstacles;
            ServicePointManager.ServerCertificateValidationCallback += AdvCompilantWithObstacles;

            //Passed as argument
            InitAsArgument(NoncompilantValidationAsArgument);
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => false);
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Noncompliant  {{Enable server certificate validation on this SSL/TLS connection}}
//                                                                          ^^^^

            //Other occurances
            var httpHandler = new System.Net.Http.HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback += NoncompilantValidation;

            var rq = CreateRQ();
            rq.ServerCertificateValidationCallback += NoncompilantValidation;

            //Do not test this one. It's .NET Standard 2.1 target only. It shoudl work since we're hunting RemoteCertificateValidationCallback and method signature
            //var ws = new System.Net.WebSockets.ClientWebSocket();
            //ws.Options.RemoteCertificateValidationCallback += NoncompilantValidation;

            //Do not test this one. It's .NET Standard 2.1 target only. It shoudl work since we're hunting RemoteCertificateValidationCallback and method signature
            //var sslOpts = new System.Net.Security.SslClientAuthentication();
            //Security.SslClientAuthenticationOptions.RemoteCertificateValidationCallback;

            using (var ms = new System.IO.MemoryStream())
            {
                using (var ssl = new System.Net.Security.SslStream(ms, true, NoncompilantValidation))   //Noncompliant
                {
                    if (ssl.CanSeek)
                    {
                        ssl.Position = 0;
                    }
                }
                using (var ssl = new System.Net.Security.SslStream(ms, true, CompilantValidation))   //Noncompliant
                {
                    if (ssl.CanSeek)
                    {
                        ssl.Position = 0;
                    }
                }
            }

        }

        #region "Helpers"


        void InitAsArgument(RemoteCertificateValidationCallback Callback)
        {
            ServicePointManager.ServerCertificateValidationCallback += Callback;
            CreateRQ().ServerCertificateValidationCallback += Callback;
        }

        HttpWebRequest CreateRQ()
        {
            return (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");
        }

        bool IsValid(X509Certificate crt)
        {
            return crt.Subject == "Test";   //We do not inspect inner logic, yet
        }

        void Log(X509Certificate crt)
        {
            //Pretend to do some logging
        }

        #endregion

        #region "Basic Validators"

        bool NoncompilantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //Noncompliant {{Enable server certificate validation on this SSL/TLS connection}}
//                 ^^^^
        }

        bool NoncompilantValidationAsArgument(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //Noncompliant {{Enable server certificate validation on this SSL/TLS connection}}
//                 ^^^^
        }

        bool FalseValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return false; //Compliant
        }

        bool CompliantValidationPositiveA(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate.Subject == "Test")
            {
                return true;
            }
            else
            {
                return false; //Compliant, checks were done
            }
        }


        bool CompliantValidationPositiveB(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return certificate.Subject == "Test";
        }


        bool CompliantValidationNegative(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate.Subject != "Test")
            {
                return false;
            }
            else if (DateTime.Parse(certificate.GetExpirationDateString()) < DateTime.Now)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region "Advanced Validators"

        bool AdvNoncompilantTry(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine(certificate.Subject);
                return true; //Noncompliant 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                return true; //Noncompliant 
            }
        }


        bool AdvNoncompilantWithTryObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                Console.WriteLine("Log something");
                System.Diagnostics.Trace.WriteLine("Log something");
                Log(certificate);

                return true; //Noncompliant 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return true; //Noncompliant
        }

        bool AdvCompilantWithTryObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                Console.WriteLine("Log something");
                System.Diagnostics.Trace.WriteLine("Log something");
                Log(certificate);

                return true; //Compliant, since Log(certificate) can also do some validation and throw exception resulting in return false. It's bad practice, but compliant.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return false; 
        }
        
        bool AdvNoncompilantWithObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Log something");
            System.Diagnostics.Trace.WriteLine("Log something");
            Log(certificate);

            return true; //Noncompliant
        }


        bool AdvCompilantWithObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Log something");
            System.Diagnostics.Trace.WriteLine("Log something");
            Log(certificate);
            return IsValid(certificate);
        }

        #endregion

    }
}
