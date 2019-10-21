Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace Tests.Diagnostics

  Class MethodsComplexity

    Private Sub Zero()

    End Sub

    Private Sub SimpleIf() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      If True Then
'     ^^ Secondary {{+1}}
      End If
    End Sub

    Private Sub SingleLineIf() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Dim i = 1
      If True Then i = 2
'     ^^ Secondary {{+1}}
    End Sub

    Private Sub IfElseIfElse() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      If (1 = 2) Then
'     ^^ Secondary {{+1}}
      ElseIf (1 = 3) Then
'     ^^^^^^ Secondary {{+1}}
      Else
'     ^^^^ Secondary {{+1}}
      End If
    End Sub

    Private Sub SingleLineIfElse() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Dim i = 1
      If True Then i = 2 Else i = 3
'     ^^ Secondary {{+1}}
    End Sub

    Private Sub TernaryInsideSingleLineIf() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Dim i = 1
      If True Then i = If(False, 3, 2)
'     ^^ Secondary {{+1}}
'                      ^^ Secondary@-1 {{+2 (incl 1 for nesting)}}
    End Sub

    Private Sub TernaryInsideSingleLineElse() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Dim i = 1
      If True Then i = 2 Else i = If(False, 3, 2)
'     ^^ Secondary {{+1}}
'                                 ^^ Secondary@-1 {{+2 (incl 1 for nesting)}}
    End Sub

    Private Sub IfNestedInElse() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
      If True Then
'     ^^ Secondary {{+1}}
      Else
'     ^^^^ Secondary {{+1}}
        If False Then
'       ^^ Secondary {{+2 (incl 1 for nesting)}}
        End If
      End If
    End Sub

    Private Sub IfElseNestedInIf() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
      If True Then
'     ^^ Secondary {{+1}}
        If True Then
'       ^^ Secondary {{+2 (incl 1 for nesting)}}
        Else
'       ^^^^ Secondary {{+1}}
        End If
      End If
    End Sub

    Private Sub MultipleIfNested() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 6 to the 0 allowed.}}
      If True Then
'     ^^ Secondary {{+1}}
        If True Then
'       ^^ Secondary {{+2 (incl 1 for nesting)}}
          If True Then
'         ^^ Secondary {{+3 (incl 2 for nesting)}}
          End If
        End If
      End If
    End Sub

    Private Sub Switch() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Select Case (10)
'     ^^^^^^ Secondary {{+1}}
        Case 1
        Case 2
      End Select
    End Sub

    Private Sub NestedSwitch() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      If True Then
'     ^^ Secondary {{+1}}
        Select Case (10)
'       ^^^^^^ Secondary {{+2 (incl 1 for nesting)}}
          Case 1
          Case 2
        End Select
      End If
    End Sub

    Private Sub SwitchWithNestedIf() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Select Case (10)
'     ^^^^^^ Secondary {{+1}}
        Case 0
          If True Then
'         ^^ Secondary {{+2 (incl 1 for nesting)}}
          End If
      End Select
    End Sub

    Private Sub TernaryOperator() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Dim t = If(True, 0, 1)
'             ^^ Secondary {{+1}}
    End Sub

    Private Sub NestedTernaryOperator() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      If True Then
'     ^^ Secondary {{+1}}
          Dim t = If(Nothing, 1)
'                 ^^ Secondary {{+2 (incl 1 for nesting)}}
      End If
    End Sub

    Private Sub TernaryOperatorWithInnerTernayOperator() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Dim t = If(True, If(False, 3, 2), 1)
'             ^^ Secondary {{+1}}
'                      ^^ Secondary@-1 {{+2 (incl 1 for nesting)}}
    End Sub

    Private Sub Whilee() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      While True
'     ^^^^^ Secondary {{+1}}
      End While
    End Sub

    Private Sub NestedWhile() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      While True
'     ^^^^^ Secondary {{+1}}
        While True
'       ^^^^^ Secondary {{+2 (incl 1 for nesting)}}
        End While
      End While
    End Sub

    Private Sub Forr() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      For i As Integer = 1 To 10
'     ^^^ Secondary {{+1}}
      Next
    End Sub

    Private Sub NestedFor() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      For i As Integer = 1 To 10
'     ^^^ Secondary {{+1}}
        For j As Integer = 1 To 10
'       ^^^ Secondary {{+2 (incl 1 for nesting)}}
        Next
      Next
    End Sub

    Private Function Foreach() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      For Each item In Enumerable.Empty(Of Integer)
'     ^^^ Secondary {{+1}}
      Next
    End Function

    Private Function ForeachNoParens ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      For Each item In Enumerable.Empty(Of Integer)
'     ^^^ Secondary {{+1}}
      Next
    End Function

    Private Sub NestedForeach() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      For Each item1 In Enumerable.Empty(Of Integer)
'     ^^^ Secondary {{+1}}
        For Each item2 In Enumerable.Empty(Of Integer)
'       ^^^ Secondary {{+2 (incl 1 for nesting)}}
        Next
      Next
    End Sub

    Private Sub DoUntil() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Do Until True
'     ^^ Secondary {{+1}}
      Loop
    End Sub

    Private Sub NestedDoUntil() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Do Until True
'     ^^ Secondary {{+1}}
        Do Until True
'       ^^ Secondary {{+2 (incl 1 for nesting)}}
        Loop
      Loop
    End Sub

    Private Sub TryCatch() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Try
      Catch ex As Exception
'     ^^^^^ Secondary {{+1}}
        Throw
      End Try
    End Sub

    Private Sub TryCatchIf() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Try
      Catch ex  As Exception
'     ^^^^^ Secondary {{+1}}
        If True Then
'       ^^ Secondary {{+2 (incl 1 for nesting)}}
          Throw
        End If
      End Try
    End Sub

    Private Sub NestedTryCatch() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      If True Then
'     ^^ Secondary {{+1}}
        Try
        Catch ex  As Exception
'       ^^^^^ Secondary {{+2 (incl 1 for nesting)}}
            Throw
        End Try
      End If
    End Sub

    Private Sub TryCatchFinally() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Try
      Catch ex  As Exception
'     ^^^^^ Secondary {{+1}}
        Throw
      Finally
      End Try
    End Sub

    Private Sub TryFinally()
      Try
      Finally
      End Try
    End Sub

    Private Sub EmptySubBody()
    End Sub

    Private Function EmptyFunctionBody()
    End Function
  End Class

  Class PropertiesComplexity

  Private Property SimpleProperty As String
      Get
      End Get
      Set
      End Set
    End Property

    Private _foo As String

    Private Property Foo As String
      Get
        Return Me._foo
      End Get
      Set
        Me._foo = Value
      End Set
    End Property

    Private ReadOnly Property IfInProperty As String
      Get ' Noncompliant {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        If True Then
'       ^^ Secondary {{+1}}
          Return "foo"
        End If

      End Get
    End Property

    Private Property IfInPropertyGetSet As String
      Get ' Noncompliant {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        If True Then
'       ^^ Secondary {{+1}}
          Return "foo"
        End If

      End Get
      Set ' Noncompliant {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        If True Then
'       ^^ Secondary {{+1}}
          Me.foo = Value
        End If
      End Set
    End Property
  End Class

  Class EventsComplexity
    ' vb.net does not support event accessors
  End Class

  Class ConstructorsComplexity

    Private Sub New()
      MyBase.New
    End Sub

    Private Sub New(ByVal foo As String) ' Noncompliant {{Refactor this constructor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      MyBase.New
      If (foo Is Nothing) Then
'     ^^ Secondary {{+1}}
        Throw New ArgumentNullException()
      End If

    End Sub
  End Class

  Class DestructorsComplexity1
    Protected Overrides Sub Finalize()
    End Sub
  End Class
  Class DestructorsComplexity2
    Protected Overrides Sub Finalize() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      If (True) Then
'     ^^ Secondary {{+1}}
      End If
    End Sub
  End Class

  Class OperatorsComplexity1
    Public Shared Operator +(ByVal left As OperatorsComplexity1, ByVal right As OperatorsComplexity1) As OperatorsComplexity1
      Return Nothing
    End Operator
  End Class
  Class OperatorsComplexity2
    Public Shared Operator +(ByVal left As OperatorsComplexity2, ByVal right As OperatorsComplexity2) As OperatorsComplexity2 ' Noncompliant {{Refactor this operator to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      If (True) Then
'     ^^ Secondary {{+1}}
      End If
      Return Nothing
    End Operator

  End Class

  Class RecursionsComplexity

    Private Sub DirectRecursionComplexityArg(ByVal arg As Integer)
'               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
'               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary@-1 {{+1 (recursion)}}
      DirectRecursionComplexityArg(arg)
    End Sub

    Private Sub DirectRecursionComplexityMe()
'               ^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
'               ^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary@-1 {{+1 (recursion)}}
      Me.DirectRecursionComplexityMe()
    End Sub

    Private Overloads Sub DirectRecursionComplexity_DifferentArguments()
      DirectRecursionComplexity_DifferentArguments(1)
      ' This is not recursion, no complexity increase
    End Sub

    Private Overloads Sub DirectRecursionComplexity_DifferentArguments(ByVal arg As Integer)
    End Sub

    Private Sub IndirectRecursionComplexity() ' FN
      TmpIndirectRecursion()
      GetMe().IndirectRecursionComplexity()
    End Sub

    Private Sub TmpIndirectRecursion()
      IndirectRecursionComplexity()
    End Sub

    Private Function GetMe() As RecursionsComplexity
      Return Me
    End Function

    Private Function IndirectRecursionFromLocalLambda()
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary@-1 {{+1 (recursion)}}
      Dim act = Function() IndirectRecursionFromLocalLambda()
      act
    End Function
  End Class


  Class AndOrConditionsComplexity

    Private Sub Simple()
      Dim a = True
    End Sub

    Private Sub SimpleAnd() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Dim a = True And False
'                  ^^^ Secondary {{+1}}
    End Sub

    Private Sub SimpleAndAlso() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Dim a = True AndAlso False
'                  ^^^^^^^ Secondary {{+1}}
    End Sub

    Private Sub SimpleOr() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Dim a = (True Or False)
'                   ^^ Secondary {{+1}}
    End Sub

    Private Sub SimpleOrElse() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Dim a = (True OrElse False)
'                   ^^^^^^ Secondary {{+1}}
    End Sub

    Private Sub SimpleNot()
      Dim a = Not True
    End Sub

    Private Sub AndOr() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
      Dim a = (True And False) Or True
'                   ^^^ Secondary {{+1}}
'                              ^^ Secondary@-1 {{+1}}
    End Sub

    Private Sub AndAlsoOrElse() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
      Dim a = (True AndAlso False) OrElse True
'                   ^^^^^^^ Secondary {{+1}}
'                                  ^^^^^^ Secondary@-1 {{+1}}
    End Sub

    Private Sub AndOrIf() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
      Dim a, b, c, d, e, f As Boolean
      If (a And b And c Or d Or e And f) Then
'     ^^ Secondary {{+1}}
'           ^^^ Secondary@-1 {{+1}}
'                       ^^ Secondary@-2 {{+1}}
'                                 ^^^ Secondary@-3 {{+1}}
      End If
    End Sub

    Private Sub AndAlsoOrElseIf() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
      Dim a, b, c, d, e, f As Boolean
      If (a AndAlso b AndAlso c OrElse d OrElse e AndAlso f) Then
'     ^^ Secondary {{+1}}
'           ^^^^^^^ Secondary@-1 {{+1}}
'                               ^^^^^^ Secondary@-2 {{+1}}
'                                                 ^^^^^^^ Secondary@-3 {{+1}}
      End If
    End Sub

    Private Sub AndNot() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
      Dim a, b, c, d As Boolean
      Dim res = a And Not (b And c) And d
'                 ^^^ Secondary {{+1}}
'                            ^^^ Secondary@-1 {{+1}}
    End Sub

    Private Sub AndAlsoNot() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
      Dim a, b, c, d As Boolean
      Dim res = a AndAlso Not (b AndAlso c) AndAlso d
'                 ^^^^^^^ Secondary {{+1}}
'                                ^^^^^^^ Secondary@-1 {{+1}}
    End Sub

    Private Sub OrAndNotOr() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Dim a, b, c, d As Boolean
      Dim res = d Or a And (Not b Or Not c)
'                 ^^ Secondary {{+1}}
'                      ^^^ Secondary@-1 {{+1}}
'                                 ^^ Secondary@-2 {{+1}}
    End Sub

    Private Sub OrElseAndNot() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Dim a, b, c, d As Boolean
      Dim res = d OrElse a And (Not b OrElse Not c)
'                 ^^^^^^ Secondary {{+1}}
'                          ^^^ Secondary@-1 {{+1}}
'                                     ^^^^^^ Secondary@-2 {{+1}}
    End Sub

    Private Sub AndOrNot2() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Dim a, b, c, d As Boolean
      Dim res = a And (Not b Or Not c) Or d
'                 ^^^ Secondary {{+1}}
'                            ^^ Secondary@-1 {{+1}}
'                                      ^^ Secondary@-2 {{+1}}
    End Sub

    Private Sub AndNot3() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
      Dim a, b, c, d As Boolean
      Dim res = a And d And Not (b And c)
'                 ^^^ Secondary {{+1}}
'                                  ^^^ Secondary@-1 {{+1}}
    End Sub

    Private Sub AndNotParenthesis() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
      Dim a, b, c As Boolean
      Dim res = a And Not (((b And c)))
'                 ^^^ Secondary {{+1}}
'                              ^^^ Secondary@-1 {{+1}}
    End Sub

    Private Sub ChainedConditionsWithParentheses() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      Dim a, b, c, d As Boolean
      Dim res = a AndAlso b AndAlso (c AndAlso d)
'                 ^^^^^^^ Secondary {{+1}}
    End Sub

  End Class

  Class GotoComplexity
    Private Sub Foo() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
      GoTo Outer
'     ^^^^ Secondary {{+1}}
  Outer:
      Console.WriteLine()
    End Sub

    Private Sub Bar() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
      Select Case (5)
'     ^^^^^^ Secondary {{+1}}
        Case 1000
          GoTo Inner
'         ^^^^ Secondary {{+2 (incl 1 for nesting)}}
        Case 100
          Inner:
      End Select

    End Sub
  End Class

  Class LambdasComplexity

    Private act1 As Action(Of Integer) = Function(x As Integer) ' Noncompliant {{Refactor this field to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
                                            If (x > 5)
'                                           ^^ Secondary {{+2 (incl 1 for nesting)}}
                                            End If
                                        End Function

    Private act2 As Func(Of Integer, String) = Function(x As Integer) ' Noncompliant {{Refactor this field to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
                                                If (x > 5)
'                                               ^^ Secondary {{+2 (incl 1 for nesting)}}
                                                End If
                                                Return ""
                                            End Function

    Private Sub SimpleFunc()
      Dim func = Function(num As Integer) num + 1
    End Sub

    Private Sub BlockFun()
      Dim func As Func(Of Integer, String) = Function(x As Integer)
                                              Return ""
                                            End Function
    End Sub

    Private Sub IfFunc() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
      Dim func As Func(Of Integer, String) = Function(x As Integer)
                                                  If (x > 0) Then
'                                                 ^^ Secondary {{+2 (incl 1 for nesting)}}
                                                    Return ""
                                                  End If
                                                  Return ""
                                             End Function
    End Sub

    Private Sub SimpleAction()
      Dim act = Sub(x) Console.WriteLine(x)
    End Sub

    Private Sub BlockAction()
      Dim act As Action(Of String) = Sub(x)
                                          Console.Write(x)
                                     End Sub
    End Sub

    Private Sub IfAction() ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
      Dim func As Action(Of Integer) = Sub(x As Integer)
                                          If (x > 0) Then
'                                         ^^ Secondary {{+2 (incl 1 for nesting)}}
                                              Console.Write(x)
                                          End If
                                       End Sub
    End Sub


    Private Sub IfActionNoParens ' Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
      Dim func As Action(Of Integer) = Sub(x As Integer)
                                          If (x > 0) Then
'                                         ^^ Secondary {{+2 (incl 1 for nesting)}}
                                              Console.Write(x)
                                          End If
                                       End Sub
    End Sub


  End Class

End Namespace
