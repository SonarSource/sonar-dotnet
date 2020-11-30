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
'Originally created by Lawrence "nicezia" Winston (http://sourceforge.net/projects/scraperxml/)
'Converted to VB.NET and modified for use with Ember Media Manager

Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Xml.Linq

Imports EmberScraperModule.XMLScraper.Utilities

Namespace XMLScraper
    Namespace ScraperLib

        Public Class Tester

#Region "Fields"

            Const MaxScraperBuffers As Integer = 20

            Private _cachefolder As String
            Private _param As String() = New String(MaxScraperBuffers - 1) {}

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                Me._param = New String(MaxScraperBuffers - 1) {}
                Me._cachefolder = String.Empty

                For i As Integer = 0 To MaxScraperBuffers - 1
                    Me._param(i) = String.Empty
                Next
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property CacheFolder() As String
                Get
                    Return _cachefolder
                End Get
                Set(ByVal value As String)
                    _cachefolder = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Function FindAndParse(ByVal objScraper As Scraper, ByVal FunctionName As String) As String
                Dim IndexNo As Integer = -1

                For i As Integer = 0 To objScraper.ScraperFunctions.Count - 1
                    If objScraper.ScraperFunctions(i).Name = FunctionName Then
                        IndexNo = i
                        Exit For
                    End If
                Next

                If Not IndexNo = -1 Then
                    Return ParseFunction(objScraper.ScraperFunctions(IndexNo))
                Else
                    Return "Function Not Found"
                End If
            End Function

            Public Function GetEditableSettings(ByVal scGetSettingsFunction As ScraperFunction) As EditorSettingsHolder
                Dim xSettings As New XElement("settings")
                Dim tmpSettings As New XElement("settings")

                tmpSettings = XElement.Parse(ParseFunction(scGetSettingsFunction))

                Return New EditorSettingsHolder(tmpSettings)
            End Function

            Public Sub ParseExpression(ByVal objRegExp As ScraperRegExp)
                If objRegExp.Expression.Clear Then
                    _param(objRegExp.Dest - 1) = String.Empty
                End If

                ExpressionCompile(objRegExp.Dest - 1, objRegExp.Append, objRegExp.Input, objRegExp.Output, objRegExp.Expression.CaseSensitive, objRegExp.Expression.Repeat, _
                 objRegExp.Expression.Expression, objRegExp.Expression.NoCleanString, objRegExp.Expression.TrimString, objRegExp.Expression.EncodeString)
            End Sub

            Public Function ParseFunction(ByVal sFunction As ScraperFunction) As String
                Dim WhereToFind As Integer = sFunction.Dest - 1

                For Each RegExp As ScraperRegExp In sFunction.RegExps
                    ParseNext(RegExp)
                Next

                Return _param(WhereToFind)
            End Function

            Public Sub ParseNext(ByVal objRegExp As ScraperRegExp)
                For Each item As ScraperRegExp In objRegExp.RegExps
                    ParseNext(item)
                Next

                ParseExpression(objRegExp)
            End Sub

            Public Sub SetBuffer(ByVal BufferNumber As Integer, ByVal strBufferFill As String, ByVal append As Boolean)
                If append Then
                    _param(BufferNumber) += strBufferFill
                Else
                    _param(BufferNumber) = strBufferFill
                End If
            End Sub

            Friend Function GetFullSettings(ByVal scItem As Scraper) As ScraperSettings
                Dim tmp As New ScraperSettings()

                Dim tmpElement As XElement = XElement.Parse(ParseFunction(scItem.GetSettingsFunction))

                For Each item As XElement In tmpElement.Elements()
                    If item.Name.ToString = "setting" Then
                        tmp.Add(New ScraperSetting(item))
                    ElseIf item.Name.ToString = "url" Then
                        Dim tmpUrl As New UrlInfo(item)
                        SetBuffer(0, XMLScraper.Utilities.HttpRetrieve.GetPage(tmpUrl, ""), False)
                        tmp.AddRange(GetSettingsUrl(tmpUrl, scItem))
                    End If
                Next
                Return tmp
            End Function

            Friend Function GetSettings(ByVal scraperItem As Scraper) As ScraperSettings
                Dim tmpSettings As New ScraperSettings()

                Dim temp As XElement = XElement.Parse(ParseFunction(scraperItem.GetSettingsFunction))

                For Each item As XElement In temp.Elements()
                    If item.Name.ToString = "setting" Then
                        tmpSettings.Add(New ScraperSetting(item))
                    Else
                        tmpSettings.AddRange(GetSettingsUrl(New UrlInfo(item), scraperItem))
                    End If
                Next

                Return tmpSettings
            End Function

            Friend Function GetSettingsUrl(ByVal Url As UrlInfo, ByVal scItem As Scraper) As List(Of ScraperSetting)
                Dim tmp As New List(Of ScraperSetting)()
                Dim tmpElement As XElement = XElement.Parse(FindAndParse(scItem, Url.[Function]))

                For Each item As XElement In tmpElement.Elements()
                    If item.Name.ToString = "setting" Then
                        tmp.Add(New ScraperSetting(item))
                    ElseIf item.Name.ToString = "url" Then
                        tmp.AddRange(GetSettingsUrl(New UrlInfo(item), scItem))
                    End If
                Next
                Return tmp
            End Function

            Private Shared Function CleanString(ByVal strHTML As String) As String
                Dim iNested As Integer = 0
                Dim sTemp As New StringBuilder

                For i As Integer = 0 To strHTML.Length - 1
                    If strHTML(i) = "<" Then
                        iNested += 1
                    ElseIf strHTML(i) = ">" Then
                        iNested -= 1
                    Else
                        If iNested <= 0 Then
                            sTemp.Append(strHTML(i))
                        End If
                    End If
                Next

                Dim strReturn As String = sTemp.ToString.Replace("&mdash;", "--")
                strReturn = strReturn.Replace("&#160;", " ")
                strReturn = strReturn.Replace("&ndash;", "-")
                strReturn = strReturn.Replace("&oacute;", "ó")
                strReturn = strReturn.Replace("&nbsp;", "")
                strReturn = strReturn.Replace("&rsquo;", "&apos;")

                Return strReturn
            End Function

            Private Sub ExpressionCompile(ByVal _dest As Integer, ByVal _append As Boolean, ByVal _input As String, ByVal _output As String, ByVal _cs As Boolean, ByVal _repeat As Boolean, _
                ByVal _exp As String, ByVal _noclean As String, ByVal _trim As String, ByVal _encode As String)
                Dim RegExp As Regex

                If String.IsNullOrEmpty(_exp) Then
                    _exp = "(.*)"
                End If

                If _cs Then
                    RegExp = New Regex(ReplaceBuffers(_exp), RegexOptions.Singleline)
                Else
                    RegExp = New Regex(ReplaceBuffers(_exp), RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                End If

                Dim curInput As String = ReplaceBuffers(_input)

                If RegExp.Match(curInput).Success Then
                    Dim strReturn As String = String.Empty

                    If _repeat Then
                        Dim AllMatches As MatchCollection = RegExp.Matches(curInput)

                        For i As Integer = 0 To AllMatches.Count - 1
                            Dim tmpString As String = _output

                            For iReplace As Integer = 1 To 9
                                Dim ReplaceString As String = String.Format("\{0}", Convert.ToString(iReplace))
                                If tmpString.Contains(ReplaceString) Then
                                    tmpString = tmpString.Replace(ReplaceString, PostMatch(AllMatches(i).Groups(iReplace).Value, _encode.Contains(Convert.ToString(iReplace)), Not _noclean.Contains(Convert.ToString(iReplace)), _trim.Contains(Convert.ToString(iReplace))))
                                End If
                            Next
                            strReturn += tmpString
                        Next
                        strReturn = ReplaceBuffers(strReturn)
                    Else
                        strReturn = _output
                        Dim SingleMatch As Match = RegExp.Match(curInput)

                        For i As Integer = 1 To 9
                            Dim ReplaceString As String = String.Format("\{0}", Convert.ToString(i))
                            If strReturn.Contains(ReplaceString) Then
                                If strReturn.Contains(ReplaceString) Then
                                    strReturn = strReturn.Replace(ReplaceString, PostMatch(SingleMatch.Groups(i).Value, _encode.Contains(Convert.ToString(i)), Not _noclean.Contains(Convert.ToString(i)), _trim.Contains(Convert.ToString(i))))
                                End If
                            End If
                        Next
                        strReturn = ReplaceBuffers(strReturn)
                    End If
                    SetBuffer(_dest, strReturn, _append)
                End If
            End Sub

            Private Function PostMatch(ByVal strToProcess As String, ByVal bUrlEncode As Boolean, ByVal bCleanString As Boolean, ByVal bTrimString As Boolean) As String
                If bCleanString Then
                    strToProcess = CleanString(strToProcess)
                End If

                If bTrimString Then
                    strToProcess = strToProcess.Trim
                End If

                If bUrlEncode Then
                    strToProcess = HttpUtility.UrlEncode(strToProcess)
                End If

                strToProcess = strToProcess.Replace("&", "&amp;")

                Return strToProcess
            End Function

            Private Function ReplaceBuffers(ByVal strReplace As String) As String
                For i As Integer = 20 To 1 Step -1
                    strReplace = strReplace.Replace(String.Concat("$$", Convert.ToString(i)), Me._param(i - 1))
                Next

                strReplace = strReplace.Replace("\n", vbLf)
                Return strReplace
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
