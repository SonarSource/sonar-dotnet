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

        Public Class EditorSettingsHolder

#Region "Fields"

            Private _referencedsettings As List(Of UrlInfo)
            Private _settings As List(Of ScraperSetting)

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                Me.Clear()
            End Sub

            Public Sub New(ByVal xInfo As XElement)
                Me.Clear()
                For Each item As XElement In xInfo.Elements()
                    If item.Name.ToString() = "setting" Then
                        Settings.Add(New ScraperSetting(item))
                    ElseIf item.Name.ToString() = "url" Then
                        ReferencedSettings.Add(New UrlInfo(item))
                    End If
                Next
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public ReadOnly Property Integers() As List(Of ScraperSetting)
                Get
                    Return Settings.FindAll(Function(n) n.Type = ScraperSetting.ScraperSettingType.int)
                End Get
            End Property

            Public ReadOnly Property LabelEnumerators() As List(Of ScraperSetting)
                Get
                    Return Settings.FindAll(Function(n) n.Type = ScraperSetting.ScraperSettingType.labelenum)
                End Get
            End Property

            Public Property ReferencedSettings() As List(Of UrlInfo)
                Get
                    Return Me._referencedsettings
                End Get
                Set(ByVal value As List(Of UrlInfo))
                    Me._referencedsettings = value
                End Set
            End Property

            Public Property Settings() As List(Of ScraperSetting)
                Get
                    Return Me._settings
                End Get
                Set(ByVal value As List(Of ScraperSetting))
                    Me._settings = value
                End Set
            End Property

            Public ReadOnly Property TextSettings() As List(Of ScraperSetting)
                Get
                    Return Settings.FindAll(Function(n) n.Type = ScraperSetting.ScraperSettingType.text)
                End Get
            End Property

            Public ReadOnly Property [Boolean]() As List(Of ScraperSetting)
                Get
                    Return Settings.FindAll(Function(n) n.Type = ScraperSetting.ScraperSettingType.bool)
                End Get
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub Clear()
                Me._settings = New List(Of ScraperSetting)
                Me._referencedsettings = New List(Of UrlInfo)
            End Sub

            Friend Function GetFunction(ByVal scraper As Scraper) As ScraperFunction
                Dim GetSettings As New ScraperFunction(scraper)

                GetSettings = New ScraperFunction()
                GetSettings.Name = "GetSettings"
                GetSettings.Dest = 3
                GetSettings.AddRegExp(String.Empty, False, False, "$$5", "<settings>\1</settings>", 3)
                GetSettings.RegExps(0).SetExpression("", False, False, False, "1", String.Empty, String.Empty, String.Empty)

                For i As Integer = 0 To Me._settings.Count - 1
                    If i > 0 Then
                        GetSettings.RegExps(0).AddRegExp(String.Empty, False, True, "$$1", Me._settings(i).GetScraperStringValue(), 5)
                    Else
                        GetSettings.RegExps(0).AddRegExp(String.Empty, False, False, "$$1", Me._settings(i).GetScraperStringValue(), 5)
                    End If
                    GetSettings.RegExps(0).RegExps(i).SetExpression(String.Empty, False, False, False, String.Empty, String.Empty, String.Empty, String.Empty)
                Next

                For Each item As UrlInfo In Me._referencedsettings

                    GetSettings.RegExps(0).AddRegExp("", False, True, "$$1", item.Serialize("url").ToString(), 5)
                Next
                Return GetSettings
            End Function

#End Region 'Methods

        End Class

        Public NotInheritable Class ScraperSetting

#Region "Fields"

            Private _default As String
            Private _hidden As Boolean
            Private _id As String
            Private _invaliddata As Dictionary(Of String, String)
            Private _label As String
            Private _parameter As String
            Private _type As ScraperSettingType
            Private _values As List(Of String)

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal element As XElement)
                Me._values = New List(Of String)()
                Deserialize(element)
            End Sub

            Public Sub New()
                Me._label = "New Setting"
                Me._id = "newsettingid"
                Me._type = ScraperSettingType.bool
                Me._default = "false"
                Me._values = New List(Of String)()
                Me._parameter = "false"
                Me._hidden = False
                Me._values = New List(Of String)()
            End Sub

            Public Sub New(ByVal sstType As ScraperSettingType, ByVal strID As String, ByVal strLabel As String, ByVal strDefault As String, ByVal bHidden As Boolean, ByVal arrValues As String())
                Me._type = sstType
                Me._values = New List(Of String)()
                If Me._type <> ScraperSettingType.sep Then
                    Me._id = strID
                    Me._label = strLabel
                    If Not IsNothing(arrValues) AndAlso arrValues.Count() > 0 Then
                        Me._values.AddRange(arrValues)
                    End If

                    Me._default = strDefault

                    If Me._type = ScraperSettingType.labelenum Then
                        If Not String.IsNullOrEmpty(Me._default) Then
                            If Not Me._values.Contains(Me._default) Then
                                Me._values.Add(Me._default)
                            End If
                        End If
                    End If

                    If Me._type = ScraperSettingType.text Then
                        Me._hidden = bHidden
                    End If
                End If
            End Sub

#End Region 'Constructors

#Region "Enumerations"

            Public Enum ScraperSettingType
                bool
                int
                labelenum
                text
                sep
            End Enum

#End Region 'Enumerations

#Region "Properties"

            Public Property Hidden() As Boolean
                Get
                    Return Me._hidden
                End Get
                Set(ByVal value As Boolean)
                    Me._hidden = value
                End Set
            End Property

            Public Property ID() As String
                Get
                    Return Me._id
                End Get
                Set(ByVal value As String)
                    Me._id = value
                End Set
            End Property

            Public Property InvalidData() As Dictionary(Of String, String)
                Get
                    Return Me._invaliddata
                End Get
                Set(ByVal value As Dictionary(Of String, String))
                    Me._invaliddata = value
                End Set
            End Property

            Public ReadOnly Property IsValid() As Boolean
                Get
                    Return ValidateSetting()
                End Get
            End Property

            Public Property Label() As String
                Get
                    Return Me._label
                End Get
                Set(ByVal value As String)
                    Me._label = Value
                End Set
            End Property

            Public Property Parameter() As String
                Get
                    Return Me._parameter
                End Get
                Set(ByVal value As String)
                    Me._parameter = value
                End Set
            End Property

            Public Property Type() As ScraperSettingType
                Get
                    Return Me._type
                End Get
                Set(ByVal value As ScraperSettingType)
                    Me._type = value
                End Set
            End Property

            Public Property Values() As List(Of String)
                Get
                    Return Me._values
                End Get
                Private Set(ByVal value As List(Of String))
                    Me._values = value
                End Set
            End Property

            Public Property [Default]() As String
                Get
                    Return Me._default
                End Get
                Set(ByVal value As String)
                    Me._default = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Function Clone() As ScraperSetting
                Dim tmp As New ScraperSetting()

                tmp._default = Me._default
                tmp._hidden = Me._hidden
                tmp._id = Me._id
                tmp._label = Me._label
                tmp._parameter = Me._parameter
                tmp._type = Me._type
                tmp._values = Me._values
                Return tmp
            End Function

            Public Sub Deserialize(ByVal element As XElement)
                If Not IsNothing(element.Attribute("type")) Then
                    Me._type = DirectCast([Enum].Parse(GetType(ScraperSettingType), element.Attribute("type").Value.ToLower), ScraperSettingType)

                    If Not IsNothing(element.Attribute("label")) Then
                        Me._label = element.Attribute("label").Value
                    End If

                    If Not IsNothing(element.Attribute("id")) Then
                        Me._id = element.Attribute("id").Value
                    End If

                    If Me._type = ScraperSettingType.labelenum Then
                        Me._values = New List(Of String)()
                        Dim attribute As XAttribute = element.Attribute("values")
                        If Not IsNothing(attribute) Then
                            Dim tmp As String() = attribute.Value.Split(Convert.ToChar("|"))
                            Me._values.AddRange(tmp)
                        End If
                    End If

                    If Me._type = ScraperSettingType.bool Then
                    End If
                    If Not Me._type = ScraperSettingType.sep Then
                        If Not IsNothing(element.Attribute("default")) Then
                            Me._default = element.Attribute("default").Value
                        End If
                    Else
                        Me._default = String.Empty
                    End If

                    If Not IsNothing(element.Attribute("option")) AndAlso element.Attribute("option").Value.Contains("hidden") Then
                        Me._hidden = True
                    End If

                    If Not Me._type = ScraperSettingType.sep Then
                        If Not String.IsNullOrEmpty(element.Value) Then
                            If Me._type = ScraperSettingType.bool Then
                                Me._parameter = element.Value.ToLower
                            Else
                                Me._parameter = element.Value
                            End If
                        Else
                            Me._parameter = Me._default
                        End If
                    End If
                Else
                    Throw New Exception("The Setting Type is not specified")
                End If
            End Sub

            Public Function GetScraperStringValue() As String
                Dim strReturn As New StringBuilder

                strReturn.Append("<setting")

                If Me._type <> ScraperSettingType.sep Then
                    strReturn.Append(String.Format(" label=""{0}""", XmlUtilities.ReplaceEntities(Me._label)))
                    strReturn.Append(String.Format(" type=""{0}""", Me._type))
                    strReturn.Append(String.Format(" id=""{0}""", XmlUtilities.ReplaceEntities(Me._id)))

                    If Me._type = ScraperSettingType.labelenum Then
                        If Me._values.Count > 0 Then
                            If Not Me._values.Contains(Me._default) Then
                                Me._values.Add(Me._default)
                            End If
                        Else
                            If Not String.IsNullOrEmpty(Me._default) Then
                                Me._values.Add(Me._default)
                            End If
                        End If
                        strReturn.Append(String.Format(" values=""{0}""", XmlUtilities.ReplaceEntities(String.Join("|", Me._values.ToArray()))))
                        strReturn.Append(String.Format(" default=""{0}""", XmlUtilities.ReplaceEntities(Me._default)))
                    ElseIf Me._type = ScraperSettingType.text Then
                        If String.IsNullOrEmpty(Me._default) Then
                            strReturn.Append(" default=""""")
                        Else
                            strReturn.Append(String.Format(" default=""{0}""", XmlUtilities.ReplaceEntities(Me._default)))
                        End If

                        If Me._hidden Then
                            strReturn.Append(" option=""hidden""")
                        End If
                    ElseIf Me._type = ScraperSettingType.int Then
                        Try
                            strReturn.Append(String.Format(" default=""{0}""", Convert.ToInt32(Me._default).ToString))
                        Catch
                            strReturn.Append(" default=""1""")
                        End Try
                    Else
                        Try
                            strReturn.Append(String.Format(" default=""{0}""", Convert.ToBoolean(Me._default).ToString.ToLower))
                        Catch
                            strReturn.Append(" default=""false""")
                        End Try
                    End If
                Else
                    strReturn.Append(" type=""sep""")
                End If

                strReturn.Append("></setting>")

                Return strReturn.ToString
            End Function

            Public Sub GetValuesFromAnotherSetting(ByVal tmpSetting As ScraperSetting)
                Me._type = tmpSetting.Type
                Me._default = tmpSetting.[Default]
                Me._hidden = tmpSetting.Hidden
                Me._id = tmpSetting.ID
                Me._label = tmpSetting.Label
                Me._parameter = tmpSetting.Parameter
                Me._values = tmpSetting.Values
            End Sub

            Public Function Serialize() As XElement
                Dim tmp As New XElement("setting")

                If Not String.IsNullOrEmpty(Me._label) Then
                    tmp.Add(New XAttribute("label", Me._label))
                End If

                tmp.Add(New XAttribute("type", Me._type))

                If Not String.IsNullOrEmpty(Me._id) Then
                    tmp.Add(New XAttribute("id", Me._id))
                End If

                If Me._type = ScraperSettingType.labelenum Then

                    If Me._values.Count > 0 Then
                        If Not Me._values.Contains(Me._default) Then
                            Me._values.Add(Me._default)
                        End If

                        tmp.Add(New XAttribute("values", String.Join("|", Me._values.ToArray())))
                    Else
                        If Not String.IsNullOrEmpty(Me._default) Then
                            Me._values.Add(Me._default)
                        End If
                    End If
                End If

                If Me._type = ScraperSettingType.bool Then
                    tmp.AddBooleanAttribute("default", Me._default)
                Else
                    tmp.Add(New XAttribute("default", Me._default))
                End If

                Return tmp
            End Function

            Private Function ValidateBool() As Boolean
                Me._invaliddata = New Dictionary(Of String, String)()

                If String.IsNullOrEmpty(Me._id) Then
                    Me._invaliddata.Add("ID", "ID Cannot be empty for any type but ""sep"".")
                End If

                If String.IsNullOrEmpty(Me._label) Then
                    Me._invaliddata.Add("Label", "Label Cannot be empty for any type but ""sep"".")
                End If

                If String.IsNullOrEmpty(Me._default) Then
                    Me._invaliddata.Add("Default", "Default value cannot be empty for any type other than ""text"" or ""sep"".")
                Else
                    If Not Me._default = "true" Then
                        If Not Me._default = "false" Then
                            Me._invaliddata.Add("Default", "For boolean values Default must be either ""true"" or ""false"".")
                        End If
                    End If
                End If

                Try
                    Convert.ToBoolean(Me._parameter.ToLower())
                Catch
                    Me._invaliddata.Add("CurrentValue", "The current value does not equate to ""true"" or ""false"".")
                End Try

                If Me._invaliddata.Count > 0 Then
                    Return False
                End If
                Return True
            End Function

            Private Function ValidateInteger() As Boolean
                Me._invaliddata = New Dictionary(Of String, String)()

                If String.IsNullOrEmpty(Me._id) Then
                    Me._invaliddata.Add("ID", "ID Cannot be empty for any type but ""sep"".")
                End If

                If String.IsNullOrEmpty(Me._label) Then
                    Me._invaliddata.Add("Label", "Label Cannot be empty for any type but ""sep"".")
                End If

                If String.IsNullOrEmpty(Me._default) Then
                    Me._invaliddata.Add("Default", "Default value cannot be empty for any type other than ""text"" or ""sep"".")
                Else
                    Try
                        Convert.ToInt32(Me._default)
                    Catch
                        Me._invaliddata.Add("Default", "The default value does not equate to an integer.")
                    End Try
                End If

                Try
                    Convert.ToInt32(Me._parameter)
                Catch
                    Me._invaliddata.Add("CurrentValue", "The current value does not equate to an integer.")
                End Try

                If Me._invaliddata.Count > 0 Then
                    Return False
                End If
                Return True
            End Function

            Private Function ValidateLabelEnum() As Boolean
                Me._invaliddata = New Dictionary(Of String, String)()

                If String.IsNullOrEmpty(Me._id) Then
                    Me._invaliddata.Add("ID", "ID Cannot be empty for any type but ""sep"".")
                End If

                If String.IsNullOrEmpty(Me._label) Then
                    Me._invaliddata.Add("Label", "Label Cannot be empty for any type but ""sep"".")
                End If

                If String.IsNullOrEmpty(Me._default) Then
                    Me._invaliddata.Add("Default", "Default value cannot be empty for any type other than ""text"" or ""sep"".")
                Else
                    If Not Me._values.Contains(Me._default) Then
                        Me._invaliddata.Add("Default", "The default value is not contained in the list of acceptable values.")
                    End If
                End If

                If Me._values.Count < 1 Then
                    Me._invaliddata.Add("Values", "Values must contain at least 1 selectable value")
                End If

                If Not Me._values.Contains(Me._parameter) Then
                    Me._invaliddata.Add("CurrentValue", "The current value is not contained in the list of acceptable values.")
                End If

                If Me._invaliddata.Count > 0 Then
                    Return False
                End If
                Return True
            End Function

            Private Function ValidateSetting() As Boolean
                If Me._type = ScraperSettingType.sep Then
                    Me._invaliddata = New Dictionary(Of String, String)()
                    Return True
                ElseIf Me._type = ScraperSettingType.bool Then
                    Return ValidateBool()
                ElseIf Me._type = ScraperSettingType.labelenum Then
                    Return ValidateLabelEnum()
                ElseIf Me._type = ScraperSettingType.int Then
                    Return ValidateInteger()
                Else
                    Return ValidateText()
                End If
            End Function

            Private Function ValidateText() As Boolean
                Me._invaliddata = New Dictionary(Of String, String)()

                If String.IsNullOrEmpty(Me._id) Then
                    Me._invaliddata.Add("ID", "ID Cannot be empty for any type but ""sep"".")
                End If

                If String.IsNullOrEmpty(Me._label) Then
                    Me._invaliddata.Add("Label", "Label Cannot be empty for any type but ""sep"".")
                End If

                If String.IsNullOrEmpty(Me._default) Then
                    Me._invaliddata.Add("Warning", "Default value is Empty, acceptable for ""text"" setting but not recommended.")
                End If

                If Me._invaliddata.Count > 0 Then
                    If Me._invaliddata.Count = 1 Then
                        If Me._invaliddata.First().Key = "Warning" Then
                            Return True
                        Else
                            Return False
                        End If
                    End If
                    Return False
                End If
                Return True
            End Function

#End Region 'Methods

        End Class

        Public NotInheritable Class ScraperSettings
            Inherits List(Of ScraperSetting)

#Region "Constructors"

            Public Sub New(ByVal element As XElement)
                Deserialize(element)
            End Sub

            Public Sub New()
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public ReadOnly Property Conditionals() As ScraperSettings
                Get
                    Dim findSettings As IEnumerable(Of ScraperSetting) = Me.FindAll(Function(n) n.Type = ScraperSetting.ScraperSettingType.bool)

                    Dim tmpSettings As New ScraperSettings
                    For Each ssItem As ScraperSetting In findSettings
                        tmpSettings.Add(ssItem)
                    Next
                    Return tmpSettings
                End Get
            End Property

#End Region 'Properties

#Region "Methods"

            Public Overloads Function Contains(ByVal settingID As String) As Boolean
                If Me.Where(Function(s) s.ID = settingID).Count > 0 Then Return True
                Return False
            End Function

            Public Overloads Function Contains(ByVal msItem As SettingsUsed) As Boolean
                If Me.Where(Function(s) s.ID = msItem.ID).Count > 0 Then Return True
                Return False
            End Function

            Public Sub Deserialize(ByVal element As XElement)
                For Each settingItem As XElement In element.Elements("setting")
                    Dim newSetting As New ScraperSetting(settingItem)
                    If Not Contains(newSetting.ID) Then
                        Add(New ScraperSetting(settingItem))
                    End If
                Next
            End Sub

            Public Function Serialize(ByVal elementName As String) As XElement
                Dim tmp As New XElement("settings")
                For Each settingItem As ScraperSetting In ToArray()
                    tmp.Add(New XElement(settingItem.Serialize()))
                Next

                Return tmp
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
