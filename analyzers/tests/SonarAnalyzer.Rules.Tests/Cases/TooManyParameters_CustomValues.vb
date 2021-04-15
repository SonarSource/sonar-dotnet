Imports System
Imports System.Runtime.InteropServices

Public Class TooManyParameters
    Implements MyInterface

    Public Event GoodEvent(Sender As Object, e As EventArgs)
    Public Event Fire(Sender As Object, a As String, b As String, c As String, d As String) ' Noncompliant

    Public Sub New()
    End Sub

    Public Sub New(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer)
    End Sub

    Public Sub New(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) ' Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}
        '         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    End Sub

    Public Sub SubNoParams()
    End Sub

    Public Function FuncNoParams()
        Return 1
    End Function

    Public Sub F1(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer) Implements MyInterface.F1
    End Sub

    Public Sub F2(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) Implements MyInterface.F2 ' Compliant, interface implementation
    End Sub

    Public Sub F1(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer, ByVal p5 As String) ' Noncompliant {{Sub has 5 parameters, which is greater than the 3 authorized.}}
    End Sub

    Public Function F3(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer) Implements MyInterface.F3
        Return p1
    End Function

    Public Function F4(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) Implements MyInterface.F4 ' Compliant, interface implementation
        Return p1
    End Function

    Sub F5(ByRef p1 As Integer, ByRef p2 As Integer, ByRef p3 As Integer, ByRef p4 As Integer) Implements MyInterface.F5
    End Sub

    Function F6(ByRef p1 As Integer, ByRef p2 As Integer, ByRef p3 As Integer, ByRef p4 As Integer) Implements MyInterface.F6
        Return p1
    End Function

    Delegate Sub ThreeParametersSub(one As Integer, two As Integer, three As Integer)
    Delegate Function ThreeParametersFunction(one As Integer, two As Integer, three As Integer) As Integer
    Delegate Sub FourParametersSub(one As Integer, two As Integer, three As Integer, four As String) ' Noncompliant
    '                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    Delegate Function FourParametersFunction(one As Integer, two As Integer, three As Integer, four As String) As Integer ' Noncompliant {{Delegate has 4 parameters, which is greater than the 3 authorized.}}
    '                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    Public Sub CallFunctionDelegate(ByVal first As ThreeParametersFunction, ByVal second As FourParametersFunction)
    End Sub

    Public Sub CallSubDelegate(ByVal first As ThreeParametersSub, ByVal second As FourParametersSub)
    End Sub

    Public Sub F()
        Dim v1 = New Action(Of Integer, Integer, Integer)(
          Function(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer)
              Console.WriteLine()
          End Function)
        Dim v2 = New Action(Of Integer, Integer, Integer, Integer)(
          Function(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) ' Noncompliant {{Lambda has 4 parameters, which is greater than the 3 authorized.}}
              '   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
              Console.WriteLine()
          End Function)
        Dim v3 = New Action(Function()
                            End Function)

        Dim v4 = Function(num As Integer, num2 As Integer, num3 As Integer) num + num2 + num3 + 1
        Dim v5 = Function(num As Integer, num2 As Integer, num3 As Integer, num4 As Integer) num + num2 + num3 + num4 + 1 ' Noncompliant {{Lambda has 4 parameters, which is greater than the 3 authorized.}}
        ' the above is a FP because of below
        CallFunctionDelegate(v4, v5)

        Dim v6 = Sub(num As Integer, num2 As Integer, num3 As Integer) Console.WriteLine()
        Dim v7 = Sub(num As Integer, num2 As Integer, num3 As Integer, num4 As Integer) Console.WriteLine() ' Noncompliant (FP because of below)
        CallSubDelegate(v6, v7)

        F2(1, 2, 3, 4)
    End Sub

    Default Public ReadOnly Property Indexer(a As Integer, b As Integer, c As Integer, d As Integer) As Integer ' Noncompliant
        Get
            Return a + b + c + d + 42
        End Get
    End Property

    Public ReadOnly Property ParametrizedProperty(a As Integer, b As Integer, c As Integer, d As Integer) As Integer ' Noncompliant
        Get
            Return a + b + c + d + 42
        End Get
    End Property

    ' We should not raise for imported methods according to external definition.
    <DllImport("foo.dll")>
    Public Shared Sub Extern(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) ' Compliant, external definition
    End Sub

    Public Declare Function ExternSyntax Lib "user32.dll" (hwnd As IntPtr, a As String, b As String, c As String, d As String) As Integer  ' Compliant
End Class

Interface MyInterface

    Sub F1(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer)
    Sub F2(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer)      ' Noncompliant
    Function F3(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer)
    Function F4(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) ' Noncompliant {{Function has 4 parameters, which is greater than the 3 authorized.}}
    Sub F5(ByRef p1 As Integer, ByRef p2 As Integer, ByRef p3 As Integer, ByRef p4 As Integer)      ' Noncompliant
    Function F6(ByRef p1 As Integer, ByRef p2 As Integer, ByRef p3 As Integer, ByRef p4 As Integer) ' Noncompliant

End Interface

Public Class MyWrongClass
    Public Sub New(ByVal a As String, ByVal b As String, ByVal c As String, ByVal d As String, ByVal e As String, ByVal f As String, ByVal g As String, ByVal h As String) ' Noncompliant
    End Sub
End Class

Public Class SubClass
    Inherits MyWrongClass

    ' We should not raise when parent base class forces usage of too many args
    Public Sub New(ByVal a As String, ByVal b As String, ByVal c As String, ByVal d As String, ByVal e As String, ByVal f As String, ByVal g As String, ByVal h As String) ' Compliant (base class requires them)
        MyBase.New(a, b, c, d, e, f, g, h)
    End Sub

    Public Sub New()
        MyBase.New("", "", "", "", "", "", "", "")
    End Sub
End Class

Public Class SubClass2
    Inherits TooManyParameters

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, s1 As String, s2 As String, s3 As String) ' Compliant, base class has 3. This adds only 3 new parameters.
        MyBase.New(p1, p2, p3)
    End Sub

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, s1 As String, s2 As String, s3 As String, s4 As String) ' Noncompliant {{Constructor has 4 new parameters, which is greater than the 3 authorized.}}
        MyBase.New(p1, p2, p3)
        'For coverage, other expressions
        Dim V As Integer
        V = 0
        Method(p1, p2, p3, V)
        Me.Method(p1, p2, p3, V)
        MyBase.F4(p1, p2, p3, V)
    End Sub

    Public Sub New(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) ' Compliant (needs arguments for base constructor)
        MyBase.New(p1, p2, p3, p4)
        F5(p1, p2, p3, p4)
        F2(p1, p2, p3, p4)
    End Sub

    Private Sub Method(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer)  ' Noncompliant
    End Sub

End Class
