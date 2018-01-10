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
    End Class
End Namespace

