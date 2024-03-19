' <Your-Product-Name>
' Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
'
' Please configure this header in your SonarCloud/SonarQube quality profile.
' You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.

Namespace IntentionalFindings
    Public Class Entity
        Public Property Id As Date  ' Noncompliant (S3363)
    End Class
End Namespace