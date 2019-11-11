
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
            //False Negative - not implemented case
            CreateRQ().ServerCertificateValidationCallback += FindValidator(false);  //Compliant due to known False Negative

            //Inline version
            //Secondary@+1
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => true;    //Noncompliant  {{Enable server certificate validation on this SSL/TLS connection}}
                                                                                                                        //                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^                                                     ^^^^
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => false;
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => certificate.Subject == "Test";

            //Without variable
            //Secondary@+1
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => true;    //Noncompliant

            //Other occurances
            var rq = CreateRQ();
            rq.ServerCertificateValidationCallback += NoncompilantValidation;                           //Noncompliant

            //Do not test this one. It's .NET Standard 2.1 target only. It shoudl work since we're hunting RemoteCertificateValidationCallback and method signature
            //var ws = new System.Net.WebSockets.ClientWebSocket();
            //ws.Options.RemoteCertificateValidationCallback += NoncompilantValidation;

            //Do not test this one. It's .NET Standard 2.1 target only. It shoudl work since we're hunting RemoteCertificateValidationCallback and method signature
            //var sslOpts = new System.Net.Security.SslClientAuthentication();
            //Security.SslClientAuthenticationOptions.RemoteCertificateValidationCallback;
        }

        void MultipleHandlers() {
            ServicePointManager.ServerCertificateValidationCallback += CompilantValidation;
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationPositiveA;
            ServicePointManager.ServerCertificateValidationCallback += NoncompilantValidation;                          //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationPositiveB;
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationNegative;
            ServicePointManager.ServerCertificateValidationCallback += AdvNoncompilantTry;                              //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += AdvNoncompilantWithTryObstacles;                 //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += AdvCompilantWithTryObstacles;
            ServicePointManager.ServerCertificateValidationCallback += AdvNoncompilantWithObstacles;                    //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += AdvCompilantWithObstacles;
        }

        void GenericHandlerSignature()
        {
            var httpHandler = new System.Net.Http.HttpClientHandler();          //This is not RemoteCertificateValidationCallback delegate type, but Func<...>
            httpHandler.ServerCertificateCustomValidationCallback += NoncompilantValidation;            //Noncompliant          

            //Generic signature check without RemoteCertificateValidationCallback
            var ShouldTrigger = new RelatedSignatureType();
            ShouldTrigger.Callback += NoncompilantValidation;                                           //Noncompliant
            ShouldTrigger.Callback += CompilantValidation;

            var ShouldNotTrigger = new NonrelatedSignatureType();
            ShouldNotTrigger.Callback += (sender, chain, certificate, SslPolicyErrors) => true;   //Compliant, because signature types are not in expected order for validation
            ShouldNotTrigger.Callback += (sender, chain, certificate, SslPolicyErrors) => false;
        }

        void PassedAsArgument() {
            RemoteCertificateValidationCallback SingleAssignmentCB, FalseNegativeCB, CompliantCB, DeclarationAssignmentCompliantCB = null;
            if (true)
            {   //If there's only one assignemnt, we will inspect it
                //Secondary@+1
                SingleAssignmentCB = NoncompilantValidationAsArgument;              //Secondary
                FalseNegativeCB = NoncompilantValidation;                           //Compliant due to false negative, the second assignment is after usage of the variable
                CompliantCB = NoncompilantValidation;                               //Compliant due to further logic and more assingments
            }
            if (true) //Environment.ComputerName, Environment.CommandLine, Debugger.IsAttached, Config.TestEnvironment
            {
                CompliantCB = null;                                                 //Compliant, there are more assignments, so there is a logic
                DeclarationAssignmentCompliantCB = NoncompilantValidation;          //This is compliant due to the more assignments, first one is in variable initialization
            }
            //Secondary@+1
            InitAsArgument(SingleAssignmentCB);                                     //Secondary
            InitAsArgument(FalseNegativeCB);
            InitAsArgument(CompliantCB);
            InitAsArgument(DeclarationAssignmentCompliantCB);
            FalseNegativeCB = null;                                                 //False negative due to more assignments, but this one is after variable usage.
                                                                                    //Secondary@+1
            InitAsArgument(NoncompilantValidationAsArgument);                       //Secondary
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => false);
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary
                                                                                    //Secondary@-1

        }

        void ConstructorArguments() { 
            var optA = new OptionalConstructorArguments(this, cb: NoncompilantValidation);                 //Noncompliant
            var optB = new OptionalConstructorArguments(this, cb: CompilantValidation);                  

            using (var ms = new System.IO.MemoryStream())
            {                                                                                                                          //Secondary@+1
                using (var ssl = new System.Net.Security.SslStream(ms, true, (sender, chain, certificate, SslPolicyErrors) => true))   //Noncompliant
                {
                }
                using (var ssl = new System.Net.Security.SslStream(ms, true, NoncompilantValidation))   //Noncompliant
                {
                }
                using (var ssl = new System.Net.Security.SslStream(ms, true, CompilantValidation))
                {
                }
            }

        }

        #region Helpers


        void InitAsArgument(RemoteCertificateValidationCallback Callback)   //This double-assigment will fire the seconday for each occurence twice
        {
            var cb = Callback;                                              //Secondary
            CreateRQ().ServerCertificateValidationCallback += Callback;     //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += cb;           //Noncompliant
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

        static RemoteCertificateValidationCallback FindValidator(bool Compliant)
        {
            if (Compliant)
            {
                return CompilantValidation;
            }
            else 
            {
                return FindValidatorNested(3);
            }
        }

        static RemoteCertificateValidationCallback FindValidatorNested(int Index)
        {
            if (Index <=0 )
            {
                return NoncompilantValidation;
            }
            else
            {
                return FindValidatorNested(Index - 1);
            }
        }

        #endregion

        #region Basic Validators

        static bool NoncompilantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;    //Secondary
                            //Secondary@-1
                            //Secondary@-2
                            //Secondary@-3
                            //Secondary@-4
                            //Secondary@-5

        }

        bool NoncompilantValidationAsArgument(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;    //Secondary
                            //Secondary@-1
                            //Secondary@-2
                            //Secondary@-3
        }

        static bool CompilantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

        #region Advanced Validators

        bool AdvNoncompilantTry(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine(certificate.Subject);
                return true; //Secondary
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                return true; //Secondary 
            }
        }


        bool AdvNoncompilantWithTryObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                Console.WriteLine("Log something");
                System.Diagnostics.Trace.WriteLine("Log something");
                Log(certificate);

                return true; //Secondary
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return true; //Secondary
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

            return true; //Secondary
        }


        bool AdvCompilantWithObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Log something");
            System.Diagnostics.Trace.WriteLine("Log something");
            Log(certificate);
            return IsValid(certificate);
        }

        #endregion

        #region Nested classes

        class RelatedSignatureType
        {

            public Func<NonrelatedSignatureType, X509Certificate2, X509Chain, SslPolicyErrors, Boolean> Callback { get; set; }

        }

        class NonrelatedSignatureType
        {
            //Parameters are in order, that we do not inspect
            public Func<NonrelatedSignatureType, X509Chain, X509Certificate2, SslPolicyErrors, Boolean> Callback { get; set; }

        }

        class OptionalConstructorArguments
        {

            public OptionalConstructorArguments(object owner, int a = 0, int b = 0, RemoteCertificateValidationCallback cb = null)
            {

            }

        }


        #endregion

    }
}
