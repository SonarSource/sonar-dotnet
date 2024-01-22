Imports System.Collections.Generic
Imports System.Linq

Public Class BooleanLiteralUnnecessary

  Public Sub New(ByVal a As Boolean, ByVal b As Boolean, ByVal c As Boolean?)
    Dim condition = False
    Dim exp = True
    Dim exp2 = True

    Dim z = True OrElse ((True)) ' Noncompliant (also S1764)
    z = True Or ((True)) ' Noncompliant (also S1764)
    z = False OrElse False ' Noncompliant (also S1764)
    z = False AndAlso False ' Noncompliant (also S1764)
    z = False And False ' Noncompliant (also S1764)
    z = True AndAlso True ' Noncompliant (also S1764)
    z = True And True ' Noncompliant (also S1764)
    z = True = True ' Noncompliant (also S1764)
    z = False = False ' Noncompliant (also S1764)
    z = True <> True ' Noncompliant (also S1764)
    z = False <> False ' Noncompliant (also S1764)
    SomeFunc(True OrElse True) ' Noncompliant (also S1764)
    Dim booleanVariable = If(condition, True, True) ' compliant (also S3923)

    z = True OrElse False ' Noncompliant {{Remove the unnecessary Boolean literal(s).}}
'            ^^^^^^^^^^^^
    z = False OrElse True ' Noncompliant
    z = False Or True ' Noncompliant
    z = False AndAlso True ' Noncompliant
    z = False And True ' Noncompliant
    z = True AndAlso False ' Noncompliant
    z = False = True ' Noncompliant
'             ^^^^^^
    z = True = False ' Noncompliant
'       ^^^^^^
    z = False <> True ' Noncompliant
    z = True <> False ' Noncompliant
    Dim x = Not True ' Noncompliant
'               ^^^^
    x = True OrElse False ' Noncompliant
    x = Not False ' Noncompliant
    x = Not True ' Noncompliant
    x = (Not ((True))) ' Noncompliant
    x = (a = False) AndAlso True
'                   ^^^^^^^^^^^^
'          ^^^^^^^@-1
    x = a = True ' Noncompliant
    x = a <> False ' Noncompliant
    x = a <> True ' Noncompliant
    x = False = a ' Noncompliant
'       ^^^^^^^
    x = True = a ' Noncompliant
    x = False <> a ' Noncompliant
'       ^^^^^^^^
    x = True <> a ' Noncompliant
'       ^^^^
    x = False AndAlso Foo() ' Noncompliant
'             ^^^^^^^^^^^^^
    x = Foo() AndAlso False ' Noncompliant
'       ^^^^^^^^^^^^^
    x = Foo() And False ' Noncompliant
'       ^^^^^^^^^
    x = True AndAlso Foo() ' Noncompliant
'       ^^^^^^^^^^^^
    x = Foo() AndAlso True ' Noncompliant
'             ^^^^^^^^^^^^
    x = Foo() OrElse False ' Noncompliant
'             ^^^^^^^^^^^^
    x = Foo() Or False ' Noncompliant
'             ^^^^^^^^
    x = False OrElse Foo() ' Noncompliant
'       ^^^^^^^^^^^^
    x = Foo() OrElse True ' Noncompliant
'       ^^^^^^^^^^^^
    x = True OrElse Foo() ' Noncompliant
'            ^^^^^^^^^^^^
    x = a = True = b ' Noncompliant
'         ^^^^^^

    x = a = Foo2(((True))) ' compliant
    x = Not a ' compliant
    x = Foo() AndAlso Bar() ' compliant
    booleanVariable = If(condition, ((True)), exp) ' Noncompliant
'                                   ^^^^^^^^
    booleanVariable = If(condition, False, exp) ' Noncompliant
    booleanVariable = If(condition, exp, True) ' Noncompliant
    booleanVariable = If(condition, exp, False) ' Noncompliant
    booleanVariable = If(condition, True, False) ' Noncompliant
    booleanVariable = If(condition, exp, exp2) ' ok
    b = If(x OrElse booleanVariable, False, True) ' Noncompliant

    If c = True Then
    End If

    Dim d = If(True, c, False)
  End Sub

  Public Shared Sub SomeFunc(ByVal x As Boolean)
  End Sub

  Private Function Foo() As Boolean
    Return False
  End Function

  Private Function Foo2(ByVal a As Boolean) As Boolean
    Return a
  End Function

  Private Function Bar() As Boolean
    Return False
  End Function

  Private Sub M()
    Dim i As Integer = 0

    While True
      i += 1
    End While

    i = 0

    While False
      i += 1
    End While

    i = 0

    Dim b = True
    i = 0
    While b
      i += 1
    End While
  End Sub

  Sub Foo(exp As Boolean)
    If BooleanMethod() = True Then ' Noncompliant
'                      ^^^^^^
    End If
    If BooleanMethod() = False Then ' Noncompliant
'                      ^^^^^^^
    End If
    If BooleanMethod() OrElse False Then ' Noncompliant
'                      ^^^^^^^^^^^^
    End If

    DoSomething(Not False) ' Noncompliant
    DoSomething(BooleanMethod() = True) ' Noncompliant

    Dim booleanVariable = If(BooleanMethod(), True, False) ' Noncompliant
'                                             ^^^^^^^^^^^
    booleanVariable = If(BooleanMethod(), True, exp) ' Noncompliant
'                                         ^^^^
    booleanVariable = If(BooleanMethod(), False, exp) ' Noncompliant
'                                         ^^^^^
    booleanVariable = If(BooleanMethod(), exp, True) ' Noncompliant
'                                              ^^^^
    booleanVariable = If(BooleanMethod(), exp, False) ' Noncompliant
'                                              ^^^^^
  End Sub

  Sub Bar(exp As Boolean)
    If BooleanMethod() Then
      ' ...
    End If
    If Not BooleanMethod() Then
      ' ...
    End If
    If BooleanMethod() Then
      ' ...
    End If
    DoSomething(True)
    DoSomething(BooleanMethod())

    Dim booleanVariable = BooleanMethod()
    booleanVariable = BooleanMethod() OrElse exp
    booleanVariable = Not BooleanMethod() AndAlso exp
    booleanVariable = Not BooleanMethod() OrElse exp
    booleanVariable = BooleanMethod() AndAlso exp
  End Sub

  Sub DoSomething(ByVal foo As Boolean)
  End Sub

  Function BooleanMethod() As Boolean
        Return True
    End Function

End Class

Public Class SocketContainer
  Private sockets As IEnumerable(Of Socket)

  Public ReadOnly Property IsValid As Boolean
    Get
      Return sockets.All(Function(x) x.IsStateValid = True)
    End Get
  End Property

End Class

Public Class Socket
  Public Property IsStateValid As Boolean?
End Class

' Repro for: https://github.com/SonarSource/sonar-dotnet/issues/5219
Public Class Repro
    Private Sub Reproducers(ByVal condition As Boolean)
        Dim v1 As Boolean? = If(condition, True, Nothing)
        Dim v2 As Boolean? = If(condition, Nothing, True)
        Dim v3 As Boolean? = If(condition, True, SomeMethod())
        Dim v4 As Boolean? = If(condition, True, SomeMethod2()) ' Noncompliant
        Dim v5 As Boolean? = condition OrElse SomeMethod2()
    End Sub

    Public Function SomeMethod() As Boolean?
        Return Nothing
    End Function

    Public Function SomeMethod2() As Boolean
        Return True
    End Function
End Class

' https://github.com/SonarSource/sonar-dotnet/issues/7792
Class ObjectIsBool
    Sub Method(obj As Object, exc As Exception)
        If obj = True Then
        End If
        If exc.Data("SomeKey") = True Then
        End If
    End Sub
End Class
