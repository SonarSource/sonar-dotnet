Imports System
Imports System.Xml.Linq

Module Module1
    Sub Main()
        Console.WriteLine("1" + ' Noncompliant
                          2 + "3") ' Noncompliant - will display "6"
        Console.WriteLine("1" + "2") ' Noncompliant {{Switch this use of the '+' operator to the '&'.}}
'                             ^
        Console.WriteLine(1 & 2)   ' Compliant - will display "12"
        Console.WriteLine(1 + 2)   ' Compliant - will display "3"
        Console.WriteLine("1" & 2) ' Compliant - will display "12"
    End Sub
End Module

' https://github.com/SonarSource/sonar-dotnet/issues/4346
Public Class Repro_4346

    Public Sub XmlLinqXNamespace(xE As XElement, xNS As XNamespace, Name As String)
        Dim xN1 As XName = xNS + "Name" ' Noncompliant FP
        Dim xN2 As XName = xNS + Name   ' Noncompliant FP
        xE.Element(xNS + "Name")        ' Noncompliant FP
        xE.Element(xNS + Name)          ' Noncompliant FP
        xE.Element("Prefix" & Name)
        xE.Element("Prefix" + Name)     ' Noncompliant
    End Sub

    Public Sub CustomOperator(Arg As Repro_4346, Name As String)
        Dim S As String = Arg + Name    ' Noncompliant FP
        S = Arg & Name  ' Error [BC30452] Operator '&' is not defined for types 'Repro' and 'String'
    End Sub

    Public Shared Operator +(A As Repro_4346, B As String) As String
        Return A.ToString & B
    End Operator

End Class
