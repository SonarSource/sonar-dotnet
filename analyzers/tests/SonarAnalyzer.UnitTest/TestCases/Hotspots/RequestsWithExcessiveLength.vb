Imports System
Imports Microsoft.AspNetCore.Mvc

Namespace Tests.TestCases

    Public Class MyController
        Inherits Controller

        <HttpPost>
        <DisableRequestSizeLimit()> ' Noncompliant ^10#25 {{Make sure the content length limit is safe here.}}
        Public Function PostRequestWithNoLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(8_000_001)> ' Noncompliant ^10#27 {{Make sure the content length limit is safe here.}}
        Public Function PostRequestAboveLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(8_000_000)> ' Compliant value Is below limit.
        Public Function PostRequestBelowLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=8_000_001, MultipartHeadersLengthLimit:=42)> ' Noncompliant ^10#87 {{Make sure the content length limit is safe here.}}
        Public Function MultipartFormRequestAboveLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartHeadersLengthLimit:=8_000_000)>
        Public Function MultipartFormRequestHeadersLimitSet() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits()>
        Public Function MultiPartFromRequestWithDefaultLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(mULTIPARTbODYlENGTHlIMIT:=8_000_000)> ' Compliant value Is below limit.
        Public Function MultipartFormRequestBelowLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartHeadersLengthLimit:="NonNumericalValue")> ' Error [BC30934]
        Public Function MultipartFormRequestNonNumerical() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits>
        Public Function MultipartFormRequestWithoutParams() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(42)> ' Error [BC30057]
        Public Function MultipartFormRequestNonNameEquals() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit("NonNumerical")> ' Error [BC30934]
        Public Function PostRequestNonNumerical() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit> ' Error [BC30455]
        Public Function PostRequestWithoutParams() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(Integer.MaxValue)> ' Noncompliant
        Public Function RequestSizeLimitWithoutNumericLiteral() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=1000000000)> ' Noncompliant [1]
        <RequestSizeLimit(1000000000)> ' Secondary [1]
        Public Function RequestSizeLimitAndFormLimits() As ActionResult
            Return Nothing
        End Function

        Public Function MethodWithoutAttributes() As ActionResult
            Return Nothing
        End Function

    End Class

    <DisableRequestSizeLimit()> ' Noncompliant ^6#25
    Public Class DisableRequestSizeLimitController
        Inherits Controller

    End Class

    <RequestSizeLimit(8_000_001)> ' Noncompliant
    Public Class RequestSizeLimitAboveController
        Inherits Controller

    End Class

    <RequestFormLimits(MultipartBodyLengthLimit:=8_000_001)> ' Noncompliant
    Public Class RequestFormLimitsAboveController
        Inherits Controller

    End Class

    <RequestSizeLimit(8_000_000)>
    Public Class RequestSizeLimitBelowController
        Inherits Controller

    End Class

    <RequestFormLimits(MultipartBodyLengthLimit:=8_000_000)>
    Public Class RequestFormLimitsBelowController
        Inherits Controller

    End Class

End Namespace
