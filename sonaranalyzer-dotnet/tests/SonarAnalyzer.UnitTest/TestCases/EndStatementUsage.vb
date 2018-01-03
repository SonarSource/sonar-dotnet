Module Module1
    Sub Print(ByVal str As String)
        Try

            End        ' Noncompliant {{Remove this call to 'End' or ensure it is really required.}}
'           ^^^
            Dim a As Integer

        Finally
            ' do something important here

        End Try
    End Sub
End Module