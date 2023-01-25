
Public Class Program
    Public Sub Base(ByVal myArray As String())
        Method(New String() {"s1", "s2"}) ' Noncompliant {{Arrays should not be created for ParamArray parameters.}}
        '      ^^^^^^^^^^^^^^^^^^^^^^^^^
        Method(New String() {"s1"}) ' Noncompliant
        Method("s1")               ' Compliant
        Method("s1", "s2")         ' Compliant
        Method(myArray)            ' Compliant
        Method(New String(11) {})  ' Compliant

        Method2(1, New String() {"s1", "s2"}) ' Noncompliant {{Arrays should not be created for ParamArray parameters.}}
        '          ^^^^^^^^^^^^^^^^^^^^^^^^^
        Method2(1, New String() {"s1"}) ' Noncompliant
        Method2(1, "s1")                ' Compliant
        Method2(1, "s1", "s2")          ' Compliant
        Method2(1, myArray)             ' Compliant
        Method2(1, New String(11) {})   ' Compliant

        Method3(New String() {"s1", "s2"}, "s1") ' Compliant
        Method3(New String() {"s1", "s2"}, New String(11) {}) ' Compliant
        Method3(New String() {"s1", "s2"}, New String() {"s1", "s2"}) ' Noncompliant
        '                                  ^^^^^^^^^^^^^^^^^^^^^^^^^
        Method3(Nothing, Nothing) ' Compliant

        Dim s = New [MyClass](1, New Integer() {2, 3}) ' Noncompliant
        '                        ^^^^^^^^^^^^^^^^^^^^
        Dim s1 = New [MyClass](1, 2, 3) ' Compliant
        s1 = New [MyClass](args:=New Integer() {2, 3}, a:=1) ' Error [BC30587]
        Dim s2 = New MyOtherClass(args:=New Integer(11) {}, a:=New Integer() {2, 3}) ' Error [BC30587]
    End Sub

    Public Sub Method(ParamArray args As String())
    End Sub

    Public Sub Method2(ByVal a As Integer, ParamArray args As String())
    End Sub

    Public Sub Method3(ByVal a As String(), ParamArray args As String())
    End Sub

    Public Sub Method4(ParamArray a As String(), ParamArray args As String()) 'Error [CS0231]
    End Sub

    Private a = New [MyClass](1, New Integer() {2, 3}) ' Noncompliant
    Private b = New [MyClass](1, 2, 3) ' Compliant
End Class

Public Class [MyClass]
    Public Sub New(ByVal a As Integer, ParamArray args As Integer())
    End Sub
End Class

Public Class MyOtherClass
    Public Sub New(ByVal a As Integer(), ParamArray args As Integer())
    End Sub
End Class
