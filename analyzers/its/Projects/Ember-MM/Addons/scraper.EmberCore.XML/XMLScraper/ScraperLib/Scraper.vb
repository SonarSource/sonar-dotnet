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
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

Imports Microsoft.VisualBasic

Imports EmberScraperModule.XMLScraper.MediaTags
Imports EmberScraperModule.XMLScraper.ScraperXML
Imports EmberScraperModule.XMLScraper.Utilities
Imports EmberAPI

Namespace XMLScraper
    Namespace ScraperLib

        Public Class Scraper
            Inherits ScraperFunctionContainer

#Region "Fields"

            Public pContent As ScraperContent

            Private m_CacheHours As Integer
            Private m_CacheMinutes As Integer
            Private m_CachePersistence As Boolean
            Private m_GetSettingsFunction As ScraperFunction
            Private m_ScraperDate As DateTime
            Private m_ScraperEncoding As String
            Private m_ScraperFramework As Single
            Private m_ScraperIncludes As List(Of String)
            Private m_ScraperLanguage As String
            Private m_ScraperThumb As String
            Private m_ThumbLocation As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                ScraperEncoding = "utf-8"
                Name = "New Scraper"
                pContent = ScraperContent.movies
                ScraperLanguage = "en"
                ScraperFramework = 1.1
                ScraperDate = DateTime.Now
                ScraperIncludes = New List(Of String)()

                CreateGetSettingsFunction()
            End Sub

            Public Sub New(ByVal xmlFilePath As String)
                Me.New()
                Load(xmlFilePath)
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public ReadOnly Property AvailableCondiionals() As Dictionary(Of String, String)
                Get
                    Dim tmp As New Dictionary(Of String, String)()

                    For Each kvItem As KeyValuePair(Of String, String) In GetSettingsFunction.GetAvailableConditionals()
                        If kvItem.Key = "urls" Then
                            Dim tmpFunctionNames As String() = kvItem.Value.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)

                            For Each functionname As String In tmpFunctionNames
                                Dim tmpFunction As ScraperFunction = GetFunction(functionname)
                                If Not IsNothing(tmpFunction) Then
                                    For Each cfkvItem As KeyValuePair(Of String, String) In tmpFunction.GetAvailableConditionals()
                                        If Not tmp.ContainsKey(cfkvItem.Key) Then
                                            tmp.Add(cfkvItem.Key, cfkvItem.Value)
                                        End If
                                    Next
                                End If
                            Next
                        ElseIf Not tmp.ContainsKey(kvItem.Key) Then
                            tmp.Add(kvItem.Key, kvItem.Value)

                        End If
                    Next
                    Return tmp
                End Get
            End Property

            Public ReadOnly Property AvailableSettings() As Dictionary(Of String, String)
                Get
                    Dim tmp As New Dictionary(Of String, String)()

                    For Each kvItem As KeyValuePair(Of String, String) In GetSettingsFunction.GetAvailableSettings()
                        If kvItem.Key = "urls" Then
                            Dim tmpFunctionNames As String() = kvItem.Value.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)

                            For Each functionname As String In tmpFunctionNames
                                Dim tmpFunction As ScraperFunction = GetFunction(functionname)
                                If Not IsNothing(tmpFunction) Then
                                    For Each cfkvItem As KeyValuePair(Of String, String) In tmpFunction.GetAvailableSettings()
                                        If Not tmp.ContainsKey(cfkvItem.Key) Then
                                            tmp.Add(cfkvItem.Key, cfkvItem.Value)
                                        End If
                                    Next
                                End If
                            Next
                        ElseIf Not tmp.ContainsKey(kvItem.Key) Then
                            tmp.Add(kvItem.Key, kvItem.Value)
                        End If
                    Next
                    Return tmp
                End Get
            End Property

            Public Property CacheHours() As Integer
                Get
                    Return m_CacheHours
                End Get
                Set(ByVal value As Integer)
                    m_CacheHours = value
                End Set
            End Property

            Public Property CacheMinutes() As Integer
                Get
                    Return m_CacheMinutes
                End Get
                Set(ByVal value As Integer)
                    m_CacheMinutes = value
                End Set
            End Property

            Public Property CachePersistence() As Boolean
                Get
                    Return m_CachePersistence
                End Get
                Set(ByVal value As Boolean)
                    m_CachePersistence = value
                End Set
            End Property

            Public Property Content() As ScraperContent
                Get
                    Return pContent
                End Get
                Set(ByVal value As ScraperContent)
                    pContent = value
                    ScraperIncludes.Clear()
                End Set
            End Property

            Public Property GetSettingsFunction() As ScraperFunction
                Get
                    Return m_GetSettingsFunction
                End Get
                Set(ByVal value As ScraperFunction)
                    m_GetSettingsFunction = value
                End Set
            End Property

            Public ReadOnly Property MissingSettings() As List(Of SettingsUsed)
                Get
                    Return GetNeededSettings()
                End Get
            End Property

            Public Property ScraperDate() As DateTime
                Get
                    Return m_ScraperDate
                End Get
                Set(ByVal value As DateTime)
                    m_ScraperDate = value
                End Set
            End Property

            Public Property ScraperEncoding() As String
                Get
                    Return m_ScraperEncoding
                End Get
                Set(ByVal value As String)
                    m_ScraperEncoding = value
                End Set
            End Property

            Public Property ScraperFramework() As Single
                Get
                    Return m_ScraperFramework
                End Get
                Set(ByVal value As Single)
                    m_ScraperFramework = value
                End Set
            End Property

            Public Property ScraperIncludes() As List(Of String)
                Get
                    Return m_ScraperIncludes
                End Get
                Set(ByVal value As List(Of String))
                    m_ScraperIncludes = value
                End Set
            End Property

            Public Property ScraperLanguage() As String
                Get
                    Return m_ScraperLanguage
                End Get
                Set(ByVal value As String)
                    m_ScraperLanguage = value
                End Set
            End Property

            Public Property ScraperThumb() As String
                Get
                    Return m_ScraperThumb
                End Get
                Set(ByVal value As String)
                    m_ScraperThumb = value
                End Set
            End Property

            Public ReadOnly Property Settings() As ScraperSettings
                Get
                    Return GetSettings()
                End Get
            End Property

            Public Property ThumbLocation() As String
                Get
                    Return m_ThumbLocation
                End Get
                Set(ByVal value As String)
                    m_ThumbLocation = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Function CheckSettingsExist(ByVal suList As List(Of SettingsUsed)) As List(Of SettingsUsed)
                Dim tmp As New List(Of SettingsUsed)()
                For Each item As SettingsUsed In suList
                    If Not Settings.Contains(item) Then
                        tmp.Add(item)
                    End If
                Next
                Return tmp
            End Function

            Public Shadows Sub Deserialize(ByVal element As XElement)
                ScraperFunctions.Clear()

                If Not IsNothing(element.Attribute("framework")) Then
                    If String.IsNullOrEmpty(element.Attribute("framework").Value) <> True Then
                        ScraperFramework = NumUtils.ConvertToSingle(element.Attribute("framework").Value)
                    End If
                End If

                If Not IsNothing(element.Attribute("date")) Then
                    If String.IsNullOrEmpty(element.Attribute("date").Value) <> True Then
                        Dim DateRegEx As New System.Text.RegularExpressions.Regex("([0-9]+)-([0-9]+)-([0-9]+)")
                        If DateRegEx.Match(element.Attribute("date").Value).Success Then
                            Dim strDate As String = DateRegEx.Match(element.Attribute("date").Value).Groups(1).Value & "/" & DateRegEx.Match(element.Attribute("date").Value).Groups(2).Value & "/" & DateRegEx.Match(element.Attribute("date").Value).Groups(3).Value
                            ScraperDate = DateTime.Parse(strDate)
                        End If
                    End If
                Else
                    ScraperDate = DateTime.Now
                End If

                If Not IsNothing(element.Attribute("name")) Then
                    If String.IsNullOrEmpty(element.Attribute("name").Value) <> True Then
                        Name = element.Attribute("name").Value
                    End If
                Else
                    Name = ""
                End If

                If Not IsNothing(element.Attribute("thumb")) Then
                    If String.IsNullOrEmpty(element.Attribute("thumb").Value) <> True Then
                        ScraperThumb = element.Attribute("thumb").Value
                    End If
                Else
                    ScraperThumb = ""
                End If

                If Not IsNothing(element.Attribute("content")) Then
                    If String.IsNullOrEmpty(element.Attribute("content").Value) <> True Then
                        Content = CType([Enum].Parse(GetType(ScraperContent), element.Attribute("content").Value), ScraperContent)
                    End If
                Else
                    Content = ScraperContent.movies
                End If

                If Not IsNothing(element.Attribute("language")) Then
                    If String.IsNullOrEmpty(element.Attribute("language").Value) <> True Then
                        ScraperLanguage = element.Attribute("language").Value
                    End If
                End If

                If Not IsNothing(element.Attribute("cachePersistence")) Then
                    Dim tmp As String() = element.Attribute("cachePersistence").Value.Split(":".ToCharArray(), 2)

                    CachePersistence = True

                    Try
                        CacheHours = Convert.ToInt32(tmp(0))
                        CacheMinutes = Convert.ToInt32(tmp(1))
                    Catch
                        CachePersistence = False
                        CacheHours = 0
                        CacheMinutes = 0

                    End Try
                End If

                For Each item As XElement In element.Elements()
                    If item.Name.ToString() = "include" Then
                        ScraperIncludes.Add(item.Value)
                    Else

                        Dim newFunction As New ScraperFunction(item, Me)

                        If newFunction.Name = "GetSettings" Then
                            For Each reItem As ScraperRegExp In newFunction.RegExps(0).RegExps
                                GetSettingsFunction = newFunction
                            Next
                        Else
                            ScraperFunctions.Add(newFunction)
                        End If
                    End If
                Next

                If IsNothing(GetSettingsFunction) Then
                    CreateGetSettingsFunction()
                End If
            End Sub

            Public Overloads Overrides Function Evaluate() As List(Of ScraperEvaluation)
                Dim tmp As List(Of ScraperEvaluation) = MyBase.Evaluate()

                Select Case pContent
                    Case ScraperContent.tvshows
                        If Not HasFunction("CreateSearchUrl") Then
                            Dim tmpMessage As String = "CreateSearchUrl Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        If Not HasFunction("GetSearchResults") Then
                            Dim tmpMessage As String = "GetSearchResults Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        If Not HasFunction("GetDetails") Then
                            Dim tmpMessage As String = "GetDetails Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        If Not HasFunction("GetEpisodeList") Then
                            Dim tmpMessage As String = "GetEpisodeList Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        If Not HasFunction("GetEpisodeDetails") Then
                            Dim tmpMessage As String = "GetEpisodeDetails Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        Exit Select
                    Case ScraperContent.albums
                        If Not HasFunction("CreateAlbumSearchUrl") Then
                            Dim tmpMessage As String = "CreateAlbumSearchUrl Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        If Not HasFunction("GetAlbumSearchResults") Then
                            Dim tmpMessage As String = "GetAlbumSearchResults Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        If Not HasFunction("GetAlbumDetails") Then
                            Dim tmpMessage As String = "GetAlbumDetails Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        If HasFunction("CreateArtistSearchUrl") OrElse HasFunction("GetArtistSearchResults") OrElse HasFunction("GetArtistDetails") Then
                            If Not HasFunction("CreateArtistSearchUrl") Then
                                Dim tmpMessage As String = "CreateArtistSearchUrl Function does not exist: This can be safely ignored if you do not wish to add Artist Search options"
                                tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                            End If
                            If Not HasFunction("GetArtistSearchResults") Then
                                Dim tmpMessage As String = "GetArtistSearchResults Function does not exist: This can be safely ignored if you do not wish to add Artist Search options"
                                tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                            End If
                            If Not HasFunction("GetArtistDetails") Then
                                Dim tmpMessage As String = "GetArtistDetails Function does not exist: This can be safely ignored if you do not wish to add Artist Search options"
                                tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                            End If
                        End If
                        Exit Select
                    Case Else
                        If Not HasFunction("CreateSearchUrl") Then
                            Dim tmpMessage As String = "CreateSearchUrl Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        If Not HasFunction("GetSearchResults") Then
                            Dim tmpMessage As String = "GetSearchResults Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        If Not HasFunction("GetDetails") Then
                            Dim tmpMessage As String = "GetDetails Function does not exist"
                            tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.StandardFunctionMissing, "Scraper", tmpMessage))
                        End If
                        Exit Select
                End Select

                If Settings.Count > 0 Then
                    Dim duplicates As New List(Of String)()
                    For Each item As ScraperSetting In Settings
                        If Not duplicates.Contains(item.ID) Then
                            Dim ID As String = item.ID

                            If Settings.FindAll(Function(n) n.ID = ID).Count > 1 Then
                                duplicates.Add(Convert.ToString(item) & ":" & Settings.IndexOf(item).ToString())
                            End If
                        End If
                    Next

                    For Each item As String In duplicates
                        Dim tmpStrings As String() = item.Split(":".ToCharArray())
                        Dim tmpMessage As String = String.Format("The setting id ""{0}"" appears multiple times", tmpStrings(0))
                        tmp.Add(New ScraperEvaluation(ScraperEvaluation.NodeErrorType.DuplicateSetting, "Scraper:Settings:" & tmpStrings(1), tmpMessage))
                    Next
                End If

                tmp.Sort()
                Return tmp
            End Function

            Public Function GetFunctionIndex(ByVal functionName As String) As Integer
                Try
                    Return ScraperFunctions.FindIndex(Function(n) n.Name = functionName)
                Catch
                    Return -1
                End Try
            End Function

            Public Function GetMissingSettings() As List(Of SettingsUsed)
                Dim tmp As New List(Of SettingsUsed)()
                tmp.AddRange(MyBase.GetNeededSettings())

                For i As Integer = 0 To Settings.Count() - 1
                    Dim id As String = Settings(i).ID

                    Dim tmpCount As Integer = 0
                    While tmpCount < tmp.Count()
                        If tmp(tmpCount).ID = id Then
                            tmp.RemoveAt(tmpCount)
                        Else
                            tmpCount += 1
                        End If

                    End While
                Next

                Return New List(Of SettingsUsed)(tmp)
            End Function

            Public Function GetSettings() As ScraperSettings
                Dim tmp As New Tester()
                Return tmp.GetFullSettings(Me)
            End Function

            Public Sub Load(ByVal XmlPath As String)
                Dim ScraperXML As XDocument = XDocument.Load(XmlPath)
                Deserialize(ScraperXML.Root)

                If Not String.IsNullOrEmpty(ScraperThumb) Then

                    ThumbLocation = XmlPath.Substring(0, XmlPath.LastIndexOf(Path.DirectorySeparatorChar) + 1) & ScraperThumb
                Else
                    ThumbLocation = Nothing
                End If

                If Not IsNothing(ScraperXML.Document.Declaration) Then
                    If Not String.IsNullOrEmpty(ScraperXML.Document.Declaration.Encoding) Then
                        ScraperEncoding = ScraperXML.Document.Declaration.Encoding
                    Else
                        ScraperEncoding = "utf-8"
                    End If
                Else
                    ScraperEncoding = "utf-8"
                End If
            End Sub

            Public Sub Save(ByVal fileName As String)
                Me.ScraperDate = DateTime.Now

                If Not IsNothing(ThumbLocation) Then
                    Dim ScraperImage As New FileInfo(ThumbLocation)
                    Dim ThumbPath As String = String.Empty
                    If Not String.IsNullOrEmpty(ScraperThumb) Then
                        If ScraperImage.Exists Then
                            ThumbPath = fileName.Substring(0, fileName.LastIndexOf(Path.DirectorySeparatorChar) + 1) & ScraperThumb
                        End If
                    Else
                        Dim tmpString As String = ThumbLocation.Substring(ThumbLocation.LastIndexOf(Path.DirectorySeparatorChar) + 1)
                        ScraperThumb = tmpString
                        ThumbPath = fileName.Substring(0, fileName.LastIndexOf(Path.DirectorySeparatorChar) + 1) & tmpString
                    End If

                    If Not String.IsNullOrEmpty(ThumbPath) Then
                        If ThumbPath <> ThumbLocation Then
                            ScraperImage.CopyTo(ThumbPath, True)
                        End If
                    End If
                End If

                Dim ScraperXML As XElement = XElement.Parse(XML, LoadOptions.PreserveWhitespace)
                Dim xDec As New XDeclaration("1.0", Me.ScraperEncoding, "")
                Dim Document As New XDocument()
                Document.Declaration = xDec
                Document.Add(ScraperXML)
                Document.Save(fileName, SaveOptions.DisableFormatting)
            End Sub

            Public Shadows Function Serialize() As XDocument
                Return XDocument.Parse(XML, LoadOptions.PreserveWhitespace)
            End Function

            Protected Friend Overloads Overrides Function CreateXML() As String
                Dim strReturn As String = String.Format("<?xml version=""1.0"" encoding=""{0}""?>" & vbCr & vbLf & "<scraper", ScraperEncoding)

                strReturn += String.Format(" framework=""{0}""", ScraperFramework.ToString())

                strReturn += String.Format(" date=""{0}""", ScraperDate.ToString("yyyy-MM-dd"))

                strReturn += String.Format(" name=""{0}""", Name)

                strReturn += String.Format(" content=""{0}""", Content)

                If Not String.IsNullOrEmpty(ScraperThumb) Then
                    strReturn += String.Format(" thumb=""{0}""", ScraperThumb)
                End If

                If Not String.IsNullOrEmpty(ScraperLanguage) Then
                    strReturn += String.Format(" language=""{0}""", ScraperLanguage)
                End If

                If CachePersistence AndAlso (CacheHours > 0 OrElse CacheMinutes > 0) Then
                    strReturn += " cachePersistence="""

                    If CacheHours < 10 Then
                        strReturn += CacheHours.ToString().Insert(0, "0") & ":"

                    Else
                        strReturn += CacheHours.ToString() & ":"

                    End If

                    If CacheMinutes < 10 Then
                        strReturn += CacheMinutes.ToString().Insert(0, "0")
                    Else
                        strReturn += CacheMinutes.ToString()
                    End If

                    strReturn += """"
                End If

                strReturn += ">"

                For Each item As String In ScraperIncludes
                    strReturn += vbCr & vbLf & vbTab
                    strReturn += "<include>" & item & "</include>"
                Next

                If Not IsNothing(GetSettingsFunction) AndAlso GetSettingsFunction.RegExps(0).RegExps.Count > 0 Then
                    strReturn += GetSettingsFunction.XML
                End If

                strReturn += MyBase.CreateXML()
                strReturn += vbCr & vbLf & "</scraper>"
                strReturn += vbCr & vbLf
                Return strReturn
            End Function

            Private Sub CreateGetSettingsFunction()
                GetSettingsFunction = New ScraperFunction(Me)
                GetSettingsFunction.Dest = 3
                GetSettingsFunction.Name = "GetSettings"
                GetSettingsFunction.AddRegExp("", False, False, "$$5", "<settings>\1</settings>", 3)
            End Sub

            Private Function GetFunction(ByVal FunctionName As String) As ScraperFunction
                Return ScraperFunctions.Find(Function(n) n.Name = FunctionName)
            End Function

            Private Function GetFunction(ByVal Index As Integer) As ScraperFunction
                Try
                    Return ScraperFunctions(Index)
                Catch
                    Return Nothing
                End Try
            End Function

            Private Sub ModifyGetSettingsFunction()
            End Sub

#End Region 'Methods

        End Class

        Public NotInheritable Class SettingsUsed

#Region "Fields"

            Private m_ID As String
            Private m_ImplementingFunctions As List(Of String)
            Private m_Type As MissingSettingType

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal strID As String, ByVal mstType As MissingSettingType)
                ID = strID
                Type = mstType
                ImplementingFunctions = New List(Of String)()
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property ID() As String
                Get
                    Return m_ID
                End Get
                Friend Set(ByVal value As String)
                    m_ID = value
                End Set
            End Property

            Public Property ImplementingFunctions() As List(Of String)
                Get
                    Return m_ImplementingFunctions
                End Get
                Friend Set(ByVal value As List(Of String))
                    m_ImplementingFunctions = value
                End Set
            End Property

            Public Property Type() As MissingSettingType
                Get
                    Return m_Type
                End Get
                Friend Set(ByVal value As MissingSettingType)
                    m_Type = value
                End Set
            End Property

#End Region 'Properties

        End Class

    End Namespace
End Namespace
