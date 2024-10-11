
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace Tests.TestCases
    Class SelfAssignment
        Sub New()
        End Sub

        Sub New(Prop1 As Integer)
        End Sub

        Public Property Prop1 As Integer

        Public Sub Test()
            Prop1 = 5
            Prop1 = Prop1 ' Noncompliant
'           ^^^^^^^^^^^^^
            Prop1 = 2 * Prop1

            Dim y = 5
            y = y ' Noncompliant {{Remove or correct this useless self-assignment.}}
            Dim x = New SelfAssignment() With { .Prop1 = Prop1 }
            x = New SelfAssignment(Prop1 := Prop1)
            Dim z = New With { _
                .Prop1 = Prop1 _
            }
        End Sub
    End Class

    ' Repro for https://github.com/SonarSource/sonar-dotnet/issues/9667
    Public Class Sample

        Public First, Second As String

        Public Sub New(First As String)
            Me.First = First    ' Compliant
            Second = Second     ' Noncompliant
            Me.Second = Second  ' False Negative
            Second = Me.Second  ' False Negative
        End Sub

    End Class

End Namespace
