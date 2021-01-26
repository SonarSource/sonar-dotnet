Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace TestCasesForRuleFailure

    Public Sub Method2(i As Integer, j As Integer)
    End Sub

    Public Class DuplicatedNotImplementedInterfaces
        Implements IList
        Implements IList

    End Class

    Class InvalidSyntax

        Public Sub Method(i As Integer, j As Integer,)
            Dim x =
            For i as integer = 0 to 5
        End Sub

        Public Sub InvalidChars()
            ;
            {
            }
        End Sub

        Private Class C
            Public New
        End Class

    End Class

    Class

        i as integer

    End Class

    Public Class NoSubName

        Public Sub
        End Sub

    End Class

End Namespace

