Imports System

Module Module1
    Sub Main()
        Console.WriteLine("1" + ' Noncompliant
                          2 + "3") ' Noncompliant - will display "6"
        Console.WriteLine("1" + "2") ' Noncompliant {{Switch this use of the '+' operator to the '&'.}}
'                             ^
        Console.WriteLine(1 & 2)   ' Compliant - will display "12"
        Console.WriteLine(1 + 2)   ' Compliant - but will display "3"
        Console.WriteLine("1" & 2) ' Compliant - will display "12"
    End Sub
End Module
