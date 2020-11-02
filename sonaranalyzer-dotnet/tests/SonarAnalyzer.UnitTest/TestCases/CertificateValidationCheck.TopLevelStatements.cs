
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
