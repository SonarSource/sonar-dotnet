
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
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => true;
//                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Enable server certificate validation on this SSL/TLS connection}}
//                                                                                                             ^^^^ Secondary@-1
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
            rq.ServerCertificateValidationCallback += InvalidValidation;            //Noncompliant [flow2]

            //Without variable
            CreateRQ().ServerCertificateValidationCallback += InvalidValidation;    //Noncompliant [flow3]

            //Assignment syntax = instead of +=
            CreateRQ().ServerCertificateValidationCallback = InvalidValidation;    //Noncompliant  [flow4]
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
            ServicePointManager.ServerCertificateValidationCallback += InvalidValidation;                          //Noncompliant [flow5]
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationPositiveB;
            ServicePointManager.ServerCertificateValidationCallback += CompliantValidationNegative;
            ServicePointManager.ServerCertificateValidationCallback += AdvInvalidTry;                              //Noncompliant [flow6]
            ServicePointManager.ServerCertificateValidationCallback += AdvInvalidWithTryObstacles;                 //Noncompliant [flow7]
            ServicePointManager.ServerCertificateValidationCallback += AdvCompliantWithTryObstacles;
            ServicePointManager.ServerCertificateValidationCallback += AdvInvalidWithObstacles;                    //Noncompliant [flow8]
            ServicePointManager.ServerCertificateValidationCallback += AdvCompliantWithObstacles;
        }

        void GenericHandlerSignature()
        {
            var httpHandler = new System.Net.Http.HttpClientHandler();          //This is not RemoteCertificateValidationCallback delegate type, but Func<...>
            httpHandler.ServerCertificateCustomValidationCallback += InvalidValidation;            //Noncompliant [flow9]

            //Generic signature check without RemoteCertificateValidationCallback
            var ShouldTrigger = new RelatedSignatureType();
            ShouldTrigger.Callback += InvalidValidation;                                           //Noncompliant [flow10]
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
                                                                                    //Secondary@+1 [flow0]
                SingleAssignmentCB = InvalidValidationAsArgument;                   //Secondary [flow1]
                FalseNegativeCB = InvalidValidation;                                //Compliant due to false negative, the second assignment is after usage of the variable
                CompliantCB = InvalidValidation;                                    //Compliant due to further logic and more assingments
            }
            if (true) //Environment.ComputerName, Environment.CommandLine, Debugger.IsAttached, Config.TestEnvironment
            {
                CompliantCB = null;                                                 //Compliant, there are more assignments, so there is a logic
                DeclarationAssignmentCompliantCB = InvalidValidation;               //This is compliant due to the more assignments, first one is in variable initialization
            }
                                                                                    //Secondary@+1 [flow0]
            InitAsArgument(SingleAssignmentCB);                                     //Secondary [flow1]
            InitAsArgument(FalseNegativeCB);
            InitAsArgument(CompliantCB);
            InitAsArgument(DeclarationAssignmentCompliantCB);
            FalseNegativeCB = null;                                                 //False negative due to more assignments, but this one is after variable usage.

            InitAsArgument(InvalidValidationAsArgument);                            //Secondary [flow0, flow1]
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => false);
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary [flow0, flow1]

            InitAsArgumentRecursive(InvalidValidation, 1);                          //Secondary [flow17]
            InitAsOptionalArgument();

            //Call in nested class from root (this)
            new InnerAssignmentClass().InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary           
        }

        void DelegateReturnedByFunction()
        {
            CreateRQ().ServerCertificateValidationCallback += FindInvalid(false);       //Noncompliant [flow11]
            CreateRQ().ServerCertificateValidationCallback += FindInvalid();            //Noncompliant [flow12]
            CreateRQ().ServerCertificateValidationCallback += FindLambdaValidator();    //Noncompliant [flow13]
            CreateRQ().ServerCertificateValidationCallback += FindCompliant(true);
            CreateRQ().ServerCertificateValidationCallback += FindCompliantRecursive(3);
            CreateRQ().ServerCertificateValidationCallback += FindInvalidRecursive(3);  //Noncompliant [flow14]
        }

        void ConstructorArguments()
        {
            var optA = new OptionalConstructorArguments(this, cb: InvalidValidation);   //Noncompliant [flow15]
            var optB = new OptionalConstructorArguments(this, cb: CompliantValidation);

            using (var ms = new System.IO.MemoryStream())
            {
                using (var ssl = new System.Net.Security.SslStream(ms, true, (sender, chain, certificate, SslPolicyErrors) => true))
//                                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Enable server certificate validation on this SSL/TLS connection}}
//                                                                                                                            ^^^^ Secondary@-1
                {
                }
                using (var ssl = new System.Net.Security.SslStream(ms, true, InvalidValidation))   //Noncompliant [flow16]
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
            var cb = Callback;                                              //Secondary [flow1]
            CreateRQ().ServerCertificateValidationCallback += Callback;     //Noncompliant [flow0]
            CreateRQ().ServerCertificateValidationCallback += cb;           //Noncompliant [flow1]
        }

        void InitAsOptionalArgument(RemoteCertificateValidationCallback Callback = null)
        {
            CreateRQ().ServerCertificateValidationCallback += Callback;     //Compliant, it is called without argument
        }

        void InitAsArgumentRecursive(RemoteCertificateValidationCallback Callback, int cnt)
        {
            if (cnt == 0)
                CreateRQ().ServerCertificateValidationCallback += Callback;     //Noncompliant [flow17]
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
            return true;    //Secondary [flow2, flow3, flow4, flow5, flow9, flow10, flow11, flow12, flow14, flow15, flow16, flow17]
        }

        bool InvalidValidationAsArgument(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;    //Secondary [flow0, flow0, flow1, flow1]
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
                return true; //Secondary [flow6]
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                return true; //Secondary [flow6]
            }
        }
        
        bool AdvInvalidWithTryObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                Console.WriteLine("Log something");
                System.Diagnostics.Trace.WriteLine("Log something");
                Log(certificate);

                return true; //Secondary [flow7]
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return true; //Secondary [flow7]
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

            return true; //Secondary [flow8]
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
            return InvalidValidation;                                      //Secondary [flow12]
        }

        static RemoteCertificateValidationCallback FindLambdaValidator()
        {
            return (sender, certificate, chain, SslPolicyErrors) => true;        //Secondary [flow13]
        }

        static RemoteCertificateValidationCallback FindInvalid(bool useDelegate)   //All paths return noncompliant
        {
            if (useDelegate)
            {
                return InvalidValidation;                                  //Secondary [flow11]
            }
            else
            {
                return (sender, certificate, chain, SslPolicyErrors) => true;   //Secondary [flow11]
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
                return InvalidValidation;                                  //Secondary [flow14]
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
