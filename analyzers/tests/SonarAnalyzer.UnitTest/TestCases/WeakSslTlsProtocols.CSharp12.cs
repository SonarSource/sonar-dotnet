using System.Net;

class WeakSslTlsProtocols(SecurityProtocolType classParam = SecurityProtocolType.Ssl3) // Noncompliant
{
    void SecurityProtocolTypeNonComplaint(SecurityProtocolType methodParam = SecurityProtocolType.Ssl3) // Noncompliant
    {
        var lambda = (SecurityProtocolType lambdaParam = SecurityProtocolType.Ssl3) => lambdaParam; // Noncompliant
    }
}

