Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class StringConcatenationInLoop
        Public Sub New(ByVal objects As IList(Of MyObject), ByVal p As String)
            Dim s = ""
            Dim t = 0
            Dim dict As Dictionary(Of String, String) = New Dictionary(Of String, String)()

            For i = 0 To 49
                Dim sLoop = ""

                s = s & "a" & "b"  ' Noncompliant {{Use a StringBuilder instead.}}
'               ^^^^^^^^^^^^^^^^^
                s += "a"     ' Noncompliant
'               ^^^^^^^^

                s = s & i.ToString() ' Noncompliant
                s += i.ToString() ' Noncompliant
                s += "a" & s ' Noncompliant
                s += String.Format("{0} world;", "Hello") ' Noncompliant

                dict("a") = dict("a") & "a" ' FN
                i = i + 1
                i += 1
                t = t + 1
                t += 1
                sLoop = sLoop & "a"
                sLoop += "a"
            Next

            While True
                Dim sLoop = ""

                s = s & "a" ' Noncompliant
                s += "a" ' Noncompliant
                sLoop = s & "a" ' Compliant
                sLoop += s & "a" ' Compliant

                ' See https://github.com/SonarSource/sonar-dotnet/issues/1138
                s = If(s, "b")
            End While

            For Each o In objects
                Dim sLoop = ""

                s = s & "a" ' Noncompliant
                s += "a" ' Noncompliant
                sLoop = s & "a" ' Compliant
                sLoop += s & "a" ' Compliant
            Next

            Do
                Dim sLoop = ""

                s = s & "a" ' Noncompliant
                s += "a" ' Noncompliant
                sLoop = s & "a" ' Compliant
                sLoop += s & "a" ' Compliant
            Loop While True

            s = s & "a" ' Compliant
            s += "a" ' Compliant

            p = p & "a" ' Compliant
            p += "a" ' Compliant

            Dim l = ""
            l = l & "a" ' Compliant
            l += "a" ' Compliant
        End Sub

        ' https://github.com/SonarSource/sonar-dotnet/issues/5521
        Private Sub Repro_5521(ByVal objects As IList(Of MyObject))
            For Each obj In objects
                obj.Name += "a" ' Compliant
                obj.Name = obj.Name & "a" ' Compliant
            Next
        End Sub

        ' https://github.com/SonarSource/sonar-dotnet/issues/7713
        Private Sub Repro_7713()
            Dim s = ""

            While True
                s = "a" & "b" & "c" & s ' Noncompliant
                s = "a" & "b" & s ' Noncompliant
                s = "a" & s ' Noncompliant
            End While
        End Sub
    End Class

    Public Class MyObject
        Public Property Name As String
    End Class
End Namespace
