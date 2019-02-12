Public Class IfConditionalAlwaysTrueOrFalse

  Public Sub Rspec()
    If True Then ' Noncompliant {{Remove this useless 'If' statement.}}
'   ^^^^^^^^^^^^
      DoSomething()
    Else ' Noncompliant {{Remove this useless 'Else' clause.}}
    End If

    If False Then ' Noncompliant
'   ^^^^^^^^^^^^^
      DoSomethingElse()
    End If

    If 2 < 3 Then ' FN
    End If

    Dim i As Integer = 0
    Dim j As Integer = 0
    j = Foo()

    If j > 0 AndAlso i > 0 Then ' FN
      ' ...
    End If

    Dim b As Boolean = True
    If b OrElse Not b Then ' FN
      ' ...
    End If

    If j > 1 AndAlso True Then
    End If

    If j > 1 OrElse True Then ' FN
    End If

    If j > 1 AndAlso False Then ' FN
    End If

    If j > 1 OrElse True Then
    End If

    If True Then ' Noncompliant
      DoSomething()
    ElseIf True Then ' Noncompliant {{Remove this useless 'ElseIf' clause.}}
      DoSomething()
    ElseIf False Then ' Noncompliant {{Remove this useless 'ElseIf' clause.}}
      DoSomething()
    Else ' Noncompliant {{Remove this useless 'Else' clause.}}
      DoSomething()
    End If

    If False Then ' Noncompliant
      DoSomethingElse()
    ElseIf True Then
      DoSomething()
    Else
      DoSomething()
    End If

  End Sub

  Public Sub DoSomething()
  End Sub

  Public Sub DoSomethingElse()
  End Sub

  Public Function Foo() As Integer
    Return 42
  End Function

  Public Sub New(ByVal a As Boolean, ByVal b As Boolean)
    Dim someWronglyFormatted = 45

    If a = b Then
      DoSomething()
    End If

    If True = b Then
      DoSomethingElse()
    End If

    If a Then
      DoSomething()
    End If
  End Sub

End Class
