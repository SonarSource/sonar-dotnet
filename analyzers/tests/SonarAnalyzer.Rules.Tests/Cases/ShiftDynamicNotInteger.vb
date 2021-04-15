Option Strict Off
Namespace Tests.Diagnostics

    Public Class ShiftDynamicNotInteger
        Public Sub Test()
            Dim o As Object = 5

            Dim x as Integer

            ' Object cannot be shifted
            x = o >> 5 ' Noncompliant
            x = o << 5 ' Noncompliant

            x = Nothing >> 5 ' Compliant, returns 0
            x = Nothing << 5 ' Compliant, returns 0

            x = x >> Nothing ' Compliant, returns x
            x = x << Nothing ' Compliant, returns x

            ' Object cannot be used as shift parameter
            x = x >> o ' Noncompliant
            x = x << o ' Noncompliant
            x <<= o ' Noncompliant {{Remove this erroneous shift, it will fail because 'Object' can't be implicitly converted to 'int'.}}
            x = o << o ' Noncompliant

            ' Compliant cases
            x = x >> 2
            x = x << 2
        End Sub
    End Class

End Namespace
