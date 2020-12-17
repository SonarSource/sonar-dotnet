Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Net
Imports System.Net.Http
Imports System.Net.Security
Imports System.Net.Sockets
Imports System.Security.Authentication

Namespace Tests.Diagnostics
    Public Class WeakSslTlsProtocols

        Public Sub SecurityProtocolTypeNonComplaint()
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                       ^^^^

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                       ^^^

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                       ^^^^^

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls12 ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                       ^^^^
            Dim securityProtocol = SecurityProtocolType.Tls ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                       ^^^
            ServicePointManager.SecurityProtocol = securityProtocol
        End Sub

        Public Sub SslProtocolsNonComplaint()
            Dim HttpHandler As New HttpClientHandler With { .SslProtocols = SslProtocols.Ssl2 } ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                                        ^^^^

            HttpHandler.SslProtocols = SslProtocols.Ssl3 ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                   ^^^^

            HttpHandler.SslProtocols = SslProtocols.Tls ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                   ^^^

            HttpHandler.SslProtocols = SslProtocols.Default ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                   ^^^^^^^

            HttpHandler.SslProtocols = SslProtocols.Tls11 ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                   ^^^^^

            Dim client As TcpClient = New TcpClient("tls-v1-0.badssl.com", 1010)
            Dim sslStream = new SslStream(client.GetStream(), false)

            sslStream.AuthenticateAsClient("tls-v1-0.badssl.com", Nothing, SslProtocols.Tls, true) ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                                       ^^^

            sslStream.AuthenticateAsClientAsync("tls-v1-0.badssl.com", Nothing, SslProtocols.Tls, true) ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                                            ^^^

            sslStream.AuthenticateAsServer(Nothing, true, SslProtocols.Tls, true) ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                      ^^^

            sslStream.AuthenticateAsServerAsync(Nothing, true, SslProtocols.Tls, true) ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                           ^^^

            sslStream.BeginAuthenticateAsClient("tls-v1-0.badssl.com", Nothing, SslProtocols.Tls, true, Nothing, Nothing) ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                                            ^^^

            sslStream.BeginAuthenticateAsServer(Nothing, true, SslProtocols.Tls, true, Nothing, Nothing) ' Noncompliant {{Change this code to use a stronger protocol.}}
'                                                                           ^^^
        
            Dim protocols = New SslProtocols() {SslProtocols.None, SslProtocols.Default } ' Noncompliant
'                                                                               ^^^^^^^
        End Sub

        Private Class Dummy1

            Sub New()
                Me.New(SslProtocols.Default) ' Noncompliant
'                                   ^^^^^^^
            End Sub

            Sub New(protocol As SslProtocols)

            End Sub

        End Class

        Private Class Dumym2
            Inherits Dummy1

            Sub New()
                MyBase.New(SslProtocols.Default) ' Noncompliant
'                                       ^^^^^^^
            End Sub

        End Class

        Public Sub SecurityProtocolTypeCompliant()
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault ' Compliant
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 ' Compliant
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 ' Compliant
        End Sub

        Public Sub SslProtocolsCompliant()
            Dim HttpHandler As New HttpClientHandler With { .SslProtocols = SslProtocols.Tls13 } ' Compliant

            HttpHandler.SslProtocols = SslProtocols.Tls12 ' Compliant

            HttpHandler.SslProtocols = SslProtocols.None ' Compliant

            Dim protocols = SslProtocols.None ' Compliant

            if protocols <> SslProtocols.Default Then ' Compliant

            End If
        End Sub

    End Class
End Namespace
