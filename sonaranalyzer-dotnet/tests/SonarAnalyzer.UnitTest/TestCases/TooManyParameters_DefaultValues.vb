Imports System
Imports System.Runtime.InteropServices

Public Class TooManyParameters
  Implements MyInterface

  Public Sub New(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer)
  End Sub

  Public Sub New(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer)
  End Sub

  Public Sub F1(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer) Implements MyInterface.F1
  End Sub

  Public Sub F2(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer)  Implements MyInterface.F2
  End Sub

  Public Sub F1(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer, ByVal p5 As String)
  End Sub

  Public Function F3(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer) Implements MyInterface.F3
    Return p1
  End Function

  Public Function F4(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer)  Implements MyInterface.F4
    Return p1
  End Function

  Sub F5(ByRef p1 As Integer, ByRef p2 As Integer, ByRef p3 As Integer, ByRef p4 As Integer)  Implements MyInterface.F5
  End Sub

  Function F6(ByRef p1 As Integer, ByRef p2 As Integer, ByRef p3 As Integer, ByRef p4 As Integer) Implements MyInterface.F6
    Return p1
  End Function

  Delegate Sub ThreeParametersSub( one As Integer,  two As Integer,  three As Integer)
  Delegate Function ThreeParametersFunction(one As Integer,  two As Integer,  three As Integer) As Integer
  Delegate Sub FourParametersSub( one As Integer,  two As Integer,  three As Integer,  four As String)
  Delegate Function FourParametersFunction( one As Integer,  two As Integer,  three As Integer,  four As String) As Integer

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
      Function(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer)
        Console.WriteLine()
      End Function)
    Dim v3 = New Action(Function()
      End Function)

    Dim v4 = Function(num As Integer, num2 As Integer, num3 As Integer) num + num2 + num3 + 1
    Dim v5 = Function(num As Integer, num2 As Integer, num3 As Integer, num4 As Integer) num + num2 + num3 + num4 + 1
    CallFunctionDelegate(v4, v5)
 
    Dim v6 = Sub(num As Integer, num2 As Integer, num3 As Integer) Console.WriteLine()
    Dim v7 = Sub(num As Integer, num2 As Integer, num3 As Integer, num4 As Integer) Console.WriteLine()
    CallSubDelegate(v6, v7)

    F2(1,2,3,4)
  End Sub

' We should not raise for imported methods according to external definition.
  <DllImport("foo.dll")>
  Public Shared Sub Extern(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer)
  End Sub
End Class

Interface MyInterface
  Sub F1(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer)
  Sub F2(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer)
  Function F3(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer)
  Function F4(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer)
  Sub F5(ByRef p1 As Integer, ByRef p2 As Integer, ByRef p3 As Integer, ByRef p4 As Integer)
  Function F6(ByRef p1 As Integer, ByRef p2 As Integer, ByRef p3 As Integer, ByRef p4 As Integer)
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

  Public Sub New(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal s1 As String, ByVal s2 As String)
    MyBase.New(p1, p2, p3)
  End Sub

  Public Sub New(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer)
    MyBase.New(p1, p2, p3, p4)
    F5(p1, p2, p3, p4)
    F2(p1, p2, p3, p4)
  End Sub

End Class
