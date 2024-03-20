' <Your-Product-Name>
' Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
'
' Please configure this header in your SonarCloud/SonarQube quality profile.
' You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.

Imports System.Net
Imports System.Net.Http
Imports System.Security.Authentication

Public Module S4423

    Public Sub WeakProtocols()
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls ' Noncompliant (S4423)

        Dim HttpHandler As New HttpClientHandler With { .SslProtocols = SslProtocols.Default } ' Noncompliant
    End Sub

    Public Sub StrongProtocols()
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 ' Compliant

        Dim HttpHandler As New HttpClientHandler With { .SslProtocols = SslProtocols.None } ' Compliant
    End Sub

End Module
