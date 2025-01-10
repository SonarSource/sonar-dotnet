Imports System
Imports Microsoft.AspNetCore.Mvc

Namespace Tests.TestCases

    Public Class MyController
        Inherits Controller

        <HttpPost>
        <DisableRequestSizeLimit()>
        Public Function PostRequestWithNoLimit() As ActionResult
        '^^^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant@-1 {{Make sure the content length limit is safe here.}}
            Return Nothing
        End Function

        <HttpPost>
        <DisableRequestSizeLimitAttribute()> ' Noncompliant
        Public Function DisableRequestSizeLimitWithFullName() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(8_388_609)>
        Public Function PostRequestAboveLimit() As ActionResult
        '^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant@-1 {{Make sure the content length limit is safe here.}}
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(8_388_609)> ' Noncompliant
        Public Function RequestSizeLimitWithFullname() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(8_388_608)> ' Compliant value Is below limit.
        Public Function PostRequestBelowLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(8_389_632)> ' Noncompliant
        Public Function SizeWith1024Base() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=8_388_609, MultipartHeadersLengthLimit:=42)>
        Public Function MultipartFormRequestAboveLimit() As ActionResult
        '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant@-1 {{Make sure the content length limit is safe here.}}
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=8_388_609)> ' Noncompliant
        Public Function RequestFormLimitsWithFullname() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartHeadersLengthLimit:=8_388_608)>
        Public Function MultipartFormRequestHeadersLimitSet() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits()>
        Public Function MultiPartFromRequestWithDefaultLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartHeadersLengthLimit:=42, MultipartBodyLengthLimit:=8_388_609)> ' Noncompliant
        Public Function RequestFormLimitsWithVariousParamsV1() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(BufferBody:=True, MultipartBodyLengthLimit:=8_388_609)> ' Noncompliant
        Public Function RequestFormLimitsWithVariousParamsV2() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(BufferBody:=True, MultipartHeadersLengthLimit:=42, ValueCountLimit:=42, MultipartBodyLengthLimit:=8_388_609)> ' Noncompliant
        Public Function RequestFormLimitsWithVariousParamsV3() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(BufferBody:=True, MultipartHeadersLengthLimit:=42, ValueCountLimit:=42)>
        Public Function RequestFormLimitsWithVariousParamsV4() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(mULTIPARTbODYlENGTHlIMIT:=8_388_608)> ' Compliant value Is below limit.
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
        <RequestSizeLimit(1000000000)> ' Secondary [1] {{Make sure the content length limit is safe here.}}
        Public Function RequestSizeLimitAndFormLimits() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=1000000000)> ' Noncompliant
        <RequestSizeLimit(42)>
        Public Function SizeBelowLimitFormsAboveLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=42)>
        <RequestSizeLimit(1000000000)> ' Noncompliant
        Public Function SizeAboveLimitFormsBelowLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=42)>
        <RequestSizeLimit(42)>
        Public Function BothBelowLimit() As ActionResult
            Return Nothing
        End Function

        Public Function MethodWithoutAttributes() As ActionResult
            Return Nothing
        End Function

    End Class

    <DisableRequestSizeLimit()>
    Public Class DisableRequestSizeLimitController
        Inherits Controller

    End Class
    '^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant@-4

    <RequestSizeLimit(8_388_609)> ' Noncompliant
    Public Class RequestSizeLimitAboveController
        Inherits Controller

    End Class

    <RequestFormLimits(MultipartBodyLengthLimit:=8_388_609)> ' Noncompliant
    Public Class RequestFormLimitsAboveController
        Inherits Controller

    End Class

    <RequestSizeLimit(8_388_608)>
    Public Class RequestSizeLimitBelowController
        Inherits Controller

    End Class

    <RequestFormLimits(MultipartBodyLengthLimit:=8_388_608)>
    Public Class RequestFormLimitsBelowController
        Inherits Controller

    End Class

End Namespace
