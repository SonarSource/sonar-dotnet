using System.Net;

public class WeakSslTlsProtocols(SecurityProtocolType classParam = SecurityProtocolType.Ssl3) // Noncompliant
{
    public void SecurityProtocolTypeNonComplaint()
    {
        var lambda = (SecurityProtocolType lambdaParam = SecurityProtocolType.Ssl3) => lambdaParam; // Noncompliant
    }
}

