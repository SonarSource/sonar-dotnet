Module A
    Sub DivideByZero()
        On Error GoTo nextstep ' Noncompliant {{Remove this use of 'OnError'.}}
'       ^^^^^^^^
        On Error Resume Next ' Noncompliant
        On Error GoTo - 1 ' Noncompliant
        On Error GoTo 0 ' Noncompliant
        Dim result As Integer
        Dim num As Integer
        num = 100
        result = num / 0
nextstep:
        System.Console.WriteLine("Error")
    End Sub
End Module