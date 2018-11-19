Imports System
Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class AnyAssemblyClass
        Public Sub New(ByVal s As String)
        End Sub
    End Class

    Public Class HardcodedIpAddress
        Private Shared Sub WriteAssemblyInfo(ByVal assemblyName As String, ByVal version As String, ByVal author As String, ByVal description As String, ByVal title As String)
        End Sub

        Public Sub Foo()
            Dim ip As String = "192.168.0.1" 'Noncompliant {{Make sure using this hardcoded IP address '192.168.0.1' is safe here.}}
'                              ^^^^^^^^^^^^^
            ip = "300.0.0.0" 'Compliant, invalid IP
            ip = "127.0.0.1" 'Compliant, exception from the rule
            ip = "    127.0.0.0    " 'Compliant
            ip = "    ""127.0.0.0""    " 'Compliant
            ip = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff" 'Noncompliant
            ip = "::/0"  'Compliant, not an IPv6 address
            ip = "::" 'Compliant, exception from the rule
            ip = "2"  'Compliant
            Dim v = New Version("127.0.0.0") 'Compliant
            Dim c = New AnyAssemblyClass("127.0.0.0") 'Compliant
            WriteAssemblyInfo("ProjectWithDependenciesWithContent", _
                "1.2.0.0", "Thomas", "Project with content", "Title of Package") 'Compliant
        End Sub
    End Class
End Namespace
