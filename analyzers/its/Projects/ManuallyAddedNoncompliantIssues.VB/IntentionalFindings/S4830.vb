' <Your-Product-Name>
' Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
'
' Please configure this header in your SonarCloud/SonarQube quality profile.
' You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.

Public Module S4830

    Public Sub TestMethod()
        System.Net.ServicePointManager.ServerCertificateValidationCallback = Function(sender, certificate, chain, sslErrors) True ' Noncompliant (S4830)
    End Sub

End Module
