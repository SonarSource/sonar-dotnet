Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace Tests.TestCases
    Class BinaryOperationWithIdenticalExpressions
        Public Sub doZ()
        End Sub
        Public Sub doW()
        End Sub
        Public Sub Test(a As Boolean, b As Boolean)

            If (a = a) Then ' Noncompliant {{Correct one of the identical expressions on both sides of operator '='.}}
'               ^^^^^
                doZ()
            End If

            If a = b OrElse a = b Then ' Noncompliant {{Correct one of the identical expressions on both sides of operator 'OrElse'.}}
                doW()
            End If

            Dim j = 5 / 5 ' Noncompliant
            j = 5 \ 5 ' Noncompliant
            j = 5 Mod 5 ' Noncompliant
            Dim k = 5 - 5 ' Noncompliant
            Dim l = 5 * 5


            Dim i = 1 << 1

            i -= i ' Noncompliant
            i += i
            i /= i ' Noncompliant
            i \= i ' Noncompliant
        End Sub
    End Class
End Namespace

