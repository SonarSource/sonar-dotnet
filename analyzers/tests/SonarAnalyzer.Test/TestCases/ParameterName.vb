Module Module1

    Sub GetSomething(ByVal ID As Integer) ' Noncompliant
        '                  ^^
    End Sub

    Sub GetSomething2(ByVal id As Integer, Name As String) ' Noncompliant
        '                                  ^^^^
    End Sub

    Dim array As String()

    ReadOnly Property Foo(ByVal Index As Integer)  ' Noncompliant
        '                       ^^^^^
        Get
            Return array(Index)
        End Get
    End Property

End Module
