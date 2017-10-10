Namespace Tests.Diagnostics
    Module Module1
        Sub bad_subroutine()                      ' Noncompliant
        End Sub

        Public Function Bad_Function() As Integer ' Noncompliant
            Return 42
        End Function

        Sub GoodSubroutine()                      ' Compliant
        End Sub

        Public Function GoodFunction() As Integer ' Compliant
            Return 42
        End Function

        Sub subject__SomeEvent() Handles Obj.Ev_Event
        End Sub

        Sub subject__SomeEvent22(sender As Object, args As System.EventArgs) ' Might be event handler
        End Sub

    End Module
End Namespace