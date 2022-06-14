Public Class Noncompliant
    Private ReadOnly collection As List(Of Integer)

    Public Overrides Function ToString() As String
        If collection.Count = 0 Then
            Return Nothing ' Noncompliant
'           ^^^^^^^^^^^^^^
        Else
            ' ..
        End If

        Return Nothing ' Noncompliant {{Return empty string instead.}}
    End Function
End Class

Public Class Compliant
    Private ReadOnly collection As List(Of Integer)

    Public Function AnyOther() As String
        Return Nothing
    End Function

    Public Overrides Function ToString() As String
        If collection.Count = 0 Then
            Return String.Empty
        Else
            ' ..
        End If

        Return ""
    End Function
End Class
