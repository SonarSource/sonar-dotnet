Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class StringConcatenationInLoop

        Public Sub New()
            Dim str = "	"
            Dim str2 = "	"
            Dim i = 1
            Do
                str += "a"           ' Noncompliant {{Use a StringBuilder instead.}}
'               ^^^^^^^^^^
                str = str + "a" + "c" ' Noncompliant

                str &= "a"           ' Noncompliant
                str = str & "a"      ' Noncompliant
                str = str2 & "a"     ' Compliant
                i += 5
            Loop

            str = str & "a"
        End Sub

        Public Sub MarkDisabled(objects as IList(Of MyObject))
            For Each obj As MyObject In objects
                obj.Name += " - DISABLED" ' Noncompliant, FP See: https://github.com/SonarSource/sonar-dotnet/issues/5521
            Next
        End Sub

    End Class

    Public Class MyObject

        Private _name As String

        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                _name = value
            End Set
        End Property

    End Class
End Namespace

