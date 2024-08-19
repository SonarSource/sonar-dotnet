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

Imports EmberScraperModule.XMLScraper.Utilities

Namespace XMLScraper
    Namespace ScraperLib

        Public Class ScraperExpression
            Inherits ScraperNode

#Region "Fields"

            Private m_CaseSensitive As Boolean
            Private m_Clear As Boolean
            Private m_Encode1 As Boolean
            Private m_Encode2 As Boolean
            Private m_Encode3 As Boolean
            Private m_Encode4 As Boolean
            Private m_Encode5 As Boolean
            Private m_Encode6 As Boolean
            Private m_Encode7 As Boolean
            Private m_Encode8 As Boolean
            Private m_Encode9 As Boolean
            Private m_Expression As String
            Private m_NoClean1 As Boolean
            Private m_NoClean2 As Boolean
            Private m_NoClean3 As Boolean
            Private m_NoClean4 As Boolean
            Private m_NoClean5 As Boolean
            Private m_NoClean6 As Boolean
            Private m_NoClean7 As Boolean
            Private m_NoClean8 As Boolean
            Private m_NoClean9 As Boolean
            Private m_Repeat As Boolean
            Private m_Trim1 As Boolean
            Private m_Trim2 As Boolean
            Private m_Trim3 As Boolean
            Private m_Trim4 As Boolean
            Private m_Trim5 As Boolean
            Private m_Trim6 As Boolean
            Private m_Trim7 As Boolean
            Private m_Trim8 As Boolean
            Private m_Trim9 As Boolean

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal xExpression As XElement, ByVal ndParent As ScraperNode)
                Me.New(ndParent)
                Deserialize(xExpression)
            End Sub

            Public Sub New(ByVal ndParent As ScraperNode)
                MyBase.New(ndParent)
                Me.Expression = ""
                Me.Clear = False
                Me.Repeat = False
                Me.CaseSensitive = False
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property CaseSensitive() As Boolean
                Get
                    Return m_CaseSensitive
                End Get
                Set(ByVal value As Boolean)
                    m_CaseSensitive = Value
                End Set
            End Property

            Public Property Clear() As Boolean
                Get
                    Return m_Clear
                End Get
                Set(ByVal value As Boolean)
                    m_Clear = Value
                End Set
            End Property

            Public Property Encode1() As Boolean
                Get
                    Return m_Encode1
                End Get
                Set(ByVal value As Boolean)
                    m_Encode1 = Value
                End Set
            End Property

            Public Property Encode2() As Boolean
                Get
                    Return m_Encode2
                End Get
                Set(ByVal value As Boolean)
                    m_Encode2 = Value
                End Set
            End Property

            Public Property Encode3() As Boolean
                Get
                    Return m_Encode3
                End Get
                Set(ByVal value As Boolean)
                    m_Encode3 = Value
                End Set
            End Property

            Public Property Encode4() As Boolean
                Get
                    Return m_Encode4
                End Get
                Set(ByVal value As Boolean)
                    m_Encode4 = Value
                End Set
            End Property

            Public Property Encode5() As Boolean
                Get
                    Return m_Encode5
                End Get
                Set(ByVal value As Boolean)
                    m_Encode5 = Value
                End Set
            End Property

            Public Property Encode6() As Boolean
                Get
                    Return m_Encode6
                End Get
                Set(ByVal value As Boolean)
                    m_Encode6 = Value
                End Set
            End Property

            Public Property Encode7() As Boolean
                Get
                    Return m_Encode7
                End Get
                Set(ByVal value As Boolean)
                    m_Encode7 = Value
                End Set
            End Property

            Public Property Encode8() As Boolean
                Get
                    Return m_Encode8
                End Get
                Set(ByVal value As Boolean)
                    m_Encode8 = Value
                End Set
            End Property

            Public Property Encode9() As Boolean
                Get
                    Return m_Encode9
                End Get
                Set(ByVal value As Boolean)
                    m_Encode9 = Value
                End Set
            End Property

            Public Property EncodeString() As String
                Get
                    Return EncodeToString()
                End Get
                Set(ByVal value As String)
                    ParseEncodeFromString(Value)
                End Set
            End Property

            Public Property Expression() As String
                Get
                    Return m_Expression
                End Get
                Set(ByVal value As String)
                    m_Expression = Value
                End Set
            End Property

            Public Property NoClean1() As Boolean
                Get
                    Return m_NoClean1
                End Get
                Set(ByVal value As Boolean)
                    m_NoClean1 = Value
                End Set
            End Property

            Public Property NoClean2() As Boolean
                Get
                    Return m_NoClean2
                End Get
                Set(ByVal value As Boolean)
                    m_NoClean2 = Value
                End Set
            End Property

            Public Property NoClean3() As Boolean
                Get
                    Return m_NoClean3
                End Get
                Set(ByVal value As Boolean)
                    m_NoClean3 = Value
                End Set
            End Property

            Public Property NoClean4() As Boolean
                Get
                    Return m_NoClean4
                End Get
                Set(ByVal value As Boolean)
                    m_NoClean4 = Value
                End Set
            End Property

            Public Property NoClean5() As Boolean
                Get
                    Return m_NoClean5
                End Get
                Set(ByVal value As Boolean)
                    m_NoClean5 = Value
                End Set
            End Property

            Public Property NoClean6() As Boolean
                Get
                    Return m_NoClean6
                End Get
                Set(ByVal value As Boolean)
                    m_NoClean6 = Value
                End Set
            End Property

            Public Property NoClean7() As Boolean
                Get
                    Return m_NoClean7
                End Get
                Set(ByVal value As Boolean)
                    m_NoClean7 = Value
                End Set
            End Property

            Public Property NoClean8() As Boolean
                Get
                    Return m_NoClean8
                End Get
                Set(ByVal value As Boolean)
                    m_NoClean8 = Value
                End Set
            End Property

            Public Property NoClean9() As Boolean
                Get
                    Return m_NoClean9
                End Get
                Set(ByVal value As Boolean)
                    m_NoClean9 = Value
                End Set
            End Property

            Public Property NoCleanString() As String
                Get
                    Return NoCleanToString()
                End Get
                Set(ByVal value As String)
                    ParseNoCleanFromString(Value)
                End Set
            End Property

            Public Property Repeat() As Boolean
                Get
                    Return m_Repeat
                End Get
                Set(ByVal value As Boolean)
                    m_Repeat = Value
                End Set
            End Property

            Public Property Trim1() As Boolean
                Get
                    Return m_Trim1
                End Get
                Set(ByVal value As Boolean)
                    m_Trim1 = Value
                End Set
            End Property

            Public Property Trim2() As Boolean
                Get
                    Return m_Trim2
                End Get
                Set(ByVal value As Boolean)
                    m_Trim2 = Value
                End Set
            End Property

            Public Property Trim3() As Boolean
                Get
                    Return m_Trim3
                End Get
                Set(ByVal value As Boolean)
                    m_Trim3 = Value
                End Set
            End Property

            Public Property Trim4() As Boolean
                Get
                    Return m_Trim4
                End Get
                Set(ByVal value As Boolean)
                    m_Trim4 = Value
                End Set
            End Property

            Public Property Trim5() As Boolean
                Get
                    Return m_Trim5
                End Get
                Set(ByVal value As Boolean)
                    m_Trim5 = Value
                End Set
            End Property

            Public Property Trim6() As Boolean
                Get
                    Return m_Trim6
                End Get
                Set(ByVal value As Boolean)
                    m_Trim6 = Value
                End Set
            End Property

            Public Property Trim7() As Boolean
                Get
                    Return m_Trim7
                End Get
                Set(ByVal value As Boolean)
                    m_Trim7 = Value
                End Set
            End Property

            Public Property Trim8() As Boolean
                Get
                    Return m_Trim8
                End Get
                Set(ByVal value As Boolean)
                    m_Trim8 = Value
                End Set
            End Property

            Public Property Trim9() As Boolean
                Get
                    Return m_Trim9
                End Get
                Set(ByVal value As Boolean)
                    m_Trim9 = Value
                End Set
            End Property

            Public Property TrimString() As String
                Get
                    Return TrimToString()
                End Get
                Set(ByVal value As String)
                    ParseTrimFromString(Value)
                End Set
            End Property

            Public ReadOnly Property XML() As String
                Get
                    Dim tmpString As String = GetXMLString()

                    If Not String.IsNullOrEmpty(Comment) Then
                        tmpString = tmpString.Insert(0, String.Format("<!--{0}-->" & vbCr & vbLf, Comment))
                    End If
                    Return tmpString
                End Get
            End Property

            Friend ReadOnly Property FunctionXML() As String
                Get
                    Dim tmpString As String = indent.Remove(0, 1) & GetXMLString()

                    If Not String.IsNullOrEmpty(Comment) Then
                        tmpString = tmpString.Insert(0, indent.Remove(0, 1) & String.Format("<!--{0}-->" & vbCr & vbLf, Comment))
                    End If

                    Return tmpString
                End Get
            End Property

            Friend ReadOnly Property IndentedXML() As String
                Get
                    Dim tmpString As String = indent & GetXMLString()

                    If Not String.IsNullOrEmpty(Comment) Then
                        tmpString = tmpString.Insert(0, indent & String.Format("<!--{0}-->" & vbCr & vbLf, Comment))
                    End If
                    Return tmpString
                End Get
            End Property

#End Region 'Properties

#Region "Methods"

            Public Overloads Overrides Function CollectSettings() As List(Of SettingsUsed)
                Dim sTemp As String = String.Empty
                Dim sulistTemp As List(Of SettingsUsed) = MyBase.CollectSettings()

                Dim Replacements As New Regex("\$INFO\[(.*?)\]")

                For Each mItem As Match In Replacements.Matches(Expression)
                    sTemp = mItem.Groups(1).Value

                    Dim thisMatch As SettingsUsed = sulistTemp.Find(Function(n) n.ID = sTemp)

                    If Not IsNothing(thisMatch) Then
                        sulistTemp.Add(thisMatch)
                    End If
                Next
                Return sulistTemp
            End Function

            Public Overloads Overrides Sub Deserialize(ByVal xExpression As XElement)
                MyBase.Deserialize(xExpression)

                If Not IsNothing(xExpression.PreviousNode) AndAlso xExpression.PreviousNode.NodeType = System.Xml.XmlNodeType.Comment Then
                    Me.Comment = DirectCast(xExpression.PreviousNode, XComment).Value.Trim()
                End If

                If Not IsNothing(xExpression.Attribute("clear")) Then
                    If String.IsNullOrEmpty(xExpression.Attribute("clear").Value) <> True Then
                        If xExpression.Attribute("clear").Value = "yes" Then
                            DirectCast(Me, ScraperExpression).Clear = True
                        Else
                            DirectCast(Me, ScraperExpression).Clear = False
                        End If
                    End If
                End If

                If Not IsNothing(xExpression.Attribute("cs")) Then
                    If String.IsNullOrEmpty(xExpression.Attribute("cs").Value) <> True Then
                        If xExpression.Attribute("cs").Value = "true" Then
                            DirectCast(Me, ScraperExpression).CaseSensitive = True
                        Else
                            DirectCast(Me, ScraperExpression).CaseSensitive = False
                        End If
                    End If
                End If

                If Not IsNothing(xExpression.Attribute("repeat")) Then
                    If String.IsNullOrEmpty(xExpression.Attribute("repeat").Value) <> True Then
                        If xExpression.Attribute("repeat").Value = "yes" Then
                            DirectCast(Me, ScraperExpression).Repeat = True
                        Else
                            DirectCast(Me, ScraperExpression).Repeat = False
                        End If
                    End If
                End If

                If Not IsNothing(xExpression.Attribute("noclean")) Then
                    If String.IsNullOrEmpty(xExpression.Attribute("noclean").Value) <> True Then
                        DirectCast(Me, ScraperExpression).ParseNoCleanFromString(xExpression.Attribute("noclean").Value)
                    End If
                End If

                If Not IsNothing(xExpression.Attribute("trim")) Then
                    If String.IsNullOrEmpty(xExpression.Attribute("trim").Value) <> True Then
                        DirectCast(Me, ScraperExpression).ParseTrimFromString(xExpression.Attribute("trim").Value)
                    End If
                End If

                If Not IsNothing(xExpression.Attribute("encode")) Then
                    If String.IsNullOrEmpty(xExpression.Attribute("encode").Value) <> True Then
                        DirectCast(Me, ScraperExpression).ParseEncodeFromString(xExpression.Attribute("encode").Value)
                    End If
                End If

                If Not IsNothing(xExpression.Value) Then
                    If String.IsNullOrEmpty(xExpression.Value) <> True Then
                        DirectCast(Me, ScraperExpression).Expression = xExpression.Value
                    Else
                        DirectCast(Me, ScraperExpression).Expression = ""
                    End If
                Else
                    DirectCast(Me, ScraperExpression).Expression = ""
                End If
            End Sub

            Public Function EncodeToString() As String
                Dim tmp As String = ""

                If Encode1 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "1"
                    Else
                        tmp += ",1"
                    End If
                End If

                If Encode2 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "2"
                    Else
                        tmp += ",2"
                    End If
                End If

                If Encode3 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "3"
                    Else
                        tmp += ",3"
                    End If
                End If

                If Encode4 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "4"
                    Else
                        tmp += ",4"
                    End If
                End If

                If Encode5 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "5"
                    Else
                        tmp += ",5"
                    End If
                End If

                If Encode6 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "6"
                    Else
                        tmp += ",6"
                    End If
                End If

                If Encode7 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "7"
                    Else
                        tmp += ",7"
                    End If
                End If

                If Encode8 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "8"
                    Else
                        tmp += ",8"
                    End If
                End If

                If Encode9 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "9"
                    Else
                        tmp += ",9"
                    End If
                End If

                Return tmp
            End Function

            Public Overloads Overrides Function GetEvaluation(ByVal parentPath As String) As List(Of ScraperEvaluation)
                Dim ExpressionErrors As New List(Of ScraperEvaluation)()
                Dim Path As String = parentPath & ":" & "expression"
                Dim Finder As Regex

                If Not String.IsNullOrEmpty(Expression) Then
                    Finder = New Regex("\$\$([0-9]+)")

                    If Finder.Match(Expression).Success Then
                        For Each item As Match In Finder.Matches(Expression)
                            Dim BufferReference As Integer = Convert.ToInt32(item.Groups(1).Value)

                            If BufferReference < 0 OrElse BufferReference > 20 Then
                                Dim tmpMessage As String = """" & item.Value & """ is out of range. Scrapers only support buffers ranging from 1 - 20"
                                ExpressionErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Expression, Path & ":expression", tmpMessage))
                            End If
                        Next
                    End If
                End If

                If Not String.IsNullOrEmpty(Expression) Then
                    Try
                        Dim tmp As New Regex(Expression)
                    Catch ex As Exception
                        ExpressionErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Expression, parentPath & ":expression", "Regular Expression Compile Error: " & ex.Message))
                    End Try
                End If

                Return ExpressionErrors
            End Function

            Public Function GetXMLString() As String
                Dim tmpstring As String = "<expression"

                If CaseSensitive Then
                    tmpstring += String.Format(" cs=""{0}""", "true")
                End If

                If Repeat Then
                    tmpstring += String.Format(" repeat=""{0}""", "yes")
                End If

                If Clear Then
                    tmpstring += String.Format(" clear=""{0}""", "yes")
                End If

                If Not String.IsNullOrEmpty(TrimToString()) Then
                    tmpstring += String.Format(" trim=""{0}""", TrimString)
                End If

                If Not String.IsNullOrEmpty(NoCleanToString()) Then
                    tmpstring += String.Format(" noclean=""{0}""", NoCleanString)
                End If

                If Not String.IsNullOrEmpty(EncodeToString()) Then
                    tmpstring += String.Format(" encode=""{0}""", EncodeString)
                End If

                If Not String.IsNullOrEmpty(Expression) Then
                    tmpstring += ">"
                    tmpstring += XmlUtilities.ReplaceEntities(Expression)
                    tmpstring += "</expression>"
                Else
                    tmpstring += "/>"
                End If

                Return tmpstring
            End Function

            Public Function NoCleanToString() As String
                Dim tmp As String = ""

                If NoClean1 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "1"
                    Else
                        tmp += ",1"
                    End If
                End If

                If NoClean2 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "2"
                    Else
                        tmp += ",2"
                    End If
                End If

                If NoClean3 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "3"
                    Else
                        tmp += ",3"
                    End If
                End If

                If NoClean4 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "4"
                    Else
                        tmp += ",4"
                    End If
                End If

                If NoClean5 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "5"
                    Else
                        tmp += ",5"
                    End If
                End If

                If NoClean6 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "6"
                    Else
                        tmp += ",6"
                    End If
                End If

                If NoClean7 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "7"
                    Else
                        tmp += ",7"
                    End If
                End If

                If NoClean8 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "8"
                    Else
                        tmp += ",8"
                    End If
                End If

                If NoClean9 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "9"
                    Else
                        tmp += ",9"
                    End If
                End If

                Return tmp
            End Function

            Public Sub ParseEncodeFromString(ByVal strEncodeProperty As String)
                If strEncodeProperty.Contains("1") Then
                    Encode1 = True
                End If

                If strEncodeProperty.Contains("2") Then
                    Encode2 = True
                End If

                If strEncodeProperty.Contains("3") Then
                    Encode3 = True
                End If

                If strEncodeProperty.Contains("4") Then
                    Encode4 = True
                End If

                If strEncodeProperty.Contains("5") Then
                    Encode5 = True
                End If

                If strEncodeProperty.Contains("6") Then
                    Encode6 = True
                End If

                If strEncodeProperty.Contains("7") Then
                    Encode7 = True
                End If

                If strEncodeProperty.Contains("8") Then
                    Encode8 = True
                End If

                If strEncodeProperty.Contains("9") Then
                    Encode9 = True
                End If
            End Sub

            Public Function TrimToString() As String
                Dim tmp As String = ""

                If Trim1 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "1"
                    Else
                        tmp += ",1"
                    End If
                End If

                If Trim2 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "2"
                    Else
                        tmp += ",2"
                    End If
                End If

                If Trim3 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "3"
                    Else
                        tmp += ",3"
                    End If
                End If

                If Trim4 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "4"
                    Else
                        tmp += ",4"
                    End If
                End If

                If Trim5 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "5"
                    Else
                        tmp += ",5"
                    End If
                End If

                If Trim6 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "6"
                    Else
                        tmp += ",6"
                    End If
                End If

                If Trim7 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "7"
                    Else
                        tmp += ",7"
                    End If
                End If

                If Trim8 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "8"
                    Else
                        tmp += ",8"
                    End If
                End If

                If Trim9 Then
                    If String.IsNullOrEmpty(tmp) Then
                        tmp = "9"
                    Else
                        tmp += ",9"
                    End If
                End If

                Return tmp
            End Function

            Protected Friend Overloads Overrides Function GetIndex() As Integer
                If Not IsNothing(Parent) Then

                    Return DirectCast(Parent, ScraperRegExp).RegExps.Count
                Else
                    Return -1
                End If
            End Function

            Protected Friend Sub ParseNoCleanFromString(ByVal strNoCleanProperty As String)
                If strNoCleanProperty.Contains("1") Then
                    NoClean1 = True
                End If

                If strNoCleanProperty.Contains("2") Then
                    NoClean2 = True
                End If

                If strNoCleanProperty.Contains("3") Then
                    NoClean3 = True
                End If

                If strNoCleanProperty.Contains("4") Then
                    NoClean4 = True
                End If

                If strNoCleanProperty.Contains("5") Then
                    NoClean5 = True
                End If

                If strNoCleanProperty.Contains("6") Then
                    NoClean6 = True
                End If

                If strNoCleanProperty.Contains("7") Then
                    NoClean7 = True
                End If

                If strNoCleanProperty.Contains("8") Then
                    NoClean8 = True
                End If

                If strNoCleanProperty.Contains("9") Then
                    NoClean9 = True
                End If
            End Sub

            Protected Friend Sub ParseTrimFromString(ByVal strTrimProperty As String)
                If strTrimProperty.Contains("1") Then
                    Trim1 = True
                End If

                If strTrimProperty.Contains("2") Then
                    Trim2 = True
                End If

                If strTrimProperty.Contains("3") Then
                    Trim3 = True
                End If

                If strTrimProperty.Contains("4") Then
                    Trim4 = True
                End If

                If strTrimProperty.Contains("5") Then
                    Trim5 = True
                End If

                If strTrimProperty.Contains("6") Then
                    Trim6 = True
                End If

                If strTrimProperty.Contains("7") Then
                    Trim7 = True
                End If

                If strTrimProperty.Contains("8") Then
                    Trim8 = True
                End If

                If strTrimProperty.Contains("9") Then
                    Trim9 = True
                End If
            End Sub

#End Region 'Methods

        End Class

    End Namespace
End Namespace
