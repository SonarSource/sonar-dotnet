﻿' <Your-Product-Name>
' Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
'
' Please configure this header in your SonarCloud/SonarQube quality profile.
' You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.

Imports Microsoft.AspNetCore.Mvc

Namespace Tests.TestCases

    Public Class MyController
        Inherits Controller

        <HttpPost>
        <DisableRequestSizeLimit()> ' Noncompliant (S5693) ^10#25 {{Make sure the content length limit is safe here.}}
        Public Function PostRequestWithNoLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(10000000)> ' Noncompliant ^10#26 {{Make sure the content length limit is safe here.}}
        Public Function PostRequestAboveLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestSizeLimit(1623)> ' Compliant value Is below limit.
        Public Function PostRequestBelowLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartBodyLengthLimit:=8388609, MultipartHeadersLengthLimit:=42)> ' Noncompliant ^10#85 {{Make sure the content length limit is safe here.}}
        Public Function MultipartFormRequestAboveLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(MultipartHeadersLengthLimit:=42)>
        Public Function MultipartFormRequestHeadersLimitSet() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits()>
        Public Function MultiPartFromRequestWithDefaultLimit() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        <RequestFormLimits(mULTIPARTbODYlENGTHlIMIT:=42)> ' Compliant value Is below limit.
        Public Function MultipartFormRequestBelowLimit() As ActionResult
            Return Nothing
        End Function

        Public Function MethodWithoutAttributes() As ActionResult
            Return Nothing
        End Function

    End Class

End Namespace
