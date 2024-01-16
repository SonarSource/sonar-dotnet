Imports System
Imports System.Diagnostics

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

      With theCustomer
      End With
'     ^^^^^^^^^^^^^^^^ Noncompliant@-1 {{Either remove or fill this block of code.}}

      With theCustomer
        ' Some comment
      End With

      With theCustomer
        .Name = "Foo"
      End With

      For i As Integer = 0 To 42 - 1
      Next
'     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant@-1 {{Either remove or fill this block of code.}}

      For i As Integer = 0 To 42 - 1  ' Noncompliant

      Next

      For i As Integer = 0 To 42 - 1
        ' Comment
      Next

      For i As Integer = 0 To 42 - 1
        i = i + 1
      Next

      Dim numbers() As Integer = {1, 4, 7}

      For Each number As Integer In numbers
      Next
'     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant@-1

      For Each number As Integer In numbers
        'Comment
      Next

      For Each number As Integer In numbers
        Console.Write(number)
      Next

      While a <= 10
      End While
'     ^^^^^^^^^^^^^ Noncompliant@-1

      While a <= 10
        a = a + 1
      End While

      Do
      Loop
'     ^^ Noncompliant@-1

      Do
        ' Stuff
      Loop

      Do
        a = a + 1
      Loop

      Do While a <= 10
      Loop
'     ^^^^^^^^^^^^^^^^ Noncompliant@-1

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

      Using reader As System.IO.TextReader = System.IO.File.OpenText("log.txt")

      End Using
'     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant@-2

      Using reader As System.IO.TextReader = System.IO.File.OpenText("log.txt")
        ' TODO
      End Using

      Using reader As System.IO.TextReader = System.IO.File.OpenText("log.txt")
        a = a + 1
      End Using

      Try ' Noncompliant
      Catch e As ArgumentException
        ' Ignore as it has this comment
      Catch e As FormatException ' Noncompliant
      Catch
      Finally
      End Try
'     ^^^^^ Noncompliant@-2
'     ^^^^^^^ Noncompliant@-2

      Try
        a = a + 1
      Finally  ' Noncompliant
      End Try

      Try ' Noncompliant
      Catch
        ' comment
      End Try

      Try
        a = a + 1
      Catch  ' Noncompliant
      Catch
        a = a + 1
      Catch  ' Noncompliant
      Catch  ' Noncompliant
      End Try

      Try
        ' nothing
      Finally
        ' nothing
      End Try

      If a Then
        ' Ignore as it has this comment
      End If

      If a Then
      End If
'     ^^^^^^^^^ Noncompliant@-1

      If a Then ' Noncompliant

      End If

      If a Then
        ' Ignore as it has this comment
      Else ' Noncompliant
      End If

      If a Then ' Noncompliant
      Else
        a = a + 1
      End If

      If a Then
        ' Ignore as it has this comment
      ElseIf a Then

      End If
'     ^^^^^^^^^^^^^ Noncompliant@-2

      If a Then ' Noncompliant
      ElseIf a Then ' Noncompliant
      ElseIf a Then ' Noncompliant
      Else ' Noncompliant
      End If

      If a Then
        ' comment
      ElseIf a Then
        ' comment
      ElseIf a Then
        a = a + 1
      Else
        ' comment
      End If

      Select Case a ' Noncompliant
      End Select

      Select Case a ' Noncompliant
        ' Comment does not matter
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

    Private Sub InnerBlocks()
      Dim a = True
      If a Then
        Select Case a ' Noncompliant
        End Select
        Try
          Do ' Noncompliant
          Loop
        Finally
          For i As Integer = 0 To 42 - 1
            Do ' Noncompliant
            Loop Until a <= 10
          Next
        End Try
      End If
    End Sub

    Public Sub ConditionalCompilation(a As Boolean)
        If a Then
#If DEBUG
            Trace.WriteLine("message")
#End If
        End If

        If a Then
#If DEBUG
#End If
        End If ' Noncompliant@-3
    End Sub

  End Class

  Public Class EmptyClass
  End Class

  Delegate Sub FooDelegate()

End Namespace
