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
            Dim ip As String = "192.168.0.1"                ' Noncompliant {{Make sure using this hardcoded IP address '192.168.0.1' is safe here.}}
            '                  ^^^^^^^^^^^^^
            ip = "300.0.0.0"                                ' Compliant, invalid IP
            ip = "127.0.0.1"                                ' Compliant, exception from the rule
            ip = "    127.0.0.0    "
            ip = "    ""127.0.0.0""    "
            ip = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff"   ' Compliant. Is a documentation IP.
            ip = "2001:0db8:1234:ffff:ffff:ffff:ffff:ffff"  ' Compliant. Is a documentation IP.
            ip = "2001:abcd:1234:ffff:ffff:ffff:ffff:ffff"  ' Noncompliant
            ip = "::/0"                                     ' Compliant, not an IPv6 address
            ip = "::"                                       ' Compliant, exception from the rule
            ip = "2"
            ip = "::2"                                      ' Noncompliant
            ip = "0:0:0:0:0:0:0:2"                          ' Noncompliant
            ip = "0:0::0:2"                                 ' Noncompliant
            ip = "1623:0000:0000:0000:0000:0000:0000:0001"  ' Noncompliant

            Dim v = New Version("127.0.0.0")
            Dim c = New AnyAssemblyClass("127.0.0.0")
            WriteAssemblyInfo("ProjectWithDependenciesWithContent", _
                "1.2.0.0", "Thomas", "Project with content", "Title of Package")

            Dim broadcastAddress As String = "255.255.255.255"
            Dim loopbackAddress1 = "127.0.0.1"
            Dim loopbackAddress2 = "127.2.3.4"
            Dim loopbackAddress3 = "::ffff:127.0.0.1"    ' Compliant Mapped IP4 https://www.rfc-editor.org/rfc/rfc4291.html#section-2.5.5.2
            Dim loopbackAddress4 = "::ffff:127.2.3.4"
            Dim loopbackAddress5 = "::1"                 ' Compliant IP6 loopback https://www.rfc-editor.org/rfc/rfc4291.html#section-2.5.3
            Dim loopbackAddress6 = "64:ff9b::127.2.3.4"  ' Noncompliant Translated IP4 not supported https://www.rfc-editor.org/rfc/rfc6052.html
            Dim loopbackAddress7 = "::ffff:0:127.2.3.4"  ' Noncompliant Translated IP4 not supported https://www.rfc-editor.org/rfc/rfc2765.html
            Dim nonRoutableAddress As String = "0.0.0.0"
            Dim documentationRange1 = "192.0.2.111"
            Dim documentationRange2 = "198.51.100.111"
            Dim documentationRange3 = "203.0.113.111"
            Dim notAnIp1 As String = "0.0.0.1234"
            Dim country_oid As String = "2.5.6.2"
            Dim subschema_oid As String = "2.5.20.1"
            Dim not_considered_as_an_oid As String = "2.59.6.2" ' Noncompliant
            '                                        ^^^^^^^^^^

            ' IPV6 loopback
            Dim loopbackAddressV6_1 As String = "::1"
            Dim loopbackAddressV6_2 As String = "0:0:0:0:0:0:0:1"
            Dim loopbackAddressV6_3 As String = "0:0::0:1"
            Dim loopbackAddressV6_4 As String = "0000:0000:0000:0000:0000:0000:0000:0001"

            ' IPV6 non routable
            Dim nonRoutableAddressV6_1 As String = "::"
            Dim nonRoutableAddressV6_2 As String = "0:0:0:0:0:0:0:0"
            Dim nonRoutableAddressV6_3 As String = "0::0"
            Dim nonRoutableAddressV6_4 As String = "0000:0000:0000:0000:0000:0000:0000:0000"

            ' IPV6 invalid form
            Dim invalidIPv6 As String = "1::2::3"
            invalidIPv6 = "20015555:db8:1234:ffff:ffff:ffff:ffff:ffff"
            invalidIPv6 = "2001:db8:1234:ffff:ffff:ffff:ffff:ffff:1623:2316"
            invalidIPv6 = ":::4"
        End Sub

        Public Sub StringInterpolation(ByVal unknownPart As String, ByVal knownPart As String)
            Dim part1 As String = "192"
            Dim part2 As String = "168"
            Dim part3 As String = "0"
            Dim part4 As String = "1"
            knownPart = "255"
            Dim ip1 As String = $"{part1}.{part2}.{part3}.{part4}"                           ' Noncompliant
            Dim nonIp As String = $"{part1}:{part2}"
            Dim ip2 As String = $"{part1}.{part2}.{part3}.{knownPart}"                       ' Noncompliant
            Dim ip3 As String = $"{part1}.{part2}.{part3}.{unknownPart}"
            Dim nestedConstInterpolation As String = $"{$"{part1}.{part2}"}.{part3}.{part4}" ' Noncompliant
            Dim nestedInterpolation As String = $"{$"{part1}.{knownPart}"}.{part3}.{part4}"  ' Noncompliant
        End Sub

    End Class
End Namespace
