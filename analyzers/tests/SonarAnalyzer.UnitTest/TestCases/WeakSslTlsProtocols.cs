using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

namespace Tests.Diagnostics
{
    public class WeakSslTlsProtocols
    {
        public void SecurityProtocolTypeNonComplaint()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3; // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                      ^^^^

            ServicePointManager.SecurityProtocol = ~((SecurityProtocolType.Ssl3)) | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls; // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                                                                                    ^^^

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                      ^^^

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11; // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                      ^^^^^

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12; // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                      ^^^^

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11; // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                       ^^^^^

            var securityProtocol = SecurityProtocolType.Tls; // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                      ^^^
            ServicePointManager.SecurityProtocol = securityProtocol;
        }

        public void SslProtocolsNonComplaint()
        {
            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Ssl2 // Noncompliant {{Change this code to use a stronger protocol.}}
//                                          ^^^^
            };

            handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Ssl3 // Noncompliant {{Change this code to use a stronger protocol.}}
//                                          ^^^^
            };

            handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Default // Noncompliant {{Change this code to use a stronger protocol.}}
//                                          ^^^^^^^
            };

            handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls // Noncompliant {{Change this code to use a stronger protocol.}}
//                                          ^^^
            };

            handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls11 // Noncompliant {{Change this code to use a stronger protocol.}}
//                                          ^^^^^
            };

            handler.SslProtocols |= SslProtocols.Default; // Noncompliant {{Change this code to use a stronger protocol.}}
//                                               ^^^^^^^

            using var client = new TcpClient("tls-v1-0.badssl.com", 1010);
            var sslStream = new SslStream(client.GetStream(), false, null, null);

            sslStream.AuthenticateAsClient("tls-v1-0.badssl.com", null, SslProtocols.Tls, true); // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                                   ^^^

            sslStream.AuthenticateAsClientAsync("tls-v1-0.badssl.com", null, SslProtocols.Tls, true); // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                                        ^^^

            sslStream.AuthenticateAsServer(null, true, SslProtocols.Tls, true); // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                  ^^^

            sslStream.AuthenticateAsServerAsync(null, true, SslProtocols.Tls, true); // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                       ^^^

            sslStream.BeginAuthenticateAsClient("tls-v1-0.badssl.com", null, SslProtocols.Tls, true, null, null); // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                                        ^^^

            sslStream.BeginAuthenticateAsServer(null, true, SslProtocols.Tls, true, null, null); // Noncompliant {{Change this code to use a stronger protocol.}}
//                                                                       ^^^

            SslProtocols[] protocols = new[]
            {
                SslProtocols.Default, // Noncompliant
//                           ^^^^^^^
                SslProtocols.None
            };

            var listExample = new List<SslProtocols>()
            {
                SslProtocols.Default, // Noncompliant
//                           ^^^^^^^
                SslProtocols.None
            };

            Dictionary<SslProtocols, bool> shouldUseProtocol = new Dictionary<SslProtocols, bool>()
            {
                { SslProtocols.Default, false },  // NonCompliant
//                             ^^^^^^^

                { SslProtocols.None, true }
            };

            var numbers = new Dictionary<SslProtocols, string>
            {
                [SslProtocols.None] = "None",
                [SslProtocols.Default] = "Default", // Noncompliant
//                            ^^^^^^^
            };
        }

        private class Dummy1
        {
            public Dummy1() : this(SslProtocols.Default) // Noncompliant
//                                              ^^^^^^^
            {

            }

            public Dummy1(SslProtocols protocol)
            {

            }
        }

        private class Dummy2 : Dummy1
        {
            public Dummy2() : base(SslProtocols.Default) // Noncompliant
    //                                          ^^^^^^^
            {

            }
        }

        public void SecurityProtocolTypeCompliant()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault; // Compliant
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // Compliant
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13; // Compliant

            ServicePointManager.SecurityProtocol = ~((SecurityProtocolType.Ssl3)); // Compliant
            ServicePointManager.SecurityProtocol = ~SecurityProtocolType.Ssl3; // Compliant
        }

        public void SslProtocolsCompliant()
        {
            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls13 // Compliant
            };

            handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.None // Compliant
            };

            handler.SslProtocols = SslProtocols.Tls12; // Compliant

            var sslProtocol = SslProtocols.None;

            if (sslProtocol != SslProtocols.Default) { } // Compliant

            while (sslProtocol != SslProtocols.Default) { }  // Compliant

            var protocol = SslProtocols.None;
            bool isSafe = protocol == SslProtocols.Tls ? true : false; // Compliant
        }
    }
}
