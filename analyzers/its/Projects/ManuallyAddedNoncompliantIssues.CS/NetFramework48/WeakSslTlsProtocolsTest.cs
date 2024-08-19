/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.Net;
using System.Net.Http;
using System.Security.Authentication;

namespace NetFramework48
{
    public class WeakSslTlsProtocolsTest
    {
        public void WeakProtocols()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; // Noncompliant (S4423)

            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Default // Noncompliant
            };
        }

        public void StrongProtocols()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // Compliant

            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.None // Compliant
            };
        }
    }
}
