Imports System

Namespace Tests.Diagnostics

  Public Class Customer
    Public Property Name As String
  End Class

  Public Class EmptyNestedBlock

    Public Sub New()
    End Sub

    Protected Overrides Sub Finalize()
    End Sub

    Private Sub F1(ByVal b As Boolean)

      Dim a = b
      Dim theCustomer As New Customer

      With theCustomer ' Noncompliant
      End With
      With theCustomer
        ' Some comment
      End With
      With theCustomer
        .Name = "Foo"
      End With

      For i As Integer = 0 To 42 - 1
      Next ' Noncompliant {{Either remove or fill this block of code.}}

      For i As Integer = 0 To 42 - 1

      Next ' Noncompliant {{Either remove or fill this block of code.}}

      For i As Integer = 0 To 42 - 1
        ' Comment
      Next

      For i As Integer = 0 To 42 - 1
        i = i + 1
      Next

      Dim numbers() As Integer = {1, 4, 7}

      For Each number As Integer In numbers ' Noncompliant
      Next

      For Each number As Integer In numbers
        'Comment
      Next

      For Each number As Integer In numbers
        Console.Write(number)
      Next

      While a <= 10 ' Noncompliant
      End While

      While a <= 10
        a = a + 1
      End While

      Do While a <= 10 ' Noncompliant
      Loop

      Do While a <= 10
        a = a + 1
      Loop

      Do Until a <= 10 ' Noncompliant
      Loop

      Do Until a <= 10
        ' Comment
      Loop

      Do ' Noncompliant
      Loop While a <= 10

      Do
        a = a + 1
      Loop While a <= 10

      Do ' Noncompliant
      Loop Until a <= 10

      Do
        a = a + 1
      Loop Until a <= 10

      Using reader As System.IO.TextReader = System.IO.File.OpenText("log.txt") ' Noncompliant

      End Using

      Using reader As System.IO.TextReader = System.IO.File.OpenText("log.txt") ' Noncompliant
        ' TODO
      End Using

      Try
      Catch e As ArgumentException
        ' Ignore as it has this comment
      Catch e As FormatException ' Noncompliant
      Catch ' Noncompliant
      Finally  ' Noncompliant
      End Try

      If a Then
        ' Ignore as it has this comment
      End If

      Select Case a ' Noncompliant
      End Select

      Select Case a
        Case True
        Case Else
      End Select

      Dim foo1 = New Action(Of Object)(Sub(x)
                                       End Sub)
      Dim foo2 = Sub() Console.WriteLine()
      Dim foo3 = New Action(Sub()
                            End Sub)
      Dim foo4 = Function(num As Integer) num
    End Sub

    Private Sub F2()
    End Sub

  End Class

  Public Class EmptyClass
  End Class

  Delegate Sub FooDelegate()

End Namespace
