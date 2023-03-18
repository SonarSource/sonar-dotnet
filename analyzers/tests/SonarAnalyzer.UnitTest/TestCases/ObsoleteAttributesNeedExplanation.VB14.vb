' Commented line for concurrent namespace

Class Noncompliant
    <Obsolete("", True, DiagnosticId:="42")> ' Noncompliant
    Sub WithDiagnostics()
    End Sub

    <Obsolete("", True, DiagnosticId:="42", UrlFormat:="https://sonarsource.com")> ' Noncompliant
    Sub WithDiagnosticsAndUrlFormat()
    End Sub
End Class

Class Compliant
    <Obsolete("explanation", True, DiagnosticId:="42")>
    Sub WithDiagnostics()
    End Sub

    <Obsolete("explanation", True, DiagnosticId:="42", UrlFormat:="https://sonarsource.com")>
    Sub WithDiagnosticsAndUrlFormat()
    End Sub
End Class

Class Ignore
    <Obsolete(UrlFormat:="https://sonarsource.com")>
    Private Sub NamedParametersOnly() ' FP
    End Sub
End Class
