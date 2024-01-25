Public Class TabCharacter
    Public Sub CorrectNoncompliant()
        Dim foo = New String() {"a", "b", "c"} ' Noncompliant {{Use an array literal here instead.}}
'                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    End Sub

    Public Sub Unexpected()
        Dim foo = New String() {"a", "b", "c"}
    End Sub

    Public Sub WrongMessage()
        Dim foo = New String() {"a", "b", "c"} ' Noncompliant {{Wrong message}}
    End Sub

    Public Sub WrongStart()
        Dim foo = New String() {"a", "b", "c"} ' Noncompliant
'                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    End Sub

    Public Sub WrongLength()
        Dim foo = New String() {"a", "b", "c"} ' Noncompliant {{Use an array literal here instead.}}
'                     ^^^^^^^^^^^^^^^^^^^^^^^^^
    End Sub
End Class
