Imports System
Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Class UnaryPrefixOperatorRepeated
        Private Shared Sub NonComp(ByVal bbb As Boolean)
            Dim i As Integer = 1
            Dim k As Integer = Not Not i 'Noncompliant {{Use the 'Not' operator just once or not at all.}}
'                              ^^^^^^^
            Dim m As Integer = + +i 'Compliant, we care only about Not
            Dim n As Integer = - -i 'Compliant, we care only about Not
            Dim b As Boolean = False
            Dim c As Boolean = Not Not Not b 'Noncompliant
            NonComp(Not Not Not b) 'Noncompliant
        End Sub

        Private Shared Sub Comp()
            Dim i As Integer = 1
            Dim j As Integer = -i
            j = -(-i) 'Compliant
            Dim k As Integer = i
            Dim m As Integer = i
            Dim b As Boolean = False
            Dim c As Boolean = Not b
        End Sub
    End Class
End Namespace
