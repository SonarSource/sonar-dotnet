Namespace Tests.Diagnostics
    Module Module1
        Sub subJect__SomeEvent() Handles X.SomeEvent   ' Noncompliant - two underscores ' Error [BC30506]
        End Sub
        Sub subJect_SomeEvent() Handles X.SomeEvent    ' Compliant ' Error [BC30506]
        End Sub

        Sub SubbJect_SomeEvent() Handles X.SomeEvent    ' Compliant ' Error [BC30506]
        End Sub

        Sub OnMyButtonClicked() Handles X.SomeEvent    ' Compliant ' Error [BC30506]
        End Sub
        Sub oonMyButtonClicked() Handles X.SomeEvent    ' Noncompliant ' Error [BC30506]
        End Sub
    End Module
End Namespace
