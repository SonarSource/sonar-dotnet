Imports System
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
            cookie = New HttpCookie("c", "") ' Noncompliant {{Make sure that this cookie is used safely.}}
        End Sub

        Private Sub Value_Vaues_Write()
            Dim c = New HttpCookie("c")

            c.Value = "" ' Noncompliant
'           ^^^^^^^
            c("key") = "" ' Noncompliant
'           ^^^^^^^^
            c.Values("") = "" ' Noncompliant
            c.Values.Add("key", "value") ' Noncompliant
'           ^^^^^^^^

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
            value = cookie.Value ' Noncompliant
            value = cookie("") ' Noncompliant
            value = cookie.Values("") ' Noncompliant
            value = cookie.Values("") ' Noncompliant

            If cookie.Value <> "" Then ' Noncompliant
                Console.Write(cookie.Value) ' Noncompliant
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
