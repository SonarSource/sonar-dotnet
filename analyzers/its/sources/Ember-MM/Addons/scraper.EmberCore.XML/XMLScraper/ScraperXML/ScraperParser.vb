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
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

Imports EmberScraperModule.XMLScraper.ScraperLib
Imports EmberScraperModule.XMLScraper.Utilities

Namespace XMLScraper
    Namespace ScraperXML

        Friend NotInheritable Class ScraperParser

#Region "Fields"

            Const MaxScraperBuffers As Integer = 20

            Private _buffers As String()
            Private _content As String
            Private _name As String
            Private _settings As ScraperSettings
            Private _xmlscraper As XDocument

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal infoScraper As ScraperInfo)
                Me._buffers = New String(MaxScraperBuffers - 1) {}

                ClearBuffers()

                If Not IsNothing(infoScraper) Then
                    Me._xmlscraper = XDocument.Load(infoScraper.ScraperPath)
                    Me._content = XmlScraper.Root.Attribute("content").Value
                    Me._name = XmlScraper.Root.Attribute("name").Value
                    Me._settings = infoScraper.Settings
                End If
            End Sub

            Public Sub New()
                Me._buffers = New String(MaxScraperBuffers - 1) {}
            End Sub

#End Region 'Constructors

#Region "Properties"

            Friend Property Buffers() As String()
                Get
                    Return Me._buffers
                End Get
                Set(ByVal value As String())
                    Me._buffers = value
                End Set
            End Property

            Friend Property Content() As String
                Get
                    Return Me._content
                End Get
                Set(ByVal value As String)
                    Me._content = value
                End Set
            End Property

            Friend Property Name() As String
                Get
                    Return Me._name
                End Get
                Set(ByVal value As String)
                    Me._name = value
                End Set
            End Property

            Friend Property Settings() As ScraperSettings
                Get
                    Return Me._settings
                End Get
                Set(ByVal value As ScraperSettings)
                    Me._settings = value
                End Set
            End Property

            Friend Property XmlScraper() As XDocument
                Get
                    Return Me._xmlscraper
                End Get
                Set(ByVal value As XDocument)
                    Me._xmlscraper = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Function HasFunction(ByVal strFunctionName As String) As Boolean
                If Not IsNothing(Me._xmlscraper.Root.Element(strFunctionName)) Then
                    Return True
                Else
                    Return False
                End If
            End Function

            Public Sub ParseExpression(ByVal RegExpElement As XElement)
                Dim _dest As Integer = -1
                Dim _append As Boolean = False
                Dim _input As String = Nothing
                Dim _output As String = Nothing

                Dim _clear As Boolean = False
                Dim _cs As Boolean = False
                Dim _repeat As Boolean = False
                Dim _exp As String = Nothing
                Dim _noclean As String = Nothing
                Dim _trim As String = Nothing
                Dim _encode As String = Nothing
                Dim _fixamp As String = Nothing

                Dim tmpExpression As XElement = RegExpElement.Element("expression")

                If Not IsNothing(RegExpElement.Attribute("dest")) Then
                    If Not String.IsNullOrEmpty(RegExpElement.Attribute("dest").Value) Then
                        If RegExpElement.Attribute("dest").Value.EndsWith("+") Then
                            _dest = Convert.ToInt32(RegExpElement.Attribute("dest").Value.Replace("+", "")) - 1
                            _append = True
                        Else
                            _dest = Convert.ToInt32(RegExpElement.Attribute("dest").Value) - 1
                            _append = False
                        End If
                    End If
                Else
                    Return
                End If

                If Not IsNothing(RegExpElement.Attribute("input")) Then
                    If Not String.IsNullOrEmpty(RegExpElement.Attribute("input").Value) Then
                        _input = ReplaceBuffers(RegExpElement.Attribute("input").Value)
                    Else
                        _input = String.Empty
                    End If
                Else
                    _input = Me._buffers(0)
                End If

                If Not IsNothing(RegExpElement.Attribute("output")) Then
                    If Not String.IsNullOrEmpty(RegExpElement.Attribute("output").Value) Then
                        _output = RegExpElement.Attribute("output").Value
                    Else
                        _output = String.Empty
                    End If
                Else
                    _output = String.Empty
                End If

                If Not IsNothing(tmpExpression) Then
                    If Not IsNothing(tmpExpression.Attribute("noclean")) Then
                        If Not String.IsNullOrEmpty(tmpExpression.Attribute("noclean").Value) Then
                            _noclean = tmpExpression.Attribute("noclean").Value
                        Else
                            _noclean = String.Empty
                        End If
                    Else
                        _noclean = String.Empty
                    End If

                    If Not IsNothing(tmpExpression.Attribute("trim")) Then
                        If Not String.IsNullOrEmpty(tmpExpression.Attribute("trim").Value) Then
                            _trim = tmpExpression.Attribute("trim").Value
                        Else
                            _trim = String.Empty
                        End If
                    Else
                        _trim = String.Empty
                    End If

                    If Not IsNothing(tmpExpression.Attribute("fixamp")) Then
                        If Not String.IsNullOrEmpty(tmpExpression.Attribute("fixamp").Value) Then
                            _fixamp = tmpExpression.Attribute("fixamp").Value
                        Else
                            _fixamp = String.Empty
                        End If
                    Else
                        _fixamp = String.Empty
                    End If

                    If Not IsNothing(tmpExpression.Attribute("encode")) Then
                        If Not String.IsNullOrEmpty(tmpExpression.Attribute("encode").Value) Then
                            _encode = tmpExpression.Attribute("encode").Value
                        Else
                            _encode = String.Empty
                        End If
                    Else
                        _encode = String.Empty
                    End If

                    If Not IsNothing(tmpExpression.Attribute("clear")) Then
                        If Not String.IsNullOrEmpty(tmpExpression.Attribute("clear").Value) Then
                            If tmpExpression.Attribute("clear").Value = "yes" Then
                                _clear = True
                            Else
                                _clear = False
                            End If
                        Else
                            _clear = False
                        End If
                    Else
                        _clear = False
                    End If

                    If Not IsNothing(tmpExpression.Attribute("repeat")) Then
                        If Not String.IsNullOrEmpty(tmpExpression.Attribute("repeat").Value) Then
                            If tmpExpression.Attribute("repeat").Value = "yes" Then
                                _repeat = True
                            Else
                                _repeat = False
                            End If
                        Else
                            _repeat = False
                        End If
                    Else
                        _repeat = False
                    End If

                    If Not IsNothing(tmpExpression.Value) Then
                        If Not String.IsNullOrEmpty(tmpExpression.Value) Then
                            _exp = tmpExpression.Value
                        Else
                            _exp = "(.*)"
                        End If
                    Else
                        _exp = "(.*)"
                    End If

                    If Not IsNothing(tmpExpression.Attribute("cs")) Then
                        If Not String.IsNullOrEmpty(tmpExpression.Attribute("cs").Value) Then
                            If tmpExpression.Attribute("cs").Value = "yes" Then
                                _cs = True
                            Else
                                _cs = False
                            End If
                        Else
                            _cs = False
                        End If
                    Else
                        _cs = False
                    End If

                    If _clear Then
                        Me._buffers(_dest) = String.Empty
                    End If

                    ExpressionCompile(_dest, _append, _input, _output, _cs, _repeat, _
                     _exp, _noclean, _trim, _encode, _fixamp)
                End If
            End Sub

            Public Function ParseFunction(ByVal FunctionName As String) As String
                Dim _FunctionName As String
                Dim WhereToFind As Integer
                Dim FunctionElement As XElement
                Dim _FunctionClearBuffers As Boolean

                If Not HasFunction(FunctionName) Then
                    Return ""
                End If

                FunctionElement = XmlScraper.Root.Element(FunctionName)

                _FunctionName = FunctionElement.Name.ToString()

                If Not IsNothing(FunctionElement.Attribute("clearbuffers")) Then
                    If Not String.IsNullOrEmpty(FunctionElement.Attribute("clearbuffers").Value) Then
                        If Not FunctionElement.Attribute("clearbuffers").Value = "no" Then
                            _FunctionClearBuffers = True
                        Else
                            _FunctionClearBuffers = False
                        End If
                    Else
                        _FunctionClearBuffers = True
                    End If
                Else
                    _FunctionClearBuffers = True
                End If

                WhereToFind = Convert.ToInt32(FunctionElement.Attribute("dest").Value) - 1

                For Each RegExpItem As XElement In FunctionElement.Elements("RegExp")
                    ParseNext(RegExpItem)
                Next
                Dim ret As String = Me._buffers(WhereToFind)

                If _FunctionClearBuffers Then
                    ClearBuffers()
                End If

                Return ret
            End Function

            Public Sub setBuffer(ByVal Index As Integer, ByVal strValue As String)
                Me._buffers(Index - 1) = strValue
            End Sub

            Friend Sub LoadFromXDocument(ByVal doc As XDocument)
                Me._xmlscraper = doc
                Me._content = Me._xmlscraper.Root.Attribute("content").Value
            End Sub

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

            Private Function CheckCondition(ByVal RegExpElement As XElement) As Boolean
                If Not String.IsNullOrEmpty(RegExpElement.Attribute("conditional").Value) Then
                    Dim _condition As String
                    Dim _conditionInverse As Boolean

                    If RegExpElement.Attribute("conditional").Value.StartsWith("!") Then
                        _condition = RegExpElement.Attribute("conditional").Value.Substring(1, RegExpElement.Attribute("conditional").Value.Length - 1)
                        _conditionInverse = True
                    Else
                        _condition = RegExpElement.Attribute("conditional").Value
                        _conditionInverse = False
                    End If

                    Dim conditional As ScraperSetting = Settings.First(Function(n) n.ID = _condition)

                    If Not IsNothing(conditional) Then
                        If _conditionInverse Then
                            Return Not Convert.ToBoolean(conditional.Parameter)
                        Else
                            Return Convert.ToBoolean(conditional.Parameter)
                        End If
                    Else
                        Return False
                    End If
                End If
                Return True
            End Function

            Private Sub ClearBuffers()
                For i As Integer = 0 To MaxScraperBuffers - 1
                    Me._buffers(i) = String.Empty
                Next
            End Sub

            Private Sub ExpressionCompile(ByVal _dest As Integer, ByVal _append As Boolean, ByVal _input As String, ByVal _output As String, ByVal _cs As Boolean, ByVal _repeat As Boolean, _
                ByVal _exp As String, ByVal _noclean As String, ByVal _trim As String, ByVal _encode As String, ByVal _fixamp As String)
                Dim RegExp As Regex

                If String.IsNullOrEmpty(_exp) Then
                    _exp = "(.*)"
                End If

                If _cs Then
                    RegExp = New Regex(ReplaceBuffers(_exp), RegexOptions.Singleline)
                Else
                    RegExp = New Regex(ReplaceBuffers(_exp), RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                End If

                If RegExp.Match(_input).Success Then
                    Dim strReturn As String = String.Empty

                    If _repeat Then
                        Dim AllMatches As MatchCollection = RegExp.Matches(_input)

                        For i As Integer = 0 To AllMatches.Count - 1
                            Dim tmpString As String = _output

                            For iReplace As Integer = 1 To 9
                                Dim ReplaceString As String = String.Format("\{0}", Convert.ToString(iReplace))
                                If tmpString.Contains(ReplaceString) Then
                                    Dim bUrlEncode As Boolean = False, bCleanString As Boolean = False, bTrimString As Boolean = False
                                    If Not _noclean.Contains(Convert.ToString(iReplace)) Then
                                        bCleanString = True
                                    End If
                                    If _trim.Contains(Convert.ToString(iReplace)) Then
                                        bTrimString = True
                                    End If
                                    If _encode.Contains(Convert.ToString(iReplace)) Then
                                        bUrlEncode = True
                                    End If
                                    tmpString = tmpString.Replace(ReplaceString, PostMatch(AllMatches(i).Groups(iReplace).Value, bUrlEncode, bCleanString, bTrimString))
                                End If
                            Next
                            strReturn += tmpString
                        Next
                        strReturn = ReplaceBuffers(strReturn)
                    Else
                        strReturn = _output
                        Dim SingleMatch As Match = RegExp.Match(_input)

                        For i As Integer = 1 To 9
                            Dim ReplaceString As String = String.Format("\{0}", Convert.ToString(i))
                            If strReturn.Contains(ReplaceString) Then
                                If strReturn.Contains(ReplaceString) Then
                                    Dim bUrlEncode As Boolean = False, bCleanString As Boolean = False, bTrimString As Boolean = False
                                    If Not _noclean.Contains(Convert.ToString(i)) Then
                                        bCleanString = True
                                    End If
                                    If _trim.Contains(Convert.ToString(i)) Then
                                        bTrimString = True
                                    End If
                                    If _encode.Contains(Convert.ToString(i)) Then
                                        bUrlEncode = True
                                    End If
                                    strReturn = strReturn.Replace(ReplaceString, PostMatch(SingleMatch.Groups(i).Value, bUrlEncode, bCleanString, bTrimString))
                                End If
                            End If
                        Next
                        strReturn = ReplaceBuffers(strReturn)
                    End If

                    If _append Then
                        Buffers(_dest) += strReturn
                    Else
                        Buffers(_dest) = strReturn
                    End If
                End If
            End Sub

            Private Sub ParseNext(ByVal RegExpElement As XElement)
                If Not IsNothing(RegExpElement.Attribute("conditional")) Then
                    If Not CheckCondition(RegExpElement) Then
                        Return
                    End If
                End If

                For Each RegExpItem As XElement In RegExpElement.Elements("RegExp")
                    ParseNext(RegExpItem)
                Next

                ParseExpression(RegExpElement)
            End Sub

            Private Function PostMatch(ByVal strToProcess As String, ByVal bUrlEncode As Boolean, ByVal bCleanString As Boolean, ByVal bTrimString As Boolean) As String
                Dim strReturn As String = strToProcess
                If bCleanString Then
                    strReturn = CleanString(strReturn)
                End If

                If bTrimString Then
                    strReturn = strReturn.Trim
                End If

                If bUrlEncode Then
                    strReturn = UrlInfo.UrlEncode(strReturn)
                End If

                strReturn = strReturn.Replace("&", "&amp;")
                strReturn = strReturn.Replace("&amp;amp;", "&amp;")
                strReturn = strReturn.Replace("&amp;quot;", "&quot;")
                strReturn = strReturn.Replace("&amp;apos;", "&apos;")
                strReturn = strReturn.Replace("&amp;lt;", "&lt;")
                strReturn = strReturn.Replace("&amp;gt;", "&gt;")
                strReturn = strReturn.Replace("&amp;amp;", "&amp;")
                Return strReturn
            End Function

            Private Function ReplaceBuffers(ByVal strReplace As String) As String
                Dim strReturn As String = strReplace
                For i As Integer = 20 To 1 Step -1
                    strReturn = strReturn.Replace(String.Concat("$$", i.ToString), Me._buffers(i - 1))
                Next

                If Not IsNothing(Me._settings) Then
                    For Each settingItem As ScraperSetting In Me._settings
                        If Not settingItem.Type = ScraperSetting.ScraperSettingType.sep Then
                            strReturn = strReturn.Replace(String.Format("$INFO[{0}]", settingItem.ID), settingItem.Parameter)
                        End If
                    Next
                End If

                Return strReturn.Replace("\n", vbLf)
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
