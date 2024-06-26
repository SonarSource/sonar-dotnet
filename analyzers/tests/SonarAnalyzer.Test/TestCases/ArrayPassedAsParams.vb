
Public Class Program
    Public Sub Base(ByVal myArray As String())
        Method(New String() {"s1", "s2"}) ' Noncompliant {{Remove this array creation and simply pass the elements.}}
        '      ^^^^^^^^^^^^^^^^^^^^^^^^^
        Method(New String() {"s1"}) ' Noncompliant
        Method("s1")               ' Compliant
        Method("s1", "s2")         ' Compliant
        Method(myArray)            ' Compliant
        Method(New String(11) {})  ' Compliant

        Method2(1, New String() {"s1", "s2"}) ' Noncompliant {{Remove this array creation and simply pass the elements.}}
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

        Method3(args:=New String() {"s1", "s2"}, a:=New String(11) {}) ' Error [BC30587] Named argument cannot match a ParamArray parameter
        Method3(args:=New String(11) {}, a:=New String() {"s1", "s2"}) ' Error [BC30587] Named argument cannot match a ParamArray parameter

        Dim s = New [MyClass](1, New Integer() {2, 3}) ' Noncompliant
        '                        ^^^^^^^^^^^^^^^^^^^^
        Dim s1 = New [MyClass](1, 2, 3) ' Compliant
        s1 = New [MyClass](args:=New Integer() {2, 3}, a:=1) ' Error [BC30587] Named argument cannot match a ParamArray parameter
        Dim s2 = New MyOtherClass(args:=New Integer(11) {}, a:=New Integer() {2, 3}) ' Error [BC30587] Named argument cannot match a ParamArray parameter

        Dim s3 = Prop(New String() {"s1", "s2"}) ' FN
        Dim s4 = Prop("s1", "s2") ' Compliant
    End Sub

    Public Sub Method(ParamArray args As String())
    End Sub

    Public Sub Method2(ByVal a As Integer, ParamArray args As String())
    End Sub

    Public Sub Method3(ByVal a As String(), ParamArray args As String())
    End Sub

    Public Sub Method4(ParamArray a As String(), ParamArray args As String()) 'Error [BC30192]
    End Sub

    Public ReadOnly Property Prop(ParamArray param() As String) As Integer
        Get
        End Get
    End Property

End Class

Public Class [MyClass]
    Public Sub New(ByVal a As Integer, ParamArray args As Integer())
    End Sub
End Class

Public Class MyOtherClass
    Public Sub New(ByVal a As Integer(), ParamArray args As Integer())
    End Sub
End Class

Public Class Repro6894
    'Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/6894

    Public Sub Method(ParamArray args As Object())
    End Sub

    Public Sub MethodArray(ParamArray args As Array())
    End Sub

    Public Sub MethodJaggedArray(ParamArray args As Integer()())
    End Sub

    Public Sub CallMethod()
        Method(New String() {"1", "2"})                                 ' Noncompliant, elements in args: ["1", "2"]
                                                                        ' The argument given for a parameter array can be a single expression that is implicitly convertible (§10.2) to the parameter array type.
                                                                        ' In this case, the parameter array acts precisely like a value parameter.
                                                                        ' see: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#14625-parameter-arrays
        Method({"1", "2"})                                              ' FN
        Method(New Object() {New Integer() {1, 2}})                     ' FN, elements in args: [System.Int32[]]
        Method(New Integer() {1, 2, 3})                                 ' Compliant, Elements in args: [System.Int32[]]
        Method(New String() {"1", "2"}, New String() {"1", "2"})        ' Compliant, elements in args: [System.String[], System.String[]]
        Method(New String() {"1", "2"}, New Integer() {1, 2})           ' Compliant, elements in args: [System.String[], System.Int32[]]
        MethodArray(New String() {"1", "2"}, New String() {"1", "2"})   ' Compliant, elements in args: [System.String[], System.String[]]
        MethodArray(New Integer() {1, 2}, New Integer() {1, 2})         ' Compliant, elements in args: [System.Int32[], System.Int32[]]
        MethodArray({1, 2}, {1, 2})                                     ' Compliant, elements in args: [System.Int32[], System.Int32[]]

        MethodJaggedArray(New Integer() {1, 2})                         ' Compliant: jagged array [System.Object[]]
    End Sub
End Class
