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

Imports EmberScraperModule.XMLScraper.MediaTags

Namespace XMLScraper
    Namespace ScraperLib

        Public Class ScraperFunction
            Inherits ScraperNode

#Region "Fields"

            Private Shadows m_Parent As ScraperFunctionContainer

            Private m_ClearBuffers As Boolean
            Private m_Dest As Integer
            Private m_RegExps As List(Of ScraperRegExp)
            Private m_SearchStringEncoding As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal sfcParent As ScraperFunctionContainer)
                Me.New()
                Parent = sfcParent
                If Parent.[GetType]().Name = "Include" Then
                    IsInclude = True
                Else
                    IsInclude = False
                End If
            End Sub

            Public Sub New(ByVal xFunctionElement As XElement, ByVal sfcParent As ScraperFunctionContainer)
                Me.New(sfcParent)
                Deserialize(xFunctionElement)
            End Sub

            Friend Sub New()
                MyBase.New(Nothing)
                RegExps = New List(Of ScraperRegExp)()
                Name = "NewFunction"
                ClearBuffers = True
                SearchStringEncoding = ""
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property ClearBuffers() As Boolean
                Get
                    Return m_ClearBuffers
                End Get
                Set(ByVal value As Boolean)
                    m_ClearBuffers = value
                End Set
            End Property

            Public Property Dest() As Integer
                Get
                    Return m_Dest
                End Get
                Set(ByVal value As Integer)
                    m_Dest = value
                End Set
            End Property

            Public ReadOnly Property FirstNode() As ScraperRegExp
                Get
                    If RegExps.Count > 0 Then
                        Return RegExps(0)
                    Else
                        Return Nothing
                    End If
                End Get
            End Property

            Public ReadOnly Property FunctionXML() As String
                Get
                    Dim strReturn As String = "<" & Name

                    If Not String.IsNullOrEmpty(Comment) Then
                        strReturn = strReturn.Insert(0, "<!--" & Comment & "-->" & vbLf)
                    End If

                    strReturn += String.Format(" dest=""{0}""", Dest)

                    If Not ClearBuffers Then
                        strReturn += " clearbuffers=""no"""
                    End If

                    If Not String.IsNullOrEmpty(SearchStringEncoding) Then
                        strReturn += String.Format(" SearchStringEncoding=""{0}""", SearchStringEncoding)
                    End If

                    strReturn += ">"

                    For Each item As ScraperRegExp In RegExps
                        strReturn += vbCr & vbLf & item.FunctionXML
                    Next

                    strReturn += vbCr & vbLf & "</" & Name & ">"

                    Return strReturn
                End Get
            End Property

            Public ReadOnly Property LastNode() As ScraperRegExp
                Get
                    If RegExps.Count > 0 Then
                        Return RegExps(RegExps.Count - 1)
                    Else
                        Return Nothing
                    End If
                End Get
            End Property

            Public Shadows Property Parent() As ScraperFunctionContainer
                Get
                    Return m_Parent
                End Get
                Set(ByVal value As ScraperFunctionContainer)
                    m_Parent = Value
                End Set
            End Property

            Public Property RegExps() As List(Of ScraperRegExp)
                Get
                    Return m_RegExps
                End Get
                Set(ByVal value As List(Of ScraperRegExp))
                    m_RegExps = value
                End Set
            End Property

            Public Property SearchStringEncoding() As String
                Get
                    Return m_SearchStringEncoding
                End Get
                Set(ByVal value As String)
                    m_SearchStringEncoding = value
                End Set
            End Property

            Public ReadOnly Property StandAloneXML() As String
                Get
                    Dim strReturn As String = "<" & Name

                    If Name = "CreateSearchUrl" AndAlso Not String.IsNullOrEmpty(SearchStringEncoding) Then
                        strReturn += String.Format(" SearchStringEncoding=""{0}""", SearchStringEncoding)
                    End If

                    If Not ClearBuffers Then
                        strReturn += " clearbuffers=""no"""
                    End If

                    strReturn += String.Format(" dest=""{0}"">", Dest)

                    strReturn += "</" & Name & ">"

                    Return strReturn
                End Get
            End Property

            Public ReadOnly Property XML() As String
                Get
                    Dim strReturn As String = vbCr & vbLf & indent

                    If Not String.IsNullOrEmpty(Comment) Then
                        strReturn += "<!-- " & Comment & " -->" & vbCr & vbLf & indent
                    End If

                    strReturn += "<" & Name

                    If Not String.IsNullOrEmpty(SearchStringEncoding) Then
                        strReturn += String.Format(" SearchStringEncoding=""{0}""", SearchStringEncoding)
                    End If

                    If Not ClearBuffers Then
                        strReturn += " clearbuffers=""no"""
                    End If

                    strReturn += String.Format(" dest=""{0}"">", Dest)

                    For Each RegExpItem As ScraperRegExp In RegExps
                        strReturn += vbCr & vbLf
                        strReturn += RegExpItem.IndentedCompleteXML
                    Next
                    strReturn += vbCr & vbLf & indent & "</" & Name & ">"
                    Return strReturn
                End Get
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub AddRegExp(ByVal strRegExp As String)
                If Me.[GetType]().Name <> "ScraperExpression" Then
                    Dim xRegExp As XElement

                    Try
                        xRegExp = XElement.Parse(strRegExp)
                    Catch
                        Return
                    End Try

                    AddRegExp(xRegExp)
                End If
            End Sub

            Public Sub AddRegExp(ByVal strConditional As String, ByVal bInverseCondition As Boolean, ByVal bAppend As Boolean, ByVal strInput As String, ByVal strOutput As String, ByVal intDest As Integer)
                RegExps.Add(New ScraperRegExp(Me))
                RegExps(RegExps.Count - 1).Conditional = strConditional
                RegExps(RegExps.Count - 1).ConditionalInverse = bInverseCondition
                RegExps(RegExps.Count - 1).Dest = intDest
                RegExps(RegExps.Count - 1).Append = bAppend
                RegExps(RegExps.Count - 1).Input = strInput
                RegExps(RegExps.Count - 1).Output = strOutput
            End Sub

            Public Sub AddRegExp()
                RegExps.Add(New ScraperRegExp(Me))
            End Sub

            Public Sub AddRegExp(ByVal xRegExp As XElement)
                RegExps.Add(New ScraperRegExp(xRegExp, Me))
            End Sub

            Public Overloads Overrides Function CollectSettings() As List(Of SettingsUsed)
                Dim sTemp As String = String.Empty
                Dim sulistTemp As List(Of SettingsUsed) = MyBase.CollectSettings()

                For i As Integer = 0 To RegExps.Count - 1
                    Dim RegExpTempList As List(Of SettingsUsed) = RegExps(i).CollectSettings()

                    For Each suItem As SettingsUsed In RegExpTempList
                        sTemp = suItem.ID

                        Dim foundItem As SettingsUsed = sulistTemp.Find(Function(n) n.ID = sTemp)

                        If Not IsNothing(foundItem) Then
                            If Not foundItem.ImplementingFunctions.Contains(Me.Name) Then
                                foundItem.ImplementingFunctions.Add(Me.Name)
                            End If
                        Else
                            suItem.ImplementingFunctions.Add(Me.Name)
                            sulistTemp.Add(suItem)
                        End If
                    Next
                Next

                Return sulistTemp
            End Function

            Public Overloads Overrides Sub Deserialize(ByVal xFunctionElement As XElement)
                MyBase.Deserialize(xFunctionElement)
                If Not IsNothing(xFunctionElement.PreviousNode) AndAlso xFunctionElement.PreviousNode.NodeType = System.Xml.XmlNodeType.Comment Then
                    Comment = DirectCast(xFunctionElement.PreviousNode, XComment).Value.Trim()
                End If

                If Not IsNothing(xFunctionElement.Attribute("clearbuffers")) Then
                    If Not String.IsNullOrEmpty(xFunctionElement.Attribute("clearbuffers").Value) Then
                        If xFunctionElement.Attribute("clearbuffers").Value = "no" Then
                            DirectCast(Me, ScraperFunction).ClearBuffers = False
                        Else
                            DirectCast(Me, ScraperFunction).ClearBuffers = True
                        End If
                    Else
                        DirectCast(Me, ScraperFunction).ClearBuffers = True
                    End If
                Else
                    DirectCast(Me, ScraperFunction).ClearBuffers = True
                End If

                For Each RegExp As XElement In xFunctionElement.Elements("RegExp")
                    RegExps.Add(New ScraperRegExp(RegExp, Me))
                Next

                DirectCast(Me, ScraperFunction).Dest = Convert.ToInt32(xFunctionElement.Attribute("dest").Value)

                If Not IsNothing(xFunctionElement.Attribute("SearchStringEncoding")) Then
                    SearchStringEncoding = xFunctionElement.Attribute("SearchStringEncoding").Value
                End If
            End Sub

            Public Overloads Overrides Function GetEvaluation(ByVal parentPath As String) As List(Of ScraperEvaluation)
                Dim FunctionErrors As New List(Of ScraperEvaluation)()
                Dim Path As String = parentPath & ":" & Me.Name

                If Dest < 0 OrElse Dest > 20 Then
                    Dim tmpMessage As String = "Function Destination is out of range, Scrapers only support buffers 1-20"
                    FunctionErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.FunctionDestination, Path, tmpMessage))
                End If

                If RegExps.Count > 0 Then
                    Dim Search As New Regex("Create(.*?)SearchUrl")
                    Dim Results As New Regex("Get(.*?)SearchResults")
                    Dim Details As New Regex("Get(.*?)Details")
                    Dim List As New Regex("Get(.*?)List")
                    If Search.Match(Me.Name).Success Then
                        Dim tmp As New Regex("<url>.*?</url>")

                        For Each item As ScraperRegExp In RegExps
                            If Not tmp.Match(item.Output).Success Then
                                Dim tmpMessage As String = "The root RegExps of this function should create a enveloping tag for url (<url>\1</url>) to indicate how it should be processed"
                                FunctionErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Output, Path & ":0", tmpMessage))
                            End If
                        Next
                    ElseIf Results.Match(Me.Name).Success Then
                        Dim tmp As New Regex("<results[^>]*>.*?</results>")

                        For Each item As ScraperRegExp In RegExps
                            If Not tmp.Match(item.Output).Success Then
                                Dim tmpMessage As String = "The root RegExps of this function should create a enveloping tag for results (<results>\1</results>) to indicate how it should be processed"
                                FunctionErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Output, Path & ":0", tmpMessage))
                            End If
                        Next
                    ElseIf Details.Match(Me.Name).Success Then
                        Dim tmp As New Regex("<details[^>]*>.*?</details>")

                        For Each item As ScraperRegExp In RegExps
                            If Not tmp.Match(item.Output).Success Then
                                Dim tmpMessage As String = "The root RegExps of this function should create a enveloping tag for details (<details>\1</details>) to indicate how it should be processed"
                                FunctionErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Output, Path & ":0", tmpMessage))
                            End If
                        Next
                    ElseIf List.Match(Me.Name).Success Then
                        If List.Match(Me.Name).Groups(1).Value = "Issue" Then

                            Dim tmp As New Regex("<issuelist[^>]*>.*?</issuelist>")
                            For Each item As ScraperRegExp In RegExps
                                If Not tmp.Match(item.Output).Success Then
                                    Dim tmpMessage As String = "The root RegExps of this function should create a enveloping tag for details (<issuelist>\1</issuelist>) to indicate how it should be processed"
                                    FunctionErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Output, Path & ":0", tmpMessage))
                                End If
                            Next
                        ElseIf List.Match(Me.Name).Groups(1).Value = "Episode" Then
                            Dim tmp As New Regex("<episodelist[^>]*>.*?</episodelist>")
                            For Each item As ScraperRegExp In RegExps
                                If Not tmp.Match(item.Output).Success Then
                                    Dim tmpMessage As String = "The root RegExps of this function should create a enveloping tag for details (<episodelist>\1</episodelist>) to indicate how it should be processed"
                                    FunctionErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Output, Path & ":0", tmpMessage))
                                End If
                            Next
                        End If
                    Else
                        Dim tmp As New Regex("<(settings|url|results|details)[^>]*>.*?</\1>")

                        For i As Integer = 0 To RegExps.Count - 1
                            If Not tmp.Match(RegExps(i).Output).Success Then
                                Dim tmpMessage As String = "The root RegExps of this function should create a enveloping tag for details, results, settings, or url(i.e <url>\1</url>) to indicate how it should be processed"
                                FunctionErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Output, Path & ":0", tmpMessage))
                            End If
                        Next
                    End If
                End If

                For Each item As ScraperRegExp In RegExps
                    FunctionErrors.AddRange(item.GetEvaluation(Path))
                Next

                Return FunctionErrors
            End Function

            Public Sub InsertRegExp(ByVal Index As Integer, ByVal scRegExp As ScraperRegExp)
                RegExps.Insert(Index, scRegExp)
            End Sub

            Public Sub RemoveRegExp(ByVal Index As Integer)
                RegExps.RemoveAt(Index)
            End Sub

            Protected Friend Function GetAvailableConditionals() As Dictionary(Of String, String)
                Dim tmp As New Dictionary(Of String, String)()
                For Each item As ScraperRegExp In RegExps
                    For Each kvItem As KeyValuePair(Of String, String) In item.GetAvailableConditionals()
                        If kvItem.Key = "urls" Then
                            If tmp.ContainsKey("urls") Then
                                tmp("urls") = tmp("urls") & ";" & kvItem.Value
                            Else
                                tmp("urls") = kvItem.Value
                            End If
                        Else
                            If Not tmp.ContainsKey(kvItem.Key) Then
                                tmp.Add(kvItem.Key, kvItem.Value)
                            End If
                        End If
                    Next
                Next
                Return tmp
            End Function

            Protected Friend Function GetAvailableSettings() As Dictionary(Of String, String)
                Dim tmp As New Dictionary(Of String, String)()
                For Each item As ScraperRegExp In RegExps
                    For Each kvItem As KeyValuePair(Of String, String) In item.GetAvailableSettings()
                        If kvItem.Key = "urls" Then
                            If tmp.ContainsKey("urls") Then
                                tmp("urls") = tmp("urls") & ";" & kvItem.Value
                            Else
                                tmp("urls") = kvItem.Value
                            End If
                        Else
                            If Not tmp.ContainsKey(kvItem.Key) Then
                                tmp.Add(kvItem.Key, kvItem.Value)
                            End If
                        End If
                    Next
                Next
                Return tmp
            End Function

            Protected Friend Overloads Overrides Function GetIndex() As Integer
                If Not IsNothing(Parent) Then
                    Return Parent.ScraperFunctions.IndexOf(Me)
                Else
                    Return -1
                End If
            End Function

            Private Function GetRegExp(ByVal Index As Integer) As ScraperRegExp
                Return RegExps(Index)
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
