Imports System.Text

Public Class Program
    Public Function NotUsed(ByVal builder As StringBuilder) As StringBuilder
        Dim builder1 As StringBuilder = GetStringBuilder()  ' Compliant
        Dim builder2 = New StringBuilder()                  ' Compliant
        ExternalMethod(builder2)
        Dim builder3 As StringBuilder = New StringBuilder() ' Compliant
        builder3.ToString()
        Dim builder4 = New StringBuilder()                  ' Compliant
        Dim builder5 As StringBuilder = New StringBuilder() ' Noncompliant
        Dim builder6 = New StringBuilder()                  ' Noncompliant
        Dim builder7 As StringBuilder = New StringBuilder() ' Noncompliant
        builder7.Append(builder4.ToString())

        Dim builder8 As StringBuilder = New StringBuilder() ' FN

        If False Then
            builder8.ToString()
        End If

        Dim builder9 As StringBuilder = New StringBuilder() ' Noncompliant
        builder9.Append("Append")
        builder9.AppendLine("AppendLine")
        builder9.Replace("a", "b")
        builder9.Remove(builder9.Length - 1, 1)
        builder9.Insert(builder9.Length, "a")
        builder9.Clear()
        Dim builder10 = New StringBuilder()                 ' Compliant
        Return builder10
    End Function

    Public Function GetStringBuilder() As StringBuilder     ' Compliant
        Return New StringBuilder()
    End Function

    Public Sub ExternalMethod(ByVal builder As StringBuilder) ' Compliant
    End Sub

    Public ReadOnly Property MyProperty As String
        Get
            Dim builder1 = New StringBuilder()              ' Noncompliant
            Dim builder2 = New StringBuilder()              ' Compliant
            Return builder2.ToString()
        End Get
    End Property
End Class

