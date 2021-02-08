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
            Dim ip As String = "192.168.0.1" ' Noncompliant {{Make sure using this hardcoded IP address '192.168.0.1' is safe here.}}
            '                  ^^^^^^^^^^^^^
            ip = "300.0.0.0" ' Compliant, invalid IP
            ip = "127.0.0.1" ' Compliant, exception from the rule
            ip = "    127.0.0.0    " ' Compliant
            ip = "    ""127.0.0.0""    " ' Compliant
            ip = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff" ' Noncompliant
            ip = "::/0"  ' Compliant, not an IPv6 address
            ip = "::" ' Compliant, exception from the rule
            ip = "2" ' Compliant
            ip = "::2" ' Noncompliant
            ip = "0:0:0:0:0:0:0:2" ' Noncompliant
            ip = "0:0::0:2" ' Noncompliant
            ip = "1623:0000:0000:0000:0000:0000:0000:0001" ' Noncompliant

            Dim v = New Version("127.0.0.0") ' Compliant
            Dim c = New AnyAssemblyClass("127.0.0.0") ' Compliant
            WriteAssemblyInfo("ProjectWithDependenciesWithContent", _
                "1.2.0.0", "Thomas", "Project with content", "Title of Package") ' Compliant

            Dim broadcastAddress As String = "255.255.255.255" ' Compliant
            Dim loopbackAddress1 As String = "127.0.0.1" ' Compliant
            Dim loopbackAddress2 As String = "127.2.3.4" ' Compliant
            Dim nonRoutableAddress As String = "0.0.0.0" ' Compliant
            Dim notAnIp1 As String = "0.0.0.1234" ' Compliant
            Dim country_oid As String = "2.5.6.2" ' Compliant
            Dim subschema_oid As String = "2.5.20.1" ' Compliant
            Dim not_considered_as_an_oid As String = "2.59.6.2" ' Noncompliant
            '                                        ^^^^^^^^^^

            ' IPV6 loopback
            Dim loopbackAddressV6_1 As String = "::1" ' Compliant
            Dim loopbackAddressV6_2 As String = "0:0:0:0:0:0:0:1" ' Compliant
            Dim loopbackAddressV6_3 As String = "0:0::0:1" ' Compliant
            Dim loopbackAddressV6_4 As String = "0000:0000:0000:0000:0000:0000:0000:0001" ' Compliant

            ' IPV6 non routable
            Dim nonRoutableAddressV6_1 As String = "::" ' Compliant
            Dim nonRoutableAddressV6_2 As String = "0:0:0:0:0:0:0:0" ' Compliant
            Dim nonRoutableAddressV6_3 As String = "0::0" ' Compliant
            Dim nonRoutableAddressV6_4 As String = "0000:0000:0000:0000:0000:0000:0000:0000" ' Compliant

            ' IPV6 invalid form
            Dim invalidIPv6 As String = "1::2::3" ' Compliant
            invalidIPv6 = "20015555:db8:1234:ffff:ffff:ffff:ffff:ffff" ' Compliant
            invalidIPv6 = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff:1623:2316" ' Compliant
            invalidIPv6 = ":::4" ' Compliant
        End Sub
    End Class
End Namespace
