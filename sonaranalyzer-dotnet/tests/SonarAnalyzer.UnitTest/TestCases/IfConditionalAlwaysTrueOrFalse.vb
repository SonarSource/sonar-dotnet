Public Class IfConditionalAlwaysTrueOrFalse

  Public Sub Rspec()
    If True Then ' Noncompliant
      DoSomething()
    End If

    If False Then ' Noncompliant
      DoSomethingElse()
    End If

    If 2 < 3 Then ' Noncompliant - always false
    End If

    Dim i As Integer = 0
    Dim j As Integer = 0
    j = Foo()

    If j > 0 AndAlso i > 0 Then ' Noncompliant; always false - 'i' is never set after initialization
      ' ...
    End If

    Dim b As Boolean = True
    If b OrElse Not b Then ' Noncompliant
      ' ...
    End If
  End Sub

  Public Sub DoSomething()
  End Sub
  Public Sub DoSomethingElse()
  End Sub
  Public Function Foo() As Integer
    Return 42
  End Function

End Class
