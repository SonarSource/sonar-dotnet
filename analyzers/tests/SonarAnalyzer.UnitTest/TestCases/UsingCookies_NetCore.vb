Imports System
Imports Microsoft.AspNetCore.Http

Namespace Tests.Diagnostics
    Class Program
        Private Sub Responses(ByVal response As HttpResponse)
            Dim value = ""
            ' Response headers
            response.Headers.Add("Set-Cookie", "") ' Noncompliant
            response.Headers("Set-Cookie") = "" ' Noncompliant
            value = response.Headers("Set-Cookie") ' Compliant

            ' Not the Set-Cookie header
            response.Headers.Add("something", "")
            response.Headers("something") = value
            value = response.Headers("something")

            ' Response headers as variable
            Dim responseHeaders = response.Headers
            responseHeaders.Add("Set-Cookie", "") ' Noncompliant
            responseHeaders("Set-Cookie") = "" ' Noncompliant
            value = responseHeaders("Set-Cookie") ' Compliant

            responseHeaders.Remove("Set-Cookie") ' Compliant
            responseHeaders.Remove("") ' Compliant

            ' Response headers as variable
            response.Cookies.Append("", "") ' Noncompliant
            response.Cookies.append("", "") ' Noncompliant
            response.Cookies.APPEND("", "") ' Noncompliant
            response.Cookies.Append("", "", New CookieOptions()) ' Noncompliant

            ' Response cookies as variable
            Dim responseCookies = response.Cookies
            responseCookies.Append("", "") ' Noncompliant
            responseCookies.Append("", "", New CookieOptions()) ' Noncompliant

            responseCookies.Delete("") ' Compliant
        End Sub

        Private Sub Requests(ByVal request As HttpRequest)
            Dim value = ""
            ' Request headers
            request.Headers.Add("Set-Cookie", "") ' Noncompliant
            request.Headers("Set-Cookie") = value ' Noncompliant
            value = request.Headers("Set-Cookie") ' Compliant

            ' Not the Set-Cookie header
            request.Headers.Add("something", "")
            request.Headers("something") = value
            value = request.Headers("something")

            ' Request headers as variable
            Dim requestHeaders = request.Headers
            requestHeaders.Add("Set-Cookie", "") ' Noncompliant
            requestHeaders("Set-Cookie") = value ' Noncompliant
            value = requestHeaders("Set-Cookie") ' Compliant

            ' Request cookies as property
            value = request.Cookies("") ' Compliant
            request.Cookies.TryGetValue("", value) ' Compliant

            ' Request cookies as variable
            Dim requestCookies = request.Cookies
            value = requestCookies("") ' Compliant
            requestCookies.TryGetValue("", value) ' Compliant
        End Sub
    End Class
End Namespace
