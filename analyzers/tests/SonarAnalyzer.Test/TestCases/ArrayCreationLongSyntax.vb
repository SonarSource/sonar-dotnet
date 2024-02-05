Module Module1

    Sub Main()
        Dim foo = New String() {"a", "b", "c"} ' Noncompliant {{Use an array literal here instead.}}
        '         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        foo = New String() {} ' Compliant
        Dim foo2 = {}
        foo2 = {"a", "b", "c"}
        Dim foo3 = New A() {New B()} ' Compliant
        foo3 = New A() {New B(), New A()} ' Noncompliant

        Dim myObjects As Object() = New Object(3) {} ' Compliant

        Dim guidStrings As String
        For Each guidString As String In guidStrings.Split(New Char() {";"c}) ' Noncompliant
        Next

        myObjects = New Object() {} ' Noncompliant
        myObjects = New UnknownType() {1, 2, 3}     ' Error [BC30002]: Type 'UnknownType' is not defined
    End Sub

End Module

Class A

End Class

Class B
    Inherits A

End Class
