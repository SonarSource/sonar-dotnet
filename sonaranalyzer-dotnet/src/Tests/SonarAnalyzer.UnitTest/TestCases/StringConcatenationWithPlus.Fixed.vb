Module Module1
    Sub Main()
        Console.WriteLine("1" & ' Fixed
                          2 & "3") ' Fixed
        Console.WriteLine("1" & "2") ' Fixed
        Console.WriteLine(1 & 2)   ' Compliant - will display "12"
        Console.WriteLine(1 + 2)   ' Compliant - but will display "3"
        Console.WriteLine("1" & 2) ' Compliant - will display "12"
    End Sub
End Module