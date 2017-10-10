Namespace Tests.Diagnostics
    Module Module1
        Sub subJect__SomeEvent() Handles X.SomeEvent   ' Noncompliant - two underscores
        End Sub
        Sub subJect_SomeEvent() Handles X.SomeEvent    ' Compliant
        End Sub

        Sub SubbJect_SomeEvent() Handles X.SomeEvent    ' Compliant
        End Sub

        Sub OnMyButtonClicked() Handles X.SomeEvent    ' Compliant
        End Sub
        Sub oonMyButtonClicked() Handles X.SomeEvent    ' Noncompliant
        End Sub
    End Module
End Namespace