Imports System
Imports System.Xml.Linq

Module Module1
    Sub Main()
        ' Fixed
        Console.WriteLine("1" &
                          2 & "3") ' Fixed
        Console.WriteLine("1" & "2") ' Fixed
        Console.WriteLine(1 & 2)   ' Compliant - will display "12"
        Console.WriteLine(1 + 2)   ' Compliant - will display "3"
        Console.WriteLine("1" & 2) ' Compliant - will display "12"
    End Sub
End Module

' https://github.com/SonarSource/sonar-dotnet/issues/4346
Public Class Repro_4346

    Public Sub XmlLinqXNamespace(xE As XElement, xNS As XNamespace, Name As String)
        Dim xN1 As XName = xNS + "Name" ' Compliant, there's no &Operator for XNamespace and string
        Dim xN2 As XName = xNS + Name
        xE.Element(xNS + "Name")
        xE.Element(xNS + Name)
        xE.Element("Prefix" & Name)
        xE.Element("Prefix" & Name)     ' Fixed
    End Sub

    Public Sub CustomOperator(Arg As Repro_4346, Name As String)
        Dim S As String = Arg + Name    ' +Operator exists for this type so there's a reason for it
        S = Arg & Name  ' Error [BC30452] Operator '&' is not defined for types 'Repro' and 'String'
    End Sub

    Public Shared Operator +(A As Repro_4346, B As String) As String
        Return A.ToString & B
    End Operator

End Class
