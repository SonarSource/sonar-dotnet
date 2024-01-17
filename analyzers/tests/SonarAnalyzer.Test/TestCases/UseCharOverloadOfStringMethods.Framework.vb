Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

Class Testcases

    Sub Simple()
        Dim str = "hello"

        ' "char" overloads do not exist on .NET Framework
        str.StartsWith("x") ' Compliant
        str.EndsWith("x") ' Compliant

        str.StartsWith("x"c) ' Compliant
        str.EndsWith("x"c) ' Compliant
    End Sub

End Class
