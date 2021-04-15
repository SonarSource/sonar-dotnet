Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class InvalidCases
        Public Sub Test() ' Noncompliant {{This method 'Test' has 6 lines, which is greater than the 2 lines authorized. Split it into smaller procedures.}}
'                  ^^^^
            Dim i As Integer

            i += 1
            i += 1
            i += 1
            i += 1
            i += 1
		End Sub

        Public Function MyFunc() As Integer ' Noncompliant {{This function 'MyFunc' has 7 lines, which is greater than the 2 lines authorized. Split it into smaller procedures.}}
'                       ^^^^^^
            Dim i As Integer

            i += 1
            i += 1
            i += 1
            i += 1
            i += 1

            Return 0
        End Function

        Sub New() ' Noncompliant {{This constructor has 6 lines, which is greater than the 2 lines authorized. Split it into smaller procedures.}}
'           ^^^
            Dim i As Integer

            i += 1
            i += 1
            i += 1
            i += 1
            i += 1
        End Sub

         Protected Overrides Sub Finalize() ' Noncompliant {{This finalizer has 6 lines, which is greater than the 2 lines authorized. Split it into smaller procedures.}}
'                                ^^^^^^^^
            Dim i As Integer

            i += 1
            i += 1
            i += 1
            i += 1
            i += 1
         End Sub
    End Class

    Class ValidCases
        Public Sub Foo()

        End Sub

        Public Sub Bar()
            Dim i = 0
            i += 1
        End Sub

        Public Sub ' Error [BC30203]
            Dim i As Integer

            i += 1
            i += 1
            i += 1
            i += 1
            i += 1
        End Sub
    End Class
End Namespace
