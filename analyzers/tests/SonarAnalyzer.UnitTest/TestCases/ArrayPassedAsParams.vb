Public Class Program

    Public Sub Base(ByVal myArray As String())
        Method(New String() {"s1", "s2"}) ' Noncompliant: unnecessary
        Method("s1")                'Compliant
        Method("s1", "s2")          'Compliant
        Method(myArray)             'Compliant
        Method(New String(12) {})   'Compliant

        Method2(1, New String() {"s1", "s2"}) ' Noncompliant: unnecessary
        Method2(1, "s1")                'Compliant
        Method2(1, "s1", "s2")          'Compliant
        Method2(1, myArray)             'Compliant
        Method2(1, New String(11) {})   'Compliant
    End Sub

    Public Sub Method(ParamArray args As String())
    End Sub

    Public Sub Method2(ByVal a As Integer, ParamArray args As String())
    End Sub

End Class
