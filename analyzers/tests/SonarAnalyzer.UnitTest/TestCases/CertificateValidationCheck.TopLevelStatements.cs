
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

CreateRQ().ServerCertificateValidationCallback += (sender, certificate, chain, SslPolicyErrors) // Noncompliant
    => { return true; };    // Secondary

static HttpWebRequest CreateRQ() // static local function
{
    return (HttpWebRequest)System.Net.HttpWebRequest.Create("http://localhost");
}


// See https://github.com/SonarSource/sonar-dotnet/issues/4405
void InitAsArgument(RemoteCertificateValidationCallback callback)
{
    CreateRQ().ServerCertificateValidationCallback += callback; // FN, should be Non-compliant
}

InitAsArgument((sender, certificate, chain, SslPolicyErrors) => true);  //should be a secondary location
