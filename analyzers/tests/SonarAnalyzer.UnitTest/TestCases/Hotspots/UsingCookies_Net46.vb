Imports System
Imports System.Collections.Specialized
Imports System.Web

Namespace Tests.Diagnostics
    Class Program
        Private field1 As HttpCookie = New HttpCookie("c") ' Compliant, value is not set
        Private field2 As HttpCookie = New HttpCookie("c", "") ' Noncompliant
        Private field3 As HttpCookie

        Private Property Property1 As HttpCookie = New HttpCookie("c") ' Compliant, value is not set
        Private Property Property2 As HttpCookie = New HttpCookie("c", "") ' Noncompliant
        Private Property Property3 As HttpCookie

        Private Sub CtorSetsValue()
            Dim cookie As HttpCookie
            cookie = New HttpCookie("c") ' Compliant, value is not set
            cookie = New HttpCookie("c", "") ' Noncompliant {{Make sure that this cookie is written safely.}}
        End Sub

        Private Sub Value_Vaues_Write()
            Dim c = New HttpCookie("c")

            c.VALUE = "" ' Noncompliant
            c.value = "" ' Noncompliant
            c.Value = "" ' Noncompliant
'           ^^^^^^^
            c("key") = "" ' Noncompliant
'           ^^^^^^^^
            c.Values("") = "" ' Noncompliant
            c.Values.Add("key", "value") ' Noncompliant
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            c.VALUES.ADD("key", "value") ' Noncompliant
            c.values.add("key", "value") ' Noncompliant
            c.values.ADD("key", "value") ' Noncompliant


            ' operations on generic NameValueCollection objects do not raise any issue
            Dim nvc = New NameValueCollection From {{"a", "1"}, {"b", "2"}}
            nvc("a") = "2"
            nvc.Add("c", "2")

            ' setting HttpCookie.Value on fields
            field1.Value = "value" ' Noncompliant
            Me.field1.Value = "value" ' Noncompliant
            field1("key") = "value" ' Noncompliant
            Me.field1("key") = "value" ' Noncompliant

            ' setting HttpCookie.Value on properties
            Property1.Value = "value" ' Noncompliant
            Me.Property1.Value = "value" ' Noncompliant
            Property1("key") = "value" ' Noncompliant
            Me.Property1("key") = "value" ' Noncompliant
        End Sub

        Private Sub Value_Values_Read(ByVal cookie As HttpCookie)
            Dim value As String
            value = cookie.Value ' Compliant
            value = cookie("") ' Compliant
            value = cookie.Values("") ' Compliant
            value = cookie.Values("") ' Compliant

            If cookie.Value <> "" Then ' Compliant
                Console.Write(cookie.Value) ' Compliant
            End If
        End Sub

        Private Sub Request_Response_Cookies(ByVal request As HttpRequest, ByVal response As HttpResponse)
            request.Cookies("").Value = "" ' Noncompliant
            request.Cookies(0).Value = "" ' Noncompliant
            request.Cookies(0)("") = "" ' Noncompliant
            request.Cookies(0).Values("") = "" ' Noncompliant

            response.Cookies("").Value = "" ' Noncompliant
            response.Cookies(0).Value = "" ' Noncompliant
            response.Cookies(0)("") = "" ' Noncompliant
            response.Cookies(0).Values("") = "" ' Noncompliant
        End Sub
    End Class
End Namespace
