Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

Class Testcases
    Sub Simple()
        Dim str = "hello"

        str.StartsWith("x") ' Noncompliant {{"StartsWith" overloads that take a "char" should be used}}
        '   ^^^^^^^^^^
        str.EndsWith("x") ' Noncompliant {{"EndsWith" overloads that take a "char" should be used}}
        '   ^^^^^^^^

        str.StartsWith("x"c) ' Compliant
        str.EndsWith("x"c) ' Compliant

        str.StartsWith("xx") ' Compliant (length > 1)
        str.EndsWith("xx")  ' Compliant (length > 1)

        Dim hString = "x"
        str.StartsWith(hString) ' Compliant, only raise on literals
        str.EndsWith(hString) ' Compliant, only raise on literals

        Dim hChar As Char = "x"
        str.StartsWith(hChar) ' Compliant
        str.EndsWith(hChar) ' Compliant

        Const hConst As String = "x"
        str.StartsWith(hConst) ' Compliant
        str.EndsWith(hConst) ' Compliant

        str.StartsWith(hConst.ToLower()) ' Compliant
        str.StartsWith(hConst.ToString()) ' Compliant

        str.StartsWith(hString, StringComparison.CurrentCulture) ' Compliant
        str.EndsWith(hConst, StringComparison.CurrentCulture) ' Compliant 
        str.StartsWith("x", True, CultureInfo.CurrentCulture) ' Compliant
        str.EndsWith("x", True, CultureInfo.CurrentCulture) ' Compliant

        Const five As Integer = 5
        str.StartsWith(five.ToString()) ' Compliant
        str.EndsWith(five.ToString()) ' Compliant

        Dim list = New List(Of String) From {"hey"}
        list.First().StartsWith("x") ' Noncompliant
        list.Any(Function(x) x.EndsWith("x")) ' Noncompliant
        list.Select(Function(x) x.ToString()).Last().Trim().EndsWith("x") ' Noncompliant
    End Sub

    Private Sub Chaining()
        Dim str = "hello"

        GetString().StartsWith("x") ' Noncompliant
        GetString().EndsWith("x", StringComparison.InvariantCultureIgnoreCase) ' Compliant

        MutateString(MutateString(MutateString(GetString()))).StartsWith("x") ' Noncompliant
        MutateString(MutateString(MutateString(GetString()))).EndsWith("x", True, CultureInfo.CurrentCulture) ' Compliant

        str.Trim().PadLeft(42).PadRight(42).ToLower().StartsWith("x") ' Noncompliant
        str.Trim().PadLeft(42).PadRight(42).ToLower()?.EndsWith("x") ' Noncompliant
        str.Trim().PadLeft(42).PadRight(42)?.ToLower().StartsWith("x") ' Noncompliant
        str.Trim().PadLeft(42).PadRight(42)?.ToLower()?.EndsWith("x") ' Noncompliant
        str.Trim().PadLeft(42)?.PadRight(42).ToLower().StartsWith("x") ' Noncompliant
        str.Trim().PadLeft(42)?.PadRight(42).ToLower()?.EndsWith("x") ' Noncompliant
        str.Trim().PadLeft(42)?.PadRight(42)?.ToLower().StartsWith("x") ' Noncompliant
        str.Trim().PadLeft(42)?.PadRight(42)?.ToLower()?.EndsWith("x") ' Noncompliant
        str.Trim()?.PadLeft(42).PadRight(42).ToLower().StartsWith("x") ' Noncompliant
        str.Trim()?.PadLeft(42).PadRight(42).ToLower()?.EndsWith("x") ' Noncompliant
        str.Trim()?.PadLeft(42).PadRight(42)?.ToLower().StartsWith("x") ' Noncompliant
        str.Trim()?.PadLeft(42).PadRight(42)?.ToLower()?.EndsWith("x") ' Noncompliant
        str.Trim()?.PadLeft(42)?.PadRight(42).ToLower().StartsWith("x") ' Noncompliant
        str.Trim()?.PadLeft(42)?.PadRight(42).ToLower()?.EndsWith("x") ' Noncompliant
        str.Trim()?.PadLeft(42)?.PadRight(42)?.ToLower().StartsWith("x") ' Noncompliant
        str.Trim()?.PadLeft(42)?.PadRight(42)?.ToLower()?.EndsWith("x") ' Noncompliant
        '                                                 ^^^^^^^^
    End Sub

    Private Shared Function GetString() As String
        Return "42"
    End Function

    Private Shared Function MutateString(ByVal str As String) As String
        Return "42"
    End Function

    Private Sub NotCalledOnString()
        Dim fake = New FakeString()
        fake.StartsWith("x") ' Compliant
        fake.EndsWith("x") ' Compliant
        fake.StartsWith("x"c) ' Compliant
        fake.EndsWith("x"c) ' Compliant

        fake.StartsWith() ' Compliant
        fake.EndsWith() ' Compliant
    End Sub

    Private Class FakeString
        Public Function StartsWith(ByVal value As String) As Boolean
            Return True
        End Function

        Public Function EndsWith(ByVal value As String) As Boolean
            Return False
        End Function

        Public Function StartsWith(ByVal value As Char) As Boolean
            Return True
        End Function

        Public Function EndsWith(ByVal value As Char) As Boolean
            Return False
        End Function

        Public Function StartsWith() As Boolean
            Return False
        End Function

        Public Function EndsWith() As Boolean
            Return False
        End Function
    End Class
End Class
