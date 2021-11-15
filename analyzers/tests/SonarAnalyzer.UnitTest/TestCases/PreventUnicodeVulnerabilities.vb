Class Controller
    Function Access() As Boolean

        String access_level = "user";
        If (access_level! = "⁣user") Then ' Noncompliant {{Vulnerable character U+2063 detected}}
            '                ^
            Return True
        End If
        Return False
    End Function
End Class
