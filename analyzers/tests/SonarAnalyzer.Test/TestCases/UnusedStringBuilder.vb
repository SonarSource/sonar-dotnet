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
        Dim builder8 As StringBuilder = New StringBuilder() ' Compliant
        builder8.Append("&").ToString()

        Dim builderInLine1 As StringBuilder = New StringBuilder(), builderInLine2 As StringBuilder = New StringBuilder()
        '   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^                                                           Noncompliant
        '                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    Noncompliant@-1

        Dim sb1, sb2 ' Compliant
        Dim sb3, sb4 As StringBuilder = New StringBuilder() ' Error [BC30671]: Explicit initialization is not permitted with multiple variables declared with a single type specifier.

        Dim sb5 As StringBuilder = New StringBuilder() ' Compliant
        Dim sb6 As StringBuilder = New StringBuilder() ' Noncompliant
        sb6 = sb5

        Dim builderCfg As StringBuilder = New StringBuilder() ' FN (requires use of cfg with significant impact on performance)
        If False Then
            builderCfg.ToString()
        End If

        Dim builderCalls As StringBuilder = New StringBuilder() ' Noncompliant
        builderCalls.Append("Append")
        builderCalls.AppendLine("AppendLine")
        builderCalls.Replace("a", "b")
        builderCalls.Clear()

        Dim builderCalls2 As StringBuilder = New StringBuilder() ' Compliant
        builderCalls2.Remove(builderCalls2.Length - 1, 1)

        Dim mySbField As StringBuilder = New StringBuilder()    ' Noncompliant
        Dim [myClass] As [MyClass] = New [MyClass]()
        [myClass].mySbField.ToString()

        Dim builderReturn = New StringBuilder()                 ' Compliant
        Return builderReturn
    End Function

    Public Function GetStringBuilder() As StringBuilder     ' Compliant
        Return New StringBuilder()
    End Function

    Public Sub ExternalMethod(ByVal builder As StringBuilder) ' Compliant
    End Sub

    Public Function AnotherMethod() As String
        Dim builder = New StringBuilder() ' Compliant
        Return $"{builder} is ToStringed here"
    End Function

    Public ReadOnly Property MyProperty As String
        Get
            Dim builder1 = New StringBuilder()              ' Noncompliant
            Dim builder2 = New StringBuilder()              ' Compliant
            Return builder2.ToString()
        End Get
    End Property

    Private myField As StringBuilder = New StringBuilder() ' Compliant
End Class

Public Class [MyClass]
    Public mySbField As StringBuilder = New StringBuilder()
End Class

' https://github.com/SonarSource/sonar-dotnet/issues/8809
Public Class GH8809
    Public Function ToString() As String
        Dim builder As New StringBuilder() ' FN
        builder.AppendLine("value")

        Return String.Empty
    End Function
End Class
