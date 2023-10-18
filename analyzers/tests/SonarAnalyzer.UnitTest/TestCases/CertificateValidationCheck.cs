
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
            //Specific cases
            CreateRQ().ServerCertificateValidationCallback += FalseNegativeException;
        }

        void DirectAddHandlers()
        {
            //Inline version
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => true;
            //         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Enable server certificate validation on this SSL/TLS connection}}
            //                                                                                                 ^^^^ Secondary@-1 {{This function trusts all certificates.}}
            //Secondary@+1 {{This function trusts all certificates.}}
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => (((true)));    //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => false;
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => certificate.Subject == "Test";

            //Lambda block syntax
            //Secondary@+1 {{This function trusts all certificates.}}
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => { return true; };    //Noncompliant
            CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) =>                      //Noncompliant
            {
                return true;    //Secondary {{This function trusts all certificates.}}
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
                                                                                                                                   //Secondary@-1 {{This function trusts all certificates.}}
            CreateRQ().ServerCertificateValidationCallback += LocalFunction; // FN
            bool LocalFunction(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

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
            ServicePointManager.ServerCertificateValidationCallback += AdvCompliantWithException;
            ServicePointManager.ServerCertificateValidationCallback += AdvCompliantWithExceptionAndRethrow;
        }

        void GenericHandlerSignature()
        {
            var httpHandler = new HttpClientHandler();          //This is not RemoteCertificateValidationCallback delegate type, but Func<...>
            httpHandler.ServerCertificateCustomValidationCallback += InvalidValidation;            //Noncompliant [flow9]
            httpHandler.ServerCertificateCustomValidationCallback += HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;    // Noncompliant [flow19]
                                                                                                                                        // Secondary@-1 [flow19] {{This function trusts all certificates.}}

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
                //Secondary@+1 [flow0] {{This function trusts all certificates.}}
                SingleAssignmentCB = InvalidValidationAsArgument;                   //Secondary [flow1] {{This function trusts all certificates.}}
                FalseNegativeCB = InvalidValidation;                                //Compliant due to false negative, the second assignment is after usage of the variable
                CompliantCB = InvalidValidation;                                    //Compliant due to further logic and more assingments
            }
            if (true) //Environment.ComputerName, Environment.CommandLine, Debugger.IsAttached, Config.TestEnvironment
            {
                CompliantCB = null;                                                 //Compliant, there are more assignments, so there is a logic
                DeclarationAssignmentCompliantCB = InvalidValidation;               //This is compliant due to the more assignments, first one is in variable initialization
            }
            //Secondary@+1 [flow0] {{This function trusts all certificates.}}
            InitAsArgument(SingleAssignmentCB);                                     //Secondary [flow1] {{This function trusts all certificates.}}
            InitAsArgument(FalseNegativeCB);
            InitAsArgument(CompliantCB);
            InitAsArgument(DeclarationAssignmentCompliantCB);
            FalseNegativeCB = null;                                                 //False negative due to more assignments, but this one is after variable usage.

            InitAsArgument(InvalidValidationAsArgument);                            //Secondary [flow0, flow1] {{This function trusts all certificates.}}
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => false);
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary [flow0, flow1] {{This function trusts all certificates.}}

            InitAsArgumentRecursive(InvalidValidation, 1);                          //Secondary [flow17] {{This function trusts all certificates.}}
            InitAsOptionalArgument();

            //Call in nested class from root (this)
            new InnerAssignmentClass().InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  // Secondary {{This function trusts all certificates.}}
        }

        void DelegateReturnedByFunction(HttpClientHandler httpHandler)
        {
            CreateRQ().ServerCertificateValidationCallback += FindInvalid(false);       //Noncompliant [flow11]
            CreateRQ().ServerCertificateValidationCallback += FindInvalid();            //Noncompliant [flow12]
            CreateRQ().ServerCertificateValidationCallback += FindLambdaValidator();    //Noncompliant [flow13]
            CreateRQ().ServerCertificateValidationCallback += FindCompliant(true);
            CreateRQ().ServerCertificateValidationCallback += FindCompliantRecursive(3);
            CreateRQ().ServerCertificateValidationCallback += FindInvalidRecursive(3);  //Noncompliant [flow14]

            httpHandler.ServerCertificateCustomValidationCallback += FindDangerous();   //Noncompliant [flow20]
        }

        void ConstructorArguments()
        {
            var optA = new OptionalConstructorArguments(this, cb: InvalidValidation);   //Noncompliant [flow15]
            var optB = new OptionalConstructorArguments(this, cb: CompliantValidation);

            using (var ms = new System.IO.MemoryStream())
            {
                using (var ssl = new System.Net.Security.SslStream(ms, true, (sender, chain, certificate, SslPolicyErrors) => true))
                //                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Enable server certificate validation on this SSL/TLS connection}}
                //                                                                                                            ^^^^ Secondary@-1 {{This function trusts all certificates.}}
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
            var cb = Callback;                                              //Secondary [flow1] {{This function trusts all certificates.}}
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
            return true;    //Secondary [flow2, flow3, flow4, flow5, flow9, flow10, flow11, flow12, flow14, flow15, flow16, flow17] {{This function trusts all certificates.}}
        }

        bool InvalidValidationAsArgument(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;    //Secondary [flow0, flow0, flow1, flow1] {{This function trusts all certificates.}}
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
                return true; //Secondary [flow6] {{This function trusts all certificates.}}
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                return true; //Secondary [flow6] {{This function trusts all certificates.}}
            }
        }

        bool AdvInvalidWithTryObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                Console.WriteLine("Log something");
                System.Diagnostics.Trace.WriteLine("Log something");
                Log(certificate);

                return true; //Secondary [flow7] {{This function trusts all certificates.}}
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return true; //Secondary [flow7] {{This function trusts all certificates.}}
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

            return true; //Secondary [flow8] {{This function trusts all certificates.}}
        }

        bool AdvCompliantWithObstacles(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Log something");
            System.Diagnostics.Trace.WriteLine("Log something");
            Log(certificate);
            return IsValid(certificate);
        }

        bool AdvCompliantWithException(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate.Subject != "test")
            {
                throw new InvalidOperationException("You shall not pass!");
            }
            return true;    //Compliant, uncaught exception is thrown above
        }

        bool AdvCompliantWithExceptionAndRethrow(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                if (certificate.Subject != "test")
                {
                    throw new InvalidOperationException("You shall not pass!");
                }
                return true;    //Compliant due to throw logic
            }
            catch
            {
                //Log
                throw;
            }
            return true;        //Compliant due to throw logic
        }

        #endregion

        #region Find Validators

        static RemoteCertificateValidationCallback FindInvalid()
        {
            return InvalidValidation;                                      //Secondary [flow12] {{This function trusts all certificates.}}
        }

        static RemoteCertificateValidationCallback FindLambdaValidator()
        {
            return (sender, certificate, chain, SslPolicyErrors) => true;        //Secondary [flow13] {{This function trusts all certificates.}}
        }

        static Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, Boolean> FindDangerous()
        {
            return HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;        //Secondary [flow20] {{This function trusts all certificates.}}
        }

        static RemoteCertificateValidationCallback FindInvalid(bool useDelegate)   //All paths return noncompliant
        {
            if (useDelegate)
            {
                return InvalidValidation;                                  //Secondary [flow11] {{This function trusts all certificates.}}
            }
            else
            {
                return (sender, certificate, chain, SslPolicyErrors) => true;   //Secondary [flow11] {{This function trusts all certificates.}}
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
                return InvalidValidation;                                  //Secondary [flow14] {{This function trusts all certificates.}}
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

        bool FalseNegativeException(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                if (certificate.Subject != "test")
                {
                    throw new InvalidOperationException("You shall not pass! But you will anyway.");
                }
                return true;    //False negative
            }
            catch   //All exceptions are cought, even those throw from inner DoValidation(crt).. helpers
            {
                //Log, no throw
            }
            return true;        //False negative
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

            public void InitAsParamsArgument(params RemoteCertificateValidationCallback[] callbacks)
            {
                CertificateValidationChecks.CreateRQ().ServerCertificateValidationCallback += callbacks;  //Error [CS0029]
            }
        }

        class NeighbourAssignmentClass
        {
            public void Init(RemoteCertificateValidationCallback callback)
            {
                //Assignment from sibling class in nested tree
                new InnerAssignmentClass().InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary {{This function trusts all certificates.}}
            }
        }

        #endregion

    }

    class LocalFunctionCase
    {
        public void Foo()
        {
            InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  // Secondary {{This function trusts all certificates.}}

            HttpWebRequest CreateRQ()
            {
                return (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");
            }

            void InitAsArgument(RemoteCertificateValidationCallback callback)
            {
                CreateRQ().ServerCertificateValidationCallback += callback; // Noncompliant
            }
        }
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/4404
    struct AssignmentStruct
    {
        HttpWebRequest CreateRQ()
        {
            return (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");
        }

        public void InitAsArgument(RemoteCertificateValidationCallback callback)
        {
            CreateRQ().ServerCertificateValidationCallback += callback; // Noncompliant
        }

        static void Execute()
        {
            new AssignmentStruct().InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);
//                                                                                                 ^^^^ Secondary {{This function trusts all certificates.}}
        }
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/4710
    public static class ReproFor4710
    {
        public static void SomeMethodWithNameofCall()
        {
            Console.WriteLine(nameof(SomeMethodWithNameofCall));
        }

        public static void RestoreCertificateValidation(RemoteCertificateValidationCallback prevValidator)
        {
            ServicePointManager.ServerCertificateValidationCallback = prevValidator;
        }
    }

    public static class UnknownTypeUSage
    {
        public static void SomeMethod(RemoteCertificateValidationCallback validator)
        {
            UnknownType.RestoreCertificateValidation(validator); // Error [CS0103]
        }

        public static void RestoreCertificateValidation(RemoteCertificateValidationCallback prevValidator)
        {
            ServicePointManager.ServerCertificateValidationCallback = prevValidator;
        }
    }

    public class SomeClass
    {
        void MultipleHandlers()
        {
            var httpHandler = new HttpClientHandler();

            httpHandler.ServerCertificateCustomValidationCallback = ChainValidator(httpHandler.ServerCertificateCustomValidationCallback);
        }

        private static Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ChainValidator(Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> previousValidator)
        {

            Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> chained =
                (request, certificate, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None)
                    {
                        return previousValidator(request, certificate, chain, sslPolicyErrors);
                    }
                    return false;
                };
            return chained;
        }
    }
}
