using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

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

record CertificateValidationChecks
{
    void DirectAddHandlers()
    {
        CreateRQ().ServerCertificateValidationCallback += (_, _, _, _) => true;
//                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Enable server certificate validation on this SSL/TLS connection}}
//                                                                        ^^^^ Secondary@-1 {{This function trusts all certificates.}}

        CreateRQ().ServerCertificateValidationCallback += (sender, _, _, SslPolicyErrors) => false;

        // static
        //Secondary@+1
        CreateRQ().ServerCertificateValidationCallback += static (_, _, chain, SslPolicyErrors) => { return true; };    //Noncompliant
    }

    record InnerAssignmentRecord
    {

        public void InitAsArgument(RemoteCertificateValidationCallback callback)
        {
            CreateRQ().ServerCertificateValidationCallback += callback;  //Noncompliant
        }

    }

    record NeighbourAssignmentRecord
    {
        public void Init(RemoteCertificateValidationCallback callback)
        {
            //Assignment from sibling class in nested tree
            new InnerAssignmentRecord().InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary
        }
    }

    void ConstructorArguments()
    {
        OptionalConstructorArguments optA = new(this, cb: InvalidValidation);   // Noncompliant [flow-invalid-1]
        OptionalConstructorArguments optB = new(this, cb: CompliantValidation);

        using (var ms = new System.IO.MemoryStream())
        using (System.Net.Security.SslStream ssl = new(ms, true, (sender, chain, certificate, SslPolicyErrors) => true)) // Noncompliant
                                                                                                                         // Secondary@-1
        {
        }
    }

    record OptionalConstructorArguments
    {
        public OptionalConstructorArguments(object owner, int a = 0, int b = 0, RemoteCertificateValidationCallback cb = null)
        {

        }
    }

    void DelegateReturnedByFunction()
    {
        CreateRQ().ServerCertificateValidationCallback += FindInvalid(false);       //Noncompliant [flow-FindInvalid]
        CreateRQ().ServerCertificateValidationCallback += FindLambdaValidator();    //Noncompliant [flow-lambda]
    }

    static RemoteCertificateValidationCallback FindLambdaValidator()
    {
        return (_, _, _, _) => true;        //Secondary [flow-lambda]
    }

    static RemoteCertificateValidationCallback FindInvalid(bool useDelegate)   //All paths return noncompliant
    {
        if (useDelegate)
        {
            return InvalidValidation;                                  //Secondary [flow-FindInvalid]
        }
        else
        {
            return (sender, certificate, chain, SslPolicyErrors) => true;   //Secondary [flow-FindInvalid]
        }
    }

    void GenericHandlerSignature()
    {
        //Generic signature check without RemoteCertificateValidationCallback
        var ShouldTrigger = new RelatedSignatureType();
        ShouldTrigger.Callback += InvalidValidation;                                           //Noncompliant [flow-invalid-2]
        ShouldTrigger.Callback += CompliantValidation;

        var ShouldNotTrigger = new NonrelatedSignatureType();
        ShouldNotTrigger.Callback += (sender, chain, certificate, SslPolicyErrors) => true;   //Compliant, because signature types are not in expected order for validation
        ShouldNotTrigger.Callback += (sender, chain, certificate, SslPolicyErrors) => false;
    }

    void DelegateReturnedByProperty()
    {
        CreateRQ().ServerCertificateValidationCallback += DelegateProperty;
    }

    static RemoteCertificateValidationCallback DelegateProperty
    {
        get
        {
            return (_, _, _, _) => true;   //False negative
        }
    }

    static HttpWebRequest CreateRQ()
    {
        return (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");
    }

    static bool InvalidValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;    //Secondary [flow-invalid-1, flow-invalid-2, flow-FindInvalid]
    }

    static bool CompliantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return false; //Compliant
    }
}

record AssignmentPositionalRecord(string Value)
{
    HttpWebRequest CreateRQ()
    {
        return (HttpWebRequest)System.Net.HttpWebRequest.Create(Value);
    }

    public void InitAsArgument(RemoteCertificateValidationCallback callback)
    {
        CreateRQ().ServerCertificateValidationCallback += callback;  //Noncompliant
    }

    static void Execute()
    {
        new AssignmentPositionalRecord("http://localhost").InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //Secondary
    }
}

public partial class PartialClass
{
    public static partial RemoteCertificateValidationCallback FindInvalid();

    HttpWebRequest CreateRQ()
    {
        return (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");
    }

    public void Init()
    {
        CreateRQ().ServerCertificateValidationCallback += FindInvalid();  // Noncompliant
    }

    static void Execute()
    {
        new PartialClass().Init();
    }
}

public class NullConditionalAssignment
{
    void GenericHandlerSignature()
    {
        var ShouldTrigger = new RelatedSignatureType();
        ShouldTrigger?.Callback += InvalidValidation;                                          // Noncompliant [flow-invalid-3]
        ShouldTrigger?.Callback += CompliantValidation;

        var ShouldNotTrigger = new NonrelatedSignatureType();
        ShouldNotTrigger?.Callback += (sender, chain, certificate, SslPolicyErrors) => true;   // Compliant, because signature types are not in expected order for validation
        ShouldNotTrigger?.Callback += (sender, chain, certificate, SslPolicyErrors) => false;
    }

    static bool InvalidValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;    //Secondary [flow-invalid-3]
    }

    static bool CompliantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return false; //Compliant
    }
}


record RelatedSignatureType
{
    public Func<NonrelatedSignatureType, X509Certificate2, X509Chain, SslPolicyErrors, Boolean> Callback { get; set; }
}

record NonrelatedSignatureType
{
    //Parameters are in order, that we do not inspect
    public Func<NonrelatedSignatureType, X509Chain, X509Certificate2, SslPolicyErrors, Boolean> Callback { get; set; }
}
