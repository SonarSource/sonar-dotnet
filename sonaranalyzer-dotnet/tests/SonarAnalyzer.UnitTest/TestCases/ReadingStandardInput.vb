Imports System
Imports Con = System.Console

Namespace Tests.Diagnostics
    Class Program
        Public Sub Method()
            Dim value As String
            Dim code As Integer
            Dim key As ConsoleKeyInfo

            code = System.Console.Read() ' Noncompliant {{Make sure that reading the standard input is safe here.}}
'                  ^^^^^^^^^^^^^^^^^^^^^
            code = Con.Read() ' Noncompliant

            value = Console.ReadLine() ' Noncompliant
            code = Console.Read() ' Noncompliant
            key = Console.ReadKey() ' Noncompliant
            key = Console.ReadKey(True) ' Noncompliant

            Console.Read() ' Compliant, value is ignored
            Console.ReadLine() ' Compliant, value is ignored
            Console.ReadKey() ' Compliant, value is ignored
            Console.ReadKey(True) ' Compliant, value is ignored

            Console.OpenStandardInput() ' Noncompliant
            Console.OpenStandardInput(100) ' Noncompliant

            Dim x = System.Console.[In] ' Noncompliant
'                   ^^^^^^^^^^^^^^^^^^^
            x = Console.[In] ' Noncompliant
            x = Con.[In] ' Noncompliant
            Console.[In].Read() ' Noncompliant

            ' Other Console methods
            Console.Write(1)
            Console.WriteLine(1)
            ' Other classes
            MyConsole.Read()
            MyConsole.[In].Read()
        End Sub
    End Class

    Module MyConsole
        Function Read() As Integer
            Return 1
        End Function

        Function ReadKey() As Integer
            Return 1
        End Function

        Public ReadOnly Property [In] As System.IO.TextReader
    End Module
End Namespace
