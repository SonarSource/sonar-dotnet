
Public Class NamesAndValues

    Private Auth As String = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"  ' Noncompliant {{"Auth" detected here, make sure this is not a hard-coded secret.}}
    '                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    Private AuthWithBackSlash As String = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"        ' Noncompliant
    Private AuthStartBackSlash As String = "\mf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"     ' Compliant starts with backslash
    Private AuthBackSlashIsEscape As String = "rf6acB24J//1FZLRrKpj\tmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"  ' Compliant has escape char

    Private WillNotRaise As String = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"             ' Compliant, name does not match
    Private AuthLowEntropy As String = "aaaaaaaaaaaaaaaaaaaa"                   ' Compliant, low entropy

    Private Secret As String = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.=" + "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.="    ' Noncompliant
    Private Token As String = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.="         ' Noncompliant
    Private Credential As String = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.="    ' Noncompliant
    Private Api_Key As String = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.="       ' Noncompliant

    Private END_TOKEN As String = "EndGlobalSection"                                        ' Compliant
    Private AccessControlAllowCredentials As String = "Access-Control-Allow-Credentials"    ' Compliant
    Private AuthenticatorApp As String = "OrchardCore.Users.2FA.AuthenticatorApp"           ' Compliant
    Private BackOfficeTwoFactorRememberMeAuthenticationType As String = "UmbracoTwoFactorRememberMeCookie" ' Compliant
    Private RequestVerificationToken As String = "__RequestVerificationToken"
    Private TokenPasswordResetCode As String = "PasswordResetCode"                          ' Compliant
    Private AuthenticationServiceName As String = "marketplacecommerceanalytics"
    Private XAmzSecurityToken As String = "X-Amz-Security-Token"
    Private ProductPublicKeyToken As String = "31bf3856ad364e35"                            ' Compliant, in Banlist
    Private Shared OfficialDesktopPublicKeyToken As String = "b03f5f7f11d50a3a"             ' Compliant, In Banlist
    Private VisualBasicAssemblyPublicKeyToken_Ampersant As String = "PublicKeyToken=" & OfficialDesktopPublicKeyToken   ' Compliant as it has 'Token' in name AND in value
    Private VisualBasicAssemblyPublicKeyToken_Plus As String = "PublicKeyToken=" + OfficialDesktopPublicKeyToken        ' Compliant as it has 'Token' in name AND in value

    Private VarStrings As String = "PublicKeyToken=31bf3856ad364e35;"                            ' Compliant, in Banlist
    Private VarStringer As String = "PublicKeyToken=31bf3856ad364e35; Secret=31bf3856ad364e35"   ' Noncompliant

    Private VarString As String = "token=rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"      ' Noncompliant {{"token" detected here, make sure this is not a hard-coded secret.}}
    Private SecretString As String = "token=rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"   ' Noncompliant {{"SecretString" detected here, make sure this is not a hard-coded secret.}}

End Class

Public Class Usages

    Private Api As New Scaffold()
    Public Field_Auth As String = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"                    ' Noncompliant
    Public Property AutoProperty_Auth As String = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"    ' Noncompliant

    Public Property Property_auth As String
        Get
            Return "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"           ' FN
        End Get
        Set(value As String)
            Field_Auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"     ' Noncompliant
        End Set
    End Property

    Public Sub Concat()
        Api.Key = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.="     ' Compliant

        Dim d_auth As String = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"   ' Noncompliant
        d_auth += "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"                ' Noncompliant
        d_auth &= "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"                ' Noncompliant
        d_auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"                 ' Noncompliant

        Dim c_auth As String = "a"
        c_auth = c_auth + "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"        ' Compliant does not compile to a constant
        c_auth = c_auth & "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"        ' Compliant does not compile to a constant

    End Sub

    Public Sub Comparison()
        Dim AuthToken As String = ""
        Dim ShouldNotRaise As String = ":)"
        If AuthToken = "31bf3856ad364e35" Then                      ' Noncompliant
        ElseIf "b03f5f7f11d50a3a" = AuthToken Then                  ' Noncompliant
        ElseIf ShouldNotRaise = "31bf3856ad364e35" Then             ' Compliant
        ElseIf "31bf3856ad364e35" = ShouldNotRaise Then             ' Compliant
        End If
        If "31bf3856ad364e35".Equals(AuthToken) Then : End If       ' Noncompliant
        If AuthToken.Equals("31bf3856ad364e35") Then : End If       ' Noncompliant
        If "31bf3856ad364e35".Equals(ShouldNotRaise) Then : End If  ' Compliant
        If ShouldNotRaise.Equals("31bf3856ad364e35") Then : End If  ' Compliant

        If "31bf3856ad364e35" Is "authToken" Then : End If                                                      ' Compliant FN
        If "authToken" Is "31bf3856ad364e35" Then : End If                                                      ' Compliant FN
        Select Case AuthToken
            Case "31bf3856ad364e35"                                                                             ' Compliant FN
        End Select
    End Sub

    Public Sub EqualsMethods()
        Dim AuthToken As String = ""
        Dim ShouldNotRaise As String = ":)"
        Dim Bool As Boolean = "AuthToken".Equals("31bf3856ad364e35") ' Compliant
        AuthToken.Equals(ShouldNotRaise)    ' Compliant
        ShouldNotRaise.Equals(Nothing)      ' Compliant
        Bool = "AuthToken".Equals(Nothing)  ' Compliant

        Bool = "31bf3856ad364e35".Equals(AuthToken, StringComparison.CurrentCulture)    ' Noncompliant
        AuthToken.Equals("31bf3856ad364e35", StringComparison.CurrentCulture)           ' Noncompliant

        Dim Values As New CustomValues
        Dim Provider As New CustomProvider

        If Values.Test.Test.ShouldNotRaise.Equals("31bf3856ad364e35") Then : End If     ' Compliant
        If "31bf3856ad364e35".Equals(Values.Test.Test.ShouldNotRaise) Then : End If     ' Compliant
        If Values.Test.Test.Token.Equals("31bf3856ad364e35") Then : End If              ' Noncompliant
        If "31bf3856ad364e35".Equals(Values.Test.Test.Token) Then : End If              ' Noncompliant

        If Provider.GetToken().Equals("31bf3856ad364e35") Then : End If                 ' Compliant not String.Equals
        If Provider.GetAuthToken().Equals("31bf3856ad364e35") Then : End If             ' Noncompliant
        If "31bf3856ad364e35".Equals(Provider.GetToken()) Then : End If                 ' Noncompliant
        If Provider.GetToken().Test.Equals("31bf3856ad364e35") Then : End If            ' Compliant
        If "31bf3856ad364e35".Equals(Provider.GetToken().Test) Then : End If            ' Compliant

        String.Equals("31bf3856ad364e35", AuthToken)                                                            ' Compliant FN
        String.Equals(AuthToken, "31bf3856ad364e35")                                                            ' Compliant FN
        String.Equals("31bf3856ad364e35", AuthToken, StringComparison.CurrentCulture)                           ' Compliant FN
        String.Equals(comparisonType:=StringComparison.CurrentCulture, a:="31bf3856ad364e35", b:=AuthToken)     ' Compliant FN
        StringComparer.InvariantCulture.Equals("31bf3856ad364e35", AuthToken)                                   ' Compliant FN
        StringComparer.InvariantCulture.Equals(AuthToken, "31bf3856ad364e35")                                   ' Compliant FN
        EqualityComparer(Of String).Default.Equals("31bf3856ad364e35", AuthToken)                               ' Compliant FN
        EqualityComparer(Of String).Default.Equals(AuthToken, "31bf3856ad364e35")                               ' Compliant FN
    End Sub

End Class

Public Class CustomProvider

    Public Function GetToken() As CustomValues
        Return New CustomValues
    End Function

    Public Function GetAuthToken() As String
        Return "31bf3856ad364e35"
    End Function

End Class

Public Class CustomValues

    Public Test As New CustomValues
    Public ShouldNotRaise As String
    Public Token As String

End Class

Public Class Scaffold

    Public Property Key As String

End Class
