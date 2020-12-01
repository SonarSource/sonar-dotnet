Module Module1
    Sub Main(x As Boolean)
        Dim a = "a" IsNot Nothing ' Fixed
        a = "a" IsNot ' Fixed
            Nothing 'some comment
        a = "a" IsNot Nothing ' Compliant
        Main("a" IsNot Nothing) ' Fixed
    End Sub
End Module