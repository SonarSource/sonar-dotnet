Namespace Tests.Diagnostics
    Module Module1
        ' Error@+1 [BC30506]
        Sub subJect__SomeEvent() Handles X.SomeEvent   ' Noncompliant - two underscores
        End Sub
        ' Error@+1 [BC30506]
        Sub subJect_SomeEvent() Handles X.SomeEvent    ' Compliant
        End Sub

        ' Error@+1 [BC30506]
        Sub SubbJect_SomeEvent() Handles X.SomeEvent    ' Compliant
        End Sub

        ' Error@+1 [BC30506]
        Sub OnMyButtonClicked() Handles X.SomeEvent    ' Compliant
        End Sub
        ' Error@+1 [BC30506]
        Sub oonMyButtonClicked() Handles X.SomeEvent    ' Noncompliant
        End Sub
    End Module
End Namespace
