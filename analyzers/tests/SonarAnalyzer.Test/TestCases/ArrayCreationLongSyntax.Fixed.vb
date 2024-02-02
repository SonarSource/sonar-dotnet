Module Module1

    Sub Main()
        Dim foo = {"a", "b", "c"} ' Fixed
        foo = New String() {} ' Compliant
        Dim foo2 = {}
        foo2 = {"a", "b", "c"}
        Dim foo3 = New A() {New B()} ' Compliant
        foo3 = {New B(), New A()} ' Fixed

        Dim myObjects As Object() = New Object(3) {} ' Compliant

        Dim guidStrings As String
        For Each guidString As String In guidStrings.Split({";"c}) ' Fixed
        Next

        myObjects = {} ' Fixed
        myObjects = New UnknownType() {1, 2, 3}     ' Error [BC30002]: Type 'UnknownType' is not defined
    End Sub

End Module

Class A

End Class

Class B
    Inherits A

End Class
