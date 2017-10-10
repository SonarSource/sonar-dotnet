Imports System
Imports Microsoft.VisualBasic
Module Module1

    Dim Foo As Int16



    Sub Main()
        Const Constant = 0 ' Compliant
        Dim Foo = 0 ' Noncompliant
        Dim ffoo = 0 ' Compliant
        Dim fOOo42 = 0 ' Compliant

        Using Resource As New Object ' Noncompliant

        End Using

        For Each Xxxx As Object In {1, 2, 3} ' Noncompliant

        Next

        For Value As Integer = 0 To 10 ' Noncompliant

        Next

        For Index = 1 To 10 ' Noncompliant

        Next

        For Foo = 1 To 10 ' Compliant

        Next

        For Each X In {1, 2, 3} ' Noncompliant

        Next

        For Each Foo In {1, 2, 3} ' Compliant

        Next

    End Sub
End Module