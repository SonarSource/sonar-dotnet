Imports System

Namespace Tests.Diagnostics
    Class Program1
        Public Shared Sub Main(ParamArray args As String()) ' Noncompliant {{Make sure that command line arguments are used safely here.}}
'                         ^^^^
            Console.WriteLine(args)
        End Sub
    End Class

    Class Program2
        Public Shared Sub Main(ParamArray args As String()) ' Compliant, args is not used
        End Sub

        Public Shared Sub Main(ByVal arg As String) ' Compliant, not a Main method
            Console.WriteLine(arg)
        End Sub

        Public Shared Sub Main(ByVal x As Integer, ParamArray args As String()) ' Compliant, not a Main method
            Console.WriteLine(args)
        End Sub
    End Class

    Class Program3
        Private Shared args As String()

        Public Shared Sub Main(ParamArray args As String()) ' Compliant, args is not used
            Console.WriteLine(Program3.args)
        End Sub
    End Class

    Class Program4
        Public Shared Function Main(ParamArray args As String()) As String ' Compliant, not a Main method
            Console.WriteLine(args)
            Return Nothing
        End Function
    End Class
End Namespace
