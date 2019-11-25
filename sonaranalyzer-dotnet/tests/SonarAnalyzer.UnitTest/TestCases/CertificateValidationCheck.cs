
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Tests.Diagnostics
{
    class CertificateValidationChecks
    {

        void FalseNegatives()
        {
            //Values from properties are not inspected at all
            CreateRQ().ServerCertificateValidationCallback += FalseNegativeValidatorWithProperty;
            CreateRQ().ServerCertificateValidationCallback += DelegateProperty;
            //Values from overriden operators are not inspected at all
            CreateRQ().ServerCertificateValidationCallback += new CertificateValidationChecks() + 42; //Operator + is overriden to return delegate. 
        }

        void DirectAddHandlers()
        {
            //Inline version
            //Secondary@+1
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => true;    //Noncompliant  {{Enable server certificate validation on this SSL/TLS connection}}
//                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^                                                     ^^^^
            //Secondary@+1
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => (((true)));    //Noncompliant 
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => false;
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => certificate.Subject == "Test";

            //Lambda block syntax
                                                                                                                                    //Secondary@+1
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => { return true; };    //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) =>                      //Noncompliant
            {
                return true;    //Secondary
            };
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) =>
            {
                return false;
            };

            //With variable
            var rq = CreateRQ();
            rq.ServerCertificateValidationCallback += InvalidValidation;            //Noncompliant

            //Without variable
            CreateRQ().ServerCertificateValidationCallback += InvalidValidation;    //Noncompliant

            //Assignment syntax = instead of +=
            CreateRQ().ServerCertificateValidationCallback = InvalidValidation;    //Noncompliant
            CreateRQ().ServerCertificateValidationCallback = (sender, certificate, chain, SslPolicyErrors) => { return true; };    //Noncompliant
                                                                                   //Secondary@-1

            //Do not test this one. It's .NET Standard 2.1 target only. It should work since we're hunting RemoteCertificateValidationCallback and method signature
            //var ws = new System.Net.WebSockets.ClientWebSocket();
            //ws.Options.RemoteCertificateValidationCallback += InvalidValidation;

            //Do not test this one. It's .NET Standard 2.1 target only. It should work since we're hunting RemoteCertificateValidationCallback and method signature
            //var sslOpts = new System.Net.Security.SslClientAuthentication();
            //Security.SslClientAuthenticationOptions.RemoteCertificateValidationCallback;
        }

        void MultipleHandlers()
        {
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidation;
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationPositiveA;
            ServicePointManager.ServerCertificateValidationCallback += InvalidValidation;                          //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationPositiveB;
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationNegative;
            ServicePointManager.ServerCertificateValidationCallback += AdvInvalidTry;                              //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += AdvInvalidWithTryObstacles;                 //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += AdvCompliantWithTryObstacles;
            ServicePointManager.ServerCertificateValidationCallback += AdvInvalidWithObstacles;                    //Noncompliant
            ServicePointManager.ServerCertificateValidationCallback += AdvCompliantWithObstacles;
        }

        void GenericHandlerSignature()
        {
            var httpHandler = new System.Net.Http.HttpClientHandler();          //This is not RemoteCertificateValidationCallback delegate type, but Func<...>
            httpHandler.ServerCertificateCustomValidationCallback += InvalidValidation;            //Noncompliant          

            //Generic signature check without RemoteCertificateValidationCallback
            var ShouldTrigger = new RelatedSignatureType();
            ShouldTrigger.Callback += InvalidValidation;                                           //Noncompliant
            ShouldTrigger.Callback += CompliantValidation;

            var ShouldNotTrigger = new NonrelatedSignatureType();
            ShouldNotTrigger.Callback += (sender, chain, certificate, SslPolicyErrors) => true;   //Compliant, because signature types are not in expected order for validation
            ShouldNotTrigger.Callback += (sender, chain, certificate, SslPolicyErrors) => false;
        }

        void PassedAsArgument()
        {
            RemoteCertificateValidationCallback SingleAssignmentCB, FalseNegativeCB, CompliantCB, DeclarationAssignmentCompliantCB = null;
            if (true)
            {
                //If there's only one Assignment, we will inspect it
                //Secondary@+1
                SingleAssignmentCB = InvalidValidationAsArgument;                   //Secondary
                FalseNegativeCB = InvalidValidation;                                //Compliant due to false negative, the second assignment is after usage of the variable
                CompliantCB = InvalidValidation;                                    //Compliant due to further logic and more assingments
            }
            if (true) //Environment.ComputerName, Environment.CommandLine, Debugger.IsAttached, Config.TestEnvironment
            {
                CompliantCB = null;                                                 //Compliant, there are more assignments, so there is a logic
                DeclarationAssignmentCompliantCB = InvalidValidation;               //This is compliant due to the more assignments, first one is in variable initialization
            }
            //Secondary@+1
            InitAsArgument(SingleAssignmentCB);                                     //Secondary
            InitAsArgument(FalseNegativeCB);
            InitAsArgument(CompliantCB);
            InitAsArgument(DeclarationAssignmentCompliantCB);
            FalseNegativeCB = null;                                                 //False negative due to more assignments, but this one is after variable usage.
                                                                                    //Secondary@+1
            InitAsArgument(InvalidValidationAsArgument);                            //Secondary
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => false);
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary
                                                                                    //Secondary@-1
            InitAsArgumentRecursive(InvalidValidation, 1);                          //Secondary
            InitAsOptionalArgument();

            //Call in nested class from root (this)
            new InnerAssignmentClass().InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary           
        }

        void DelegateReturnedByFunction()
        {
            CreateRQ().ServerCertificateValidationCallback += FindInvalid(false);       //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += FindInvalid();            //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += FindLambdaValidator();    //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += FindCompliant(true);
            CreateRQ().ServerCertificateValidationCallback += FindCompliantRecursive(3);
            CreateRQ().ServerCertificateValidationCallback += FindInvalidRecursive(3);  //Noncompliant
        }

        void ConstructorArguments()
        {
            var optA = new OptionalConstructorArguments(this, cb: InvalidValidation);                 //Noncompliant
            var optB = new OptionalConstructorArguments(this, cb: CompliantValidation);

            using (var ms = new System.IO.MemoryStream())
            {                                                                                                                          //Secondary@+1
                using (var ssl = new System.Net.Security.SslStream(ms, true, (sender, chain, certificate, SslPolicyErrors) => true))   //Noncompliant
//                                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^     ^^^^
                {
                }
                using (var ssl = new System.Net.Security.SslStream(ms, true, InvalidValidation))   //Noncompliant
                {
                }
                using (var ssl = new System.Net.Security.SslStream(ms, true, CompliantValidation))
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

        void InitAsOptionalArgument(RemoteCertificateValidationCallback Callback = null)
        {
            CreateRQ().ServerCertificateValidationCallback += Callback;     //Compliant, it is called without argument
        }

        void InitAsArgumentRecursive(RemoteCertificateValidationCallback Callback, int cnt)
        {
            if (cnt == 0)
                CreateRQ().ServerCertificateValidationCallback += Callback;     //Noncompliant
            else
                InitAsArgumentRecursive(Callback, cnt - 1);
        }
        
        void InitAsArgumentRecursiveNoInvocation(RemoteCertificateValidationCallback Callback, int cnt)
        {
            if (cnt == 0)
            {
                CreateRQ().ServerCertificateValidationCallback += Callback;     //Compliant, no one is invoking this
            }
            else
            {
                InitAsArgumentRecursiveNoInvocation(Callback, cnt - 1);
            }
        }
        
        static HttpWebRequest CreateRQ()
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

        #region Basic Validators

        static bool InvalidValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;    //Secondary
                            //Secondary@-1
                            //Secondary@-2
                            //Secondary@-3
                            //Secondary@-4
                            //Secondary@-5
                            //Secondary@-6
                            //Secondary@-7
                            //Secondary@-8
                            //Secondary@-9
                            //Secondary@-10            
                            //Secondary@-11
        }

        bool InvalidValidationAsArgument(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;    //Secondary
                            //Secondary@-1
                            //Secondary@-2
                            //Secondary@-3
        }

        static bool CompliantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return false; //Compliant
        }

        bool CompliantValidationPositiveA(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate.Subject == "Test")
            {
                return true;    //Compliant, checks were done
            }
            else
            {
                return false; 
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

        bool AdvInvalidTry(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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
        
        bool AdvInvalidWithTryObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

        bool AdvCompliantWithTryObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

        bool AdvInvalidWithObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Log something");
            System.Diagnostics.Trace.WriteLine("Log something");
            Log(certificate);

            return true; //Secondary
        }
        
        bool AdvCompliantWithObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Log something");
            System.Diagnostics.Trace.WriteLine("Log something");
            Log(certificate);
            return IsValid(certificate);
        }

        #endregion

        #region Find Validators
        
        static RemoteCertificateValidationCallback FindInvalid()
        {
            return InvalidValidation;                                      //Secondary
        }

        static RemoteCertificateValidationCallback FindLambdaValidator()
        {
            return (sender, certificate, chain, SslPolicyErrors) => true;        //Secondary
        }

        static RemoteCertificateValidationCallback FindInvalid(bool useDelegate)   //All paths return noncompliant
        {
            if (useDelegate)
            {
                return InvalidValidation;                                  //Secondary
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
                return CompliantValidation;
            }
            else
            {
                return FindCompliantRecursive(Index - 1);
            }
        }

        static RemoteCertificateValidationCallback FindInvalidRecursive(int Index)
        {
            if (Index <= 0)
            {
                return InvalidValidation;                                  //Secondary
            }
            else
            {
                return FindInvalidRecursive(Index - 1);
            }
        }

        #endregion
        
        #region False negatives

        static RemoteCertificateValidationCallback DelegateProperty
        {
            get
            {
                return (sender, certificate, chain, SslPolicyErrors) => true;   //False negative
            }
        }

        static bool FalseNegativeValidatorWithProperty(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return TrueProperty;    //False negative
        }

        static bool TrueProperty
        {
            get
            {
                return true;        //False negative
            }
        }

        public static RemoteCertificateValidationCallback operator +(CertificateValidationChecks instance, int number)
        {
            return (sender, certificate, chain, SslPolicyErrors) => true;
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

        class InnerAssignmentClass
        {

            public void InitAsArgument(RemoteCertificateValidationCallback callback)
            {
                CertificateValidationChecks.CreateRQ().ServerCertificateValidationCallback += callback; //Noncompliant
            }

        }

        class NeighbourAssignmentClass
        {

            public void Init(RemoteCertificateValidationCallback callback)
            {
                //Assignment from sibling class in nested tree
                new InnerAssignmentClass().InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary           
            }

        }

        #endregion

    }
}
