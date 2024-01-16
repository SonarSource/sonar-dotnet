Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq

Namespace Tests.Diagnostics
    Public Class VariableUnused
        Private Sub F1()
            Dim packageA = DoSomething("Foo", "1.0")
            Dim packageB = DoSomething("Qux", "1.0")
            Dim localRepository = New Cl From { packageA, packageB }
'               ^^^^^^^^^^^^^^^

            Using x = New StreamReader("")
                Dim v = 5 ' Noncompliant {{Remove the unused local variable 'v'.}}
            End Using

            Dim a As Integer ' Noncompliant
            Dim b = CType((Sub(__)
                               Dim i As Integer ' Noncompliant
                               Dim j As Integer = 42
                               Console.WriteLine("Hello, world!" & j)
                           End Sub), Action(Of Integer))
            b(5)
            Dim c As String
            c = "Hello, world!"
            Console.WriteLine(c)
            Dim d = ""
            Dim e = New List(Of String) From {
                d
            }
            Console.WriteLine(e)
        End Sub

        Private Function DoSomething(ByVal foo As String, ByVal p1 As String) As Object
            Throw New NotImplementedException()
        End Function

        Private Sub F2(ByVal a As Integer)
        End Sub
    End Class

    Friend Class Cl
        Inherits List(Of Object)
    End Class

    Public Class Lambdas
        Public Sub Foo(ByVal list As List(Of Integer))
            list.[Select](Function(item) 1)
            list.[Select](Function(item)
                              Dim value = 1 ' Noncompliant
                              Return item
                          End Function)
        End Sub
    End Class
End Namespace
