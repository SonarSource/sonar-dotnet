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

        Public Class ScraperRegExp
            Inherits ScraperNode

#Region "Fields"

            Private m_Append As Boolean
            Private m_Conditional As String
            Private m_ConditionalInverse As Boolean
            Private m_Dest As Integer
            Private m_Expression As ScraperExpression
            Private m_Input As String
            Private m_Output As String
            Private m_RegExps As List(Of ScraperRegExp)

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal ndParent As ScraperNode)
                MyBase.New(ndParent)
                RegExps = New List(Of ScraperRegExp)()
                Conditional = ""
                ConditionalInverse = False
                Dest = 3
                Output = ""
                Input = ""
                Expression = New ScraperExpression(Me)
            End Sub

            Public Sub New(ByVal xRegExp As XElement, ByVal ndParent As ScraperNode)
                Me.New(ndParent)
                RegExps = New List(Of ScraperRegExp)()
                Deserialize(xRegExp)
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Append() As Boolean
                Get
                    Return m_Append
                End Get
                Set(ByVal value As Boolean)
                    m_Append = value
                End Set
            End Property

            Public Property Conditional() As String
                Get
                    Return m_Conditional
                End Get
                Set(ByVal value As String)
                    m_Conditional = value
                End Set
            End Property

            Public Property ConditionalInverse() As Boolean
                Get
                    Return m_ConditionalInverse
                End Get
                Set(ByVal value As Boolean)
                    m_ConditionalInverse = value
                End Set
            End Property

            Public Property Dest() As Integer
                Get
                    Return m_Dest
                End Get
                Set(ByVal value As Integer)
                    m_Dest = Value
                End Set
            End Property

            Public Property Expression() As ScraperExpression
                Get
                    Return m_Expression
                End Get
                Set(ByVal value As ScraperExpression)
                    m_Expression = value
                End Set
            End Property

            Public ReadOnly Property ExpressionNode() As ScraperExpression
                Get
                    Return Expression
                End Get
            End Property

            Public ReadOnly Property FirstRegExpNode() As ScraperRegExp
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
                    Return GetXML(XMLDisplayOption.[Function])
                End Get
            End Property

            Public Property Input() As String
                Get
                    Return m_Input
                End Get
                Set(ByVal value As String)
                    m_Input = value
                End Set
            End Property

            Public ReadOnly Property LastRegExpNode() As ScraperRegExp
                Get
                    If RegExps.Count > 0 Then
                        Return RegExps(RegExps.Count - 1)
                    Else
                        Return Nothing
                    End If
                End Get
            End Property

            Public ReadOnly Property NextNode() As ScraperRegExp
                Get
                    If Parent.[GetType]().Name = "ScraperFunction" Then
                        If DirectCast(Parent, ScraperFunction).RegExps.Count - 1 > Me.Index Then
                            Return DirectCast(Parent, ScraperFunction).RegExps(Index + 1)
                        Else
                            Return Nothing
                        End If
                    Else
                        If DirectCast(Parent, ScraperRegExp).RegExps.Count - 1 > Me.Index Then
                            Return DirectCast(Parent, ScraperRegExp).RegExps(Index + 1)
                        Else
                            Return Nothing
                        End If
                    End If
                End Get
            End Property

            Public Property Output() As String
                Get
                    Return m_Output
                End Get
                Set(ByVal value As String)
                    m_Output = value
                End Set
            End Property

            Public ReadOnly Property ParentFunction() As ScraperFunction
                Get
                    Dim item As ScraperNode = Me

                    While item.[GetType]().Name <> "ScraperFunction"
                        item = item.Parent
                    End While

                    Return DirectCast(item, ScraperFunction)
                End Get
            End Property

            Public ReadOnly Property PreviousNode() As ScraperRegExp
                Get
                    If Parent.[GetType]().Name = "ScraperFunction" Then
                        Dim Indexer As Integer = DirectCast(Parent, ScraperFunction).RegExps.IndexOf(Me)

                        If Indexer > 0 Then
                            Return DirectCast(Parent, ScraperFunction).RegExps(Indexer - 1)
                        Else
                            Return Nothing
                        End If
                    Else
                        Dim Indexer As Integer = DirectCast(Parent, ScraperRegExp).RegExps.IndexOf(Me)

                        If Indexer > 0 Then
                            Return DirectCast(Parent, ScraperRegExp).RegExps(Indexer - 1)
                        Else
                            Return Nothing
                        End If
                    End If
                End Get
            End Property

            Public Property RegExps() As List(Of ScraperRegExp)
                Get
                    Return m_RegExps
                End Get
                Set(ByVal value As List(Of ScraperRegExp))
                    m_RegExps = value
                End Set
            End Property

            Public ReadOnly Property RegExpXML() As String
                Get
                    Return GetXML(XMLDisplayOption.RegExpOnly)
                End Get
            End Property

            Public ReadOnly Property StandAloneXML() As String
                Get
                    Return GetXML(XMLDisplayOption.StandAlone)
                End Get
            End Property

            Friend ReadOnly Property IndentedCompleteXML() As String
                Get
                    Return GetXML(XMLDisplayOption.Complete)
                End Get
            End Property

            Private ReadOnly Property Container() As ScraperFunctionContainer
                Get
                    Dim item As ScraperNode = Me

                    While item.[GetType]().Name <> "ScraperFunction"
                        item = item.Parent
                    End While

                    Return DirectCast(item, ScraperFunction).Parent
                End Get
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub AddRegExp(ByVal strRegExp As String)
                Dim xRegExp As XElement

                Try
                    xRegExp = XElement.Parse(strRegExp)
                Catch
                    Return
                End Try

                AddRegExp(xRegExp)
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

                If Not IsNothing(DirectCast(Me, ScraperRegExp).Expression) Then
                    Dim ExpressionTempList As List(Of SettingsUsed) = Expression.CollectSettings()

                    For Each item As SettingsUsed In ExpressionTempList
                        sTemp = item.ID
                        Dim tmpSetting As SettingsUsed = sulistTemp.Find(Function(n) n.ID = sTemp)

                        If IsNothing(tmpSetting) Then
                            sulistTemp.Add(item)
                        End If
                    Next
                End If

                For i As Integer = 0 To DirectCast(Me, ScraperRegExp).RegExps.Count - 1
                    For Each item As SettingsUsed In DirectCast(Me, ScraperRegExp).RegExps(i).CollectSettings()
                        sTemp = item.ID
                        Dim tmpSetting As SettingsUsed = sulistTemp.Find(Function(n) n.ID = sTemp)

                        If IsNothing(tmpSetting) Then
                            sulistTemp.Add(item)
                        Else
                            If item.Type <> tmpSetting.Type Then
                                tmpSetting.Type = MissingSettingType.[boolean]
                            End If
                        End If
                    Next
                Next

                Dim Replacements As New Regex("\$INFO\[(.*?)\]")

                If Not String.IsNullOrEmpty(DirectCast(Me, ScraperRegExp).Conditional) Then
                    Dim msTemp As SettingsUsed = sulistTemp.Find(Function(n) n.ID = Conditional)

                    If IsNothing(msTemp) Then
                        sulistTemp.Add(New SettingsUsed(Conditional, MissingSettingType.[boolean]))
                    Else
                        msTemp.Type = MissingSettingType.[boolean]
                    End If
                End If

                If Replacements.Match(DirectCast(Me, ScraperRegExp).Input).Success Then
                    For Each mItem As Match In Replacements.Matches(DirectCast(Me, ScraperRegExp).Input)
                        sTemp = mItem.Groups(1).Value
                        Dim thisMatch As SettingsUsed = sulistTemp.Find(Function(n) n.ID = sTemp)

                        If IsNothing(thisMatch) Then
                            sulistTemp.Add(thisMatch)
                        End If
                    Next
                End If

                If Replacements.Match(DirectCast(Me, ScraperRegExp).Output).Success Then
                    For Each mItem As Match In Replacements.Matches(DirectCast(Me, ScraperRegExp).Output)
                        Dim thisMatch As New SettingsUsed(mItem.Groups(1).Value, MissingSettingType.variable)

                        If IsNothing(thisMatch) Then
                            sulistTemp.Add(New SettingsUsed(mItem.Groups(1).Value, MissingSettingType.variable))
                        End If
                    Next
                End If
                Return sulistTemp
            End Function

            Public Overloads Overrides Sub Deserialize(ByVal xRegExp As XElement)
                If Not IsNothing(xRegExp.PreviousNode) AndAlso xRegExp.PreviousNode.NodeType = System.Xml.XmlNodeType.Comment Then
                    Me.Comment = DirectCast(xRegExp.PreviousNode, XComment).Value.Trim()
                End If

                MyBase.Deserialize(xRegExp)
                If Not IsNothing(xRegExp.Attribute("conditional")) Then
                    If String.IsNullOrEmpty(xRegExp.Attribute("conditional").Value) <> True Then
                        DirectCast(Me, ScraperRegExp).ParseConditional(xRegExp.Attribute("conditional").Value)
                    End If
                End If

                If Not IsNothing(xRegExp.Attribute("input")) Then
                    If String.IsNullOrEmpty(xRegExp.Attribute("input").Value) <> True Then
                        DirectCast(Me, ScraperRegExp).Input = xRegExp.Attribute("input").Value
                    End If
                End If

                If Not IsNothing(xRegExp.Attribute("output")) Then
                    If String.IsNullOrEmpty(xRegExp.Attribute("output").Value) <> True Then
                        DirectCast(Me, ScraperRegExp).Output = xRegExp.Attribute("output").Value
                    End If
                End If

                If Not IsNothing(xRegExp.Attribute("dest")) Then
                    If String.IsNullOrEmpty(xRegExp.Attribute("dest").Value) <> True Then
                        If xRegExp.Attribute("dest").Value.EndsWith("+") Then
                            DirectCast(Me, ScraperRegExp).Dest = Convert.ToInt32(xRegExp.Attribute("dest").Value.Replace("+", ""))
                            DirectCast(Me, ScraperRegExp).Append = True
                        Else
                            DirectCast(Me, ScraperRegExp).Dest = Convert.ToInt32(xRegExp.Attribute("dest").Value)
                            DirectCast(Me, ScraperRegExp).Append = False
                        End If
                    End If
                End If

                For Each RegExp As XElement In xRegExp.Elements("RegExp")
                    DirectCast(Me, ScraperRegExp).RegExps.Add(New ScraperRegExp(RegExp, Me))
                Next

                If Not IsNothing(xRegExp.Element("expression")) Then
                    DirectCast(Me, ScraperRegExp).Expression = New ScraperExpression(xRegExp.Element("expression"), Me)
                Else
                    DirectCast(Me, ScraperRegExp).Expression = New ScraperExpression(Me)
                End If
            End Sub

            Public Overloads Overrides Function GetEvaluation(ByVal parentPath As String) As List(Of ScraperEvaluation)
                Dim RegExpErrors As New List(Of ScraperEvaluation)()
                Dim Messages As New List(Of String)()
                Dim Finder As Regex
                Dim Path As String = parentPath

                If Me.Parent.[GetType]().Name = "ScraperRegExp" Then
                    Path += ":" & DirectCast(Parent, ScraperRegExp).RegExps.IndexOf(Me).ToString()
                Else
                    Path += ":" & DirectCast(Parent, ScraperFunction).RegExps.IndexOf(Me).ToString()
                End If

                If String.IsNullOrEmpty(Input) Then
                    Dim tmpMessage As String = "Warning: Input is empty. There will be nothing to test the Regular Expression against." & vbLf & "If this is intentional (i.e. you want to have a blank value) you can safely ignore this warning"
                    RegExpErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Input, Path, tmpMessage))
                Else
                    Finder = New Regex("\$\$([0-9]+)")
                    If Finder.Match(Input).Success Then
                        For Each item As Match In Finder.Matches(Input)
                            Dim BufferReference As Integer = Convert.ToInt32(item.Groups(1).Value)

                            If BufferReference < 0 OrElse BufferReference > 20 Then
                                Dim tmpMessage As String = """" & item.Value & """ is out of range. Scrapers only support buffers ranging from 1 - 20"
                                RegExpErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Input, Path, tmpMessage))
                            End If
                        Next
                    End If
                End If

                If String.IsNullOrEmpty(Output) Then
                    Dim tmpMessage As String = "Output is empty, this RegExp will produce no results"
                    RegExpErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Output, Path, tmpMessage))
                Else
                    Finder = New Regex("\$\$([0-9]+)")
                    If Finder.Match(Output).Success Then
                        Messages.Add("Warning: Buffer reference found in Output. This is not recommended. If the buffer is empty or contains erroneous data the outcome may be unpredictable." & vbCr & vbLf & " Only use this if you ABSOLUTELY sure that this buffer will contain what you seek.")
                        For Each item As Match In Finder.Matches(Output)
                            Dim BufferReference As Integer = Convert.ToInt32(item.Groups(1).Value)

                            If BufferReference < 0 OrElse BufferReference > 20 Then
                                Dim tmpMessage As String = """" & item.Value & """ is out of range. Scrapers only support buffers ranging from 1 - 20"
                                RegExpErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Output, Path, tmpMessage))
                            End If
                        Next
                    End If

                    Finder = New Regex("<([^\/\s>]+)[^>]*>")

                    If Finder.Match(Output).Success Then
                        For Each item As Match In Finder.Matches(Output)
                            If item.Groups(1).Value <> "?xml" Then
                                If Not Output.Contains("</" & item.Groups(1).Value & ">") Then
                                    Dim tmpMessage As String = String.Format("The tag ""{0}"" is not properly closed. <""/{0}>"" is required", item.Groups(1).Value)
                                    RegExpErrors.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.Output, Path, tmpMessage))
                                End If
                            End If
                        Next
                    End If
                End If

                RegExpErrors.AddRange(Expression.GetEvaluation(Path))

                For Each item As ScraperRegExp In RegExps
                    RegExpErrors.AddRange(item.GetEvaluation(Path))
                Next

                RegExpErrors.Sort()
                Return RegExpErrors
            End Function

            Public Sub InsertRegExp(ByVal Index As Integer, ByVal scRegExp As ScraperRegExp)
                RegExps.Insert(Index, scRegExp)
            End Sub

            Public Sub RemoveRegExp(ByVal Index As Integer)
                RegExps.RemoveAt(Index)
            End Sub

            Public Function Serialize() As XElement
                Return XElement.Parse(IndentedCompleteXML)
            End Function

            Public Sub SetExpression(ByVal strExpression As String, ByVal bCaseSensitive As Boolean, ByVal bRepeat As Boolean, ByVal bClear As Boolean, ByVal strNoCleanValues As String, ByVal strTrimValues As String, _
                ByVal strEncodeValues As String, ByVal strFixampValues As String)
                Expression.CaseSensitive = bCaseSensitive
                Expression.Clear = bClear
                Expression.Repeat = bRepeat
                Expression.ParseNoCleanFromString(strNoCleanValues)
                Expression.ParseTrimFromString(strTrimValues)
                Expression.ParseEncodeFromString(strEncodeValues)
            End Sub

            Protected Friend Function GetAvailableConditionals() As Dictionary(Of String, String)
                Dim settingGrabber As New Regex("<setting.*?id=\""(.*?)\"".*?</setting>")
                Dim urlInfoGrabber As New Regex("<url.*?function=\""(.*?)\"".*?</url>")
                Dim labelGrabber As New Regex("label=\""(.*?)\""")
                Dim boolIdentifier As New Regex("type=\""bool\""")

                Dim tmp As New Dictionary(Of String, String)()

                For Each item As Match In settingGrabber.Matches(Output)
                    Dim settingString As String = item.Groups(0).Value
                    Dim settingIDString As String = item.Groups(1).Value

                    If boolIdentifier.Match(settingString).Success Then
                        If labelGrabber.Match(settingString).Success Then
                            Dim labelString As String = labelGrabber.Match(settingString).Groups(1).Value
                            tmp.Add(settingIDString, labelString)
                        End If
                    End If
                Next

                For Each item As Match In urlInfoGrabber.Matches(Output)
                    If tmp.ContainsKey("urls") Then
                        tmp("urls") = tmp("urls") & ";" & item.Groups(1).Value
                    Else
                        tmp("urls") = item.Groups(1).Value
                    End If
                Next

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
                Dim settingGrabber As New Regex("<setting.*?id=\""(.*?)\"".*?</setting>")
                Dim urlInfoGrabber As New Regex("<url.*?function=\""(.*?)\"".*?</url>")
                Dim labelGrabber As New Regex("label=\""(.*?)\""")

                Dim tmp As New Dictionary(Of String, String)()

                For Each item As Match In settingGrabber.Matches(Output)
                    Dim settingString As String = item.Groups(0).Value
                    Dim settingIDString As String = item.Groups(1).Value

                    If labelGrabber.Match(settingString).Success Then
                        Dim labelString As String = labelGrabber.Match(settingString).Groups(1).Value
                        tmp.Add(settingIDString, labelString)
                    End If
                Next

                For Each item As Match In urlInfoGrabber.Matches(Output)
                    If tmp.ContainsKey("urls") Then
                        tmp("urls") = tmp("urls") & ";" & item.Groups(1).Value
                    Else
                        tmp("urls") = item.Groups(1).Value
                    End If
                Next

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
                    If Parent.[GetType]().Name = "ScraperRegExp" Then
                        Return DirectCast(Parent, ScraperRegExp).RegExps.IndexOf(DirectCast(Me, ScraperRegExp))
                    Else
                        Return DirectCast(Parent, ScraperFunction).RegExps.IndexOf(DirectCast(Me, ScraperRegExp))
                    End If
                Else
                    Return -1
                End If
            End Function

            Protected Friend Function GetXML(ByVal optionReturn As XMLDisplayOption) As String
                Dim strReturn As String = ""

                If optionReturn = XMLDisplayOption.Complete Then
                    strReturn = String.Format("{0}<RegExp", indent)
                    GetConditionalString(strReturn)
                    GetInputString(strReturn)
                    GetOuputString(strReturn)
                    GetDestString(strReturn)
                    strReturn += ">"
                    GetNestedRegExpString(strReturn)
                    strReturn += vbCr & vbLf & Expression.IndentedXML
                    strReturn += String.Format(vbCr & vbLf & "{0}</RegExp>", indent)

                    If Not String.IsNullOrEmpty(Comment) Then
                        strReturn = strReturn.Insert(0, indent & String.Format("<!--{0}-->" & vbCr & vbLf, Comment))
                    End If
                ElseIf optionReturn = XMLDisplayOption.[Function] Then
                    strReturn = String.Format("{0}<RegExp", indent.Remove(0, 1))
                    GetConditionalString(strReturn)
                    GetInputString(strReturn)
                    GetOuputString(strReturn)
                    GetDestString(strReturn)
                    strReturn += ">"
                    For Each item As ScraperRegExp In RegExps
                        strReturn += vbCr & vbLf & item.FunctionXML
                    Next
                    strReturn += vbCr & vbLf & Expression.FunctionXML
                    strReturn += String.Format(vbCr & vbLf & "{0}</RegExp>", indent.Remove(0, 1))

                    If Not String.IsNullOrEmpty(Comment) Then
                        strReturn = strReturn.Insert(0, indent.Remove(0, 1) & String.Format("<!--{0}-->" & vbCr & vbLf, Comment))
                    End If
                Else
                    strReturn = "<RegExp"
                    GetConditionalString(strReturn)
                    GetInputString(strReturn)
                    GetOuputString(strReturn)
                    GetDestString(strReturn)

                    If optionReturn = XMLDisplayOption.StandAlone Then
                        strReturn += ">" & vbCr & vbLf & vbTab
                        strReturn += Expression.XML.Replace(vbCr & vbLf, vbCr & vbLf & vbTab)
                        strReturn += vbCr & vbLf

                    ElseIf optionReturn = XMLDisplayOption.RegExpOnly Then
                        strReturn += ">"
                    End If

                    strReturn += "</RegExp>"

                    If Not String.IsNullOrEmpty(Comment) Then
                        strReturn = strReturn.Insert(0, String.Format("<!--{0}-->" & vbCr & vbLf, Comment))
                    End If
                End If

                Return strReturn
            End Function

            Protected Friend Sub ParseConditional(ByVal strConditional As String)
                If strConditional.StartsWith("!") Then
                    Conditional = strConditional.Substring(1)
                    ConditionalInverse = True
                Else
                    Conditional = strConditional
                    ConditionalInverse = False
                End If
            End Sub

            Private Function ConditionalTostring() As String
                If ConditionalInverse Then
                    Dim sb As New StringBuilder("!")
                    sb.Append(Conditional)
                    Return sb.ToString()
                Else
                    Return Conditional
                End If
            End Function

            Private Sub GetConditionalString(ByRef strReturn As String)
                If Not String.IsNullOrEmpty(Conditional) Then
                    strReturn += " conditional="""
                    If ConditionalInverse Then
                        strReturn += "!"
                    End If

                    strReturn += Conditional & """"
                End If
            End Sub

            Private Sub GetDestString(ByRef strReturn As String)
                If Dest > -1 Then
                    Dim tmpString As String = Dest.ToString()
                    If Append Then
                        tmpString += "+"
                    End If
                    strReturn += " dest=""" & tmpString & """"
                End If
            End Sub

            Private Function GetExpression() As ScraperExpression
                Return Expression
            End Function

            Private Sub GetExpressionString(ByRef strReturn As String)
                If Not IsNothing(Expression) Then
                    strReturn += vbCr & vbLf & Expression.IndentedXML
                End If
            End Sub

            Private Sub GetInputString(ByRef strReturn As String)
                If Dest > -1 Then
                    strReturn += " input=""" & XmlUtilities.ReplaceEntities(Input) & """"
                End If
            End Sub

            Private Sub GetNestedRegExpString(ByRef strReturn As String)
                For Each RegExpItem As ScraperRegExp In RegExps
                    strReturn += vbCr & vbLf
                    strReturn += RegExpItem.IndentedCompleteXML
                Next
            End Sub

            Private Sub GetOuputString(ByRef strReturn As String)
                If Dest > -1 Then
                    strReturn += " output=""" & XmlUtilities.ReplaceEntities(Output) & """"
                End If
            End Sub

#End Region 'Methods

        End Class

    End Namespace
End Namespace
