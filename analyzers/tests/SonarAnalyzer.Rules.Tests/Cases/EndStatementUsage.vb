Module Module1
    Sub Print(ByVal str As String)
        Try

            End        ' Error [BC30615] 'End' statement cannot be used in class library projects
'           ^^^ {{Remove this call to 'End' or ensure it is really required.}}


            Dim a As Integer

        Finally
            ' do something important here

        End Try
    End Sub
End Module
