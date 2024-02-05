Module Module1
    Sub Print(ByVal str As String)
        Try

            ' Error@+1 [BC30615] 'End' statement cannot be used in class library projects
            End ' Noncompliant{{Remove this call to 'End' or ensure it is really required.}}
'           ^^^


            Dim a As Integer

        Finally
            ' do something important here

        End Try
    End Sub
End Module
