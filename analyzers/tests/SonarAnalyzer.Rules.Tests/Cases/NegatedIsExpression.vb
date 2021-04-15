Module Module1
    Sub Main(x As Boolean)
        Dim a = Not "a" Is Nothing ' Noncompliant {{Replace this use of 'Not...Is...' with 'IsNot'.}}
'               ^^^^^^^^^^^^^^^^^^
        a = Not "a" Is ' Noncompliant
            Nothing 'some comment
        a = "a" IsNot Nothing ' Compliant
        Main(Not "a" Is Nothing) ' Noncompliant
    End Sub
End Module