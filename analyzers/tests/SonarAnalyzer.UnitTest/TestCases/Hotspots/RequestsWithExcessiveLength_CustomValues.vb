Imports System
Imports Microsoft.AspNetCore.Mvc

Namespace Tests.TestCases

    Public Class MyController
        Inherits Controller

        <HttpPost>
        <RequestSizeLimit(43)> ' Noncompliant ^10#20 {{Make sure the content length limit is safe here.}}
        Public Function PostRequestAboveLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=43)> ' Noncompliant ^10#47 {{Make sure the content length limit is safe here.}}
        Public Function MultipartFormRequestAboveLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(41)>
        Public Function PostRequestBelowLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=41)>
        Public Function MultipartFormRequestBelowLimit() As ActionResult
            Return Nothing
        End Function

    End Class

End Namespace
