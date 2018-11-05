' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Drawing

Public Class StringUtils

#Region "Methods"

    Public Shared Function AlphaNumericOnly(ByVal KeyChar As Char, Optional ByVal AllowSpecial As Boolean = False) As Boolean
        If Char.IsLetterOrDigit(KeyChar) OrElse (AllowSpecial AndAlso (Char.IsControl(KeyChar) OrElse _
        Char.IsWhiteSpace(KeyChar) OrElse Asc(KeyChar) = 44 OrElse Asc(KeyChar) = 45 OrElse Asc(KeyChar) = 46 OrElse Asc(KeyChar) = 58)) Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Shared Function CleanStackingMarkers(ByVal sPath As String, Optional ByVal Asterisk As Boolean = False) As String
        If AdvancedSettings.GetBooleanSetting("DisableMultiPartMedia", False) Then Return sPath
        If String.IsNullOrEmpty(sPath) Then Return String.Empty
        Dim sReturn As String = Regex.Replace(sPath, AdvancedSettings.GetSetting("DeleteStackMarkers", "\|?\-*\.*((cd|dvd|part|dis[ck])([0-9]))"), If(Asterisk, "*", " "), RegexOptions.IgnoreCase).Trim
        If Not sReturn = sPath Then
            Return Regex.Replace(sReturn, "\s\s(\s+)?", " ").Trim
        Else
            Return sPath
        End If
    End Function

    Public Shared Function CleanURL(ByVal sURL As String) As String
        If sURL.ToLower.Contains("imgobject.com") Then
            Dim tURL As String = String.Empty
            Dim i As Integer = sURL.IndexOf("/posters/")
            If i >= 0 Then tURL = sURL.Substring(i + 9)
            i = sURL.IndexOf("/backdrops/")
            If i >= 0 Then tURL = sURL.Substring(i + 11)
            'tURL = sURL.Replace("http://images.themoviedb.org/posters/", String.Empty)
            'tURL = tURL.Replace("http://images.themoviedb.org/backdrops/", String.Empty)
            '$$ to sort first
            sURL = String.Concat("$$[imgobject.com]", tURL)
        Else
            sURL = TruncateURL(sURL, 40, True)
        End If
        Return sURL.Replace(":", "$c$").Replace("/", "$s$")
    End Function

    Public Shared Function ComputeLevenshtein(ByVal s As String, ByVal t As String) As Integer
        Dim n As Integer = s.Length
        Dim m As Integer = t.Length
        Dim d As Integer(,) = New Integer(n, m) {}

        If n = 0 Then
            Return 100
        End If

        If m = 0 Then
            Return n
        End If

        Dim i As Integer = 0
        While i <= n
            d(i, 0) = System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
        End While

        Dim j As Integer = 0
        While j <= m
            d(0, j) = System.Math.Max(System.Threading.Interlocked.Increment(j), j - 1)
        End While

        Dim cost As Integer = 0
        For k As Integer = 1 To n
            For l As Integer = 1 To m
                cost = If((t(l - 1) = s(k - 1)), 0, 1)

                d(k, l) = Math.Min(Math.Min(d(k - 1, l) + 1, d(k, l - 1) + 1), d(k - 1, l - 1) + cost)
            Next
        Next
        Return d(n, m) - 1
    End Function

    Public Shared Function Decode(ByVal encText As String) As String
        Try
            Dim dByte() As Byte
            dByte = System.Convert.FromBase64String(encText)
            Dim decText As String
            decText = System.Text.Encoding.ASCII.GetString(dByte)
            Return decText
        Catch
        End Try
        Return String.Empty
    End Function

    Public Shared Function Encode(ByVal decText As String) As String
        Dim eByte() As Byte
        ReDim eByte(decText.Length)
        eByte = System.Text.Encoding.ASCII.GetBytes(decText)
        Dim encText As String
        encText = System.Convert.ToBase64String(eByte)
        Return encText
    End Function

    Public Shared Function FilterName(ByVal movieName As String, Optional ByVal doExtras As Boolean = True, Optional ByVal remPunct As Boolean = False) As String
        '//
        ' Clean all the crap out of the name
        '\\
        Try

            If String.IsNullOrEmpty(movieName) Then Return String.Empty

            Dim strSplit() As String
            Try

                'run through each of the custom filters
                If Master.eSettings.FilterCustom.Count > 0 Then
                    For Each Str As String In Master.eSettings.FilterCustom

                        'everything was already filtered out, return an empty string
                        If String.IsNullOrEmpty(movieName) Then Return String.Empty

                        If Str.IndexOf("[->]") > 0 Then
                            strSplit = Strings.Split(Str, "[->]")
                            movieName = Strings.Replace(movieName, Regex.Match(movieName, strSplit.First).ToString, strSplit.Last)
                        Else
                            movieName = Strings.Replace(movieName, Regex.Match(movieName, Str).ToString, String.Empty)
                        End If
                    Next
                End If

                movieName = CleanStackingMarkers(movieName.Trim)

                'Convert String To Proper Case
                If Master.eSettings.ProperCase AndAlso doExtras Then
                    movieName = ProperCase(movieName)
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            If doExtras Then movieName = FilterTokens(movieName.Trim)
            If remPunct Then movieName = RemovePunctuation(movieName.Trim)

            Return movieName.Trim

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            ' Some error handling so EMM dont break on populate folderdata
            Return movieName.Trim
        End Try
    End Function

    Public Shared Function FilterTokens(ByVal sTitle As String) As String
        Dim newTitle As String = sTitle
        If Master.eSettings.SortTokens.Count > 0 Then
            For Each sToken As String In Master.eSettings.SortTokens
                If Regex.IsMatch(sTitle, String.Concat("^", sToken), RegexOptions.IgnoreCase) Then
                    newTitle = String.Format("{0}, {1}", Regex.Replace(sTitle, String.Concat("^", sToken), String.Empty, RegexOptions.IgnoreCase).Trim, Regex.Match(sTitle, String.Concat("^", Regex.Replace(sToken, "\[(.*?)\]", String.Empty)), RegexOptions.IgnoreCase)).Trim
                    Exit For
                End If
            Next
        End If
        Return newTitle.Trim
    End Function

    Public Shared Function FilterTVEpName(ByVal TVEpName As String, ByVal TVShowName As String, Optional ByVal doExtras As Boolean = True, Optional ByVal remPunct As Boolean = False) As String
        '//
        ' Clean all the crap out of the name
        '\\
        Try

            If String.IsNullOrEmpty(TVEpName) Then Return String.Empty

            Dim strSplit() As String

            'run through each of the custom filters
            If Master.eSettings.EpFilterCustom.Count > 0 Then
                For Each Str As String In Master.eSettings.EpFilterCustom

                    'everything was already filtered out, return an empty string
                    If String.IsNullOrEmpty(TVEpName) Then Return String.Empty

                    If Str.IndexOf("[->]") > 0 Then
                        strSplit = Strings.Split(Str, "[->]")
                        TVEpName = Strings.Replace(TVEpName, Regex.Match(TVEpName, strSplit.First).ToString, strSplit.Last)
                    Else
                        TVEpName = Strings.Replace(TVEpName, Regex.Match(TVEpName, Str).ToString, String.Empty)
                    End If

                Next
            End If

            'remove the show name from the episode name
            If Not String.IsNullOrEmpty(TVShowName) Then TVEpName = Strings.Replace(TVEpName, TVShowName, String.Empty, 1, -1, CompareMethod.Text)

            'Convert String To Proper Case
            If Master.eSettings.EpProperCase AndAlso doExtras Then
                TVEpName = ProperCase(TVEpName)
            End If

            If remPunct Then TVEpName = RemovePunctuation(CleanStackingMarkers(TVEpName.Trim))

            Return TVEpName.Trim

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            Return TVEpName.Trim
        End Try
    End Function

    Public Shared Function FilterTVShowName(ByVal TVShowName As String, Optional ByVal doExtras As Boolean = True, Optional ByVal remPunct As Boolean = False) As String
        '//
        ' Clean all the crap out of the name
        '\\
        Try

            If String.IsNullOrEmpty(TVShowName) Then Return String.Empty

            Dim strSplit() As String

            'run through each of the custom filters
            If Master.eSettings.ShowFilterCustom.Count > 0 Then
                For Each Str As String In Master.eSettings.ShowFilterCustom

                    'everything was already filtered out, return an empty string
                    If String.IsNullOrEmpty(TVShowName) Then Return String.Empty

                    If Str.IndexOf("[->]") > 0 Then
                        strSplit = Strings.Split(Str, "[->]")
                        TVShowName = Strings.Replace(TVShowName, Regex.Match(TVShowName, strSplit.First).ToString, strSplit.Last)
                    Else
                        TVShowName = Strings.Replace(TVShowName, Regex.Match(TVShowName, Str).ToString, String.Empty)
                    End If
                Next
            End If

            'Convert String To Proper Case
            If Master.eSettings.ShowProperCase AndAlso doExtras Then
                TVShowName = ProperCase(TVShowName)
            End If

            If remPunct Then TVShowName = RemovePunctuation(CleanStackingMarkers(TVShowName.Trim))

            Return TVShowName.Trim

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            ' Some error handling so EMM dont break on populate folderdata
            Return TVShowName.Trim
        End Try
    End Function

    Public Shared Function FilterYear(ByVal sString As String) As String
        Return Regex.Replace(sString, "([ _.-]\(?\d{4}\))?", String.Empty).Trim
    End Function

    Public Shared Function FormatSeasonText(ByVal sSeason As Integer) As String
        If sSeason > 0 Then
            Return String.Concat(Master.eLang.GetString(654, "Season "), sSeason.ToString.PadLeft(2, Convert.ToChar("0")))
        ElseIf sSeason = 0 Then
            Return Master.eLang.GetString(655, "Season Specials")
        Else
            Return Master.eLang.GetString(283, "Unknown")
        End If
    End Function

    Public Shared Function HtmlEncode(ByVal stext As String) As String
        Dim chars = Web.HttpUtility.HtmlEncode(stext).ToCharArray()
        Dim result As StringBuilder = New StringBuilder(stext.Length + Convert.ToInt16(stext.Length * 0.1))

        For Each c As Char In chars
            Dim value As Integer = Convert.ToInt32(c)
            If (value > 127) Then
                result.AppendFormat("&#{0};", value)
            Else
                result.Append(c)
            End If

        Next
        Return result.ToString()
    End Function

    Public Shared Function IsStacked(ByVal sName As String, Optional ByVal VTS As Boolean = False) As Boolean
        If String.IsNullOrEmpty(sName) Then Return False
        If AdvancedSettings.GetBooleanSetting("DisableMultiPartMedia", False) Then Return False
        Dim bReturn As Boolean = False
        If VTS Then
            bReturn = Regex.IsMatch(sName, AdvancedSettings.GetSetting("CheckStackMarkers", "\|?\-*\.*((cd|dvd|part|dis[ck])([0-9]))"), RegexOptions.IgnoreCase) OrElse Regex.IsMatch(sName, "^vts_[0-9]+_[0-9]+", RegexOptions.IgnoreCase)
        Else
            bReturn = Regex.IsMatch(sName, AdvancedSettings.GetSetting("CheckStackMarkers", "\|?\-*\.*((cd|dvd|part|dis[ck])([0-9]))"), RegexOptions.IgnoreCase)
        End If
        Return bReturn
    End Function

    Public Shared Function isValidURL(ByVal sToCheck As String) As Boolean
        '        Return Regex.IsMatch(sToCheck, "^((ht|f)tps?\:\/\/|~\/|\/)?(\w+:\w+@)?(([-\w]+\.)+(com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))\/", RegexOptions.IgnoreCase)
        Return Regex.IsMatch(sToCheck, "[a-zA-Z]{3,}://[a-zA-Z0-9\.]+/*[a-zA-Z0-9/\\%_.]*\?*[a-zA-Z0-9/\\%_.=&amp;]*")

    End Function

    Public Shared Function NumericOnly(ByVal KeyChar As Char, Optional ByVal isIP As Boolean = False) As Boolean
        If Char.IsNumber(KeyChar) OrElse Char.IsControl(KeyChar) OrElse Char.IsWhiteSpace(KeyChar) OrElse (isIP AndAlso Asc(KeyChar) = 46) Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Shared Function ProperCase(ByVal sString As String) As String
        If String.IsNullOrEmpty(sString) Then Return String.Empty
        Dim sReturn As String = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(sString)
        Dim toUpper As String = AdvancedSettings.GetSetting("ToProperCase", "\b(hd|cd|dvd|bc|b\.c\.|ad|a\.d\.|sw|nw|se|sw|ii|iii|iv|vi|vii|viii|ix|x)\b")

        Dim mcUp As MatchCollection = Regex.Matches(sReturn, toUpper, RegexOptions.IgnoreCase)
        For Each M As Match In mcUp
            sReturn = sReturn.Replace(M.Value, Strings.StrConv(M.Value, VbStrConv.Uppercase))
        Next

        Return sReturn
    End Function

    Public Shared Function RemovePunctuation(ByVal sString As String) As String
        If String.IsNullOrEmpty(sString) Then Return String.Empty
        Dim sReturn As String = Regex.Replace(sString, "\W", " ")
        Return Regex.Replace(sReturn.ToLower, "\s\s(\s+)?", " ").Trim
    End Function

    Public Shared Function StringToSize(ByVal sString As String) As Size
        If Regex.IsMatch(sString, "^[0-9]+x[0-9]+$", RegexOptions.IgnoreCase) Then
            Dim SplitSize() As String = Strings.Split(sString, "x")
            Return New Size With {.Width = Convert.ToInt32(SplitSize(0)), .Height = Convert.ToInt32(SplitSize(1))}
        Else
            Return New Size With {.Width = 0, .Height = 0}
        End If
    End Function

    Public Shared Function TruncateURL(ByVal sString As String, ByVal MaxLength As Integer, Optional ByVal EndOnly As Boolean = False) As String
        '//
        ' Shorten a URL to fit on the GUI
        '\\

        Try
            Dim sEnd As String = String.Empty
            If EndOnly Then
                Return Strings.Right(sString, MaxLength)
            Else
                sEnd = Strings.Right(sString, sString.Length - sString.LastIndexOf("/"))
                If ((MaxLength - sEnd.Length) - 3) > 0 Then
                    Return String.Format("{0}...{1}", Strings.Left(sString, (MaxLength - sEnd.Length) - 3), sEnd)
                Else
                    If sEnd.Length >= MaxLength Then
                        Return String.Format("...{0}", Strings.Right(sEnd, MaxLength - 3))
                    Else
                        Return sEnd
                    End If
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return String.Empty
    End Function

    Public Shared Function USACertToMPAA(ByVal sCert As String) As String
        Select Case sCert.ToLower
            Case "usa:g"
                Return "Rated G"
            Case "usa:pg"
                Return "Rated PG"
            Case "usa:pg-13"
                Return "Rated PG-13"
            Case "usa:r"
                Return "Rated R"
            Case "usa:nc-17"
                Return "Rated NC-17"
        End Select
        Return String.Empty
    End Function

#End Region 'Methods

#Region "Nested Types"

    Public Class Wildcard

#Region "Methods"

        Public Shared Function IsMatch(ByVal ExpressionToMatch As String, ByVal FilterExpression As String, Optional ByVal IgnoreCase As Boolean = True) As Boolean
            If FilterExpression.Contains("*") _
                OrElse FilterExpression.Contains("?") _
                OrElse FilterExpression.Contains("#") Then

                If IgnoreCase Then
                    Return (ExpressionToMatch.ToLower Like FilterExpression.ToLower)
                Else
                    Return (ExpressionToMatch Like FilterExpression)
                End If

            Else
                If IgnoreCase Then
                    Return ExpressionToMatch.ToLower.Contains(FilterExpression.ToLower)
                Else
                    Return ExpressionToMatch.Contains(FilterExpression)
                End If
            End If
        End Function

#End Region 'Methods

    End Class

#End Region 'Nested Types

End Class