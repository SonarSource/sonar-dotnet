Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace Tests.TestCases
    Class ConditionalStructureSameCondition1
        Public Property condition As Boolean
        End Property
        Public Sub Test()
            Dim b = True

            If b Then Exit Sub Else Exit Sub

            If (b AndAlso condition) Then
                ' empty
            ElseIf b AndAlso condition Then ' Noncompliant {{This branch duplicates the one on line 16.}}
                ' empty
            ElseIf b AndAlso condition Then ' Noncompliant
                ' empty
            ElseIf Not b AndAlso condition Then
                ' empty
                ' Noncompliant@+1
            ElseIf b AndAlso condition _
                Then
                ' empty
                ' Noncompliant@+1
            ElseIf Not b AndAlso _
                condition Then
                ' empty
            End If
        End Sub
    End Class
End Namespace
