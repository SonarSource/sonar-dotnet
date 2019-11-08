public class Class1
{

    public Class1()
    {
        Mini();
    }

    void Mini()
    {
        var rq = (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");
        var httpHandler = new System.Net.Http.HttpClientHandler();

        // Secondary@+1
        rq.ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) => true;    // Noncompliant
        ServicePointManager.ServerCertificateValidationCallback += NoncompilantValidation;                  // Noncompliant
        httpHandler.ServerCertificateCustomValidationCallback += NoncompilantValidation;                    // Noncompliant
    }

    bool NoncompilantValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        // Secondary@+1
        return true; // Secondary
    }

}
