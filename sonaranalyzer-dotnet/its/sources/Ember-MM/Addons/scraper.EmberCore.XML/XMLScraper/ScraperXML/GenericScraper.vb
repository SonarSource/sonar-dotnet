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
Imports System.Xml.Linq

Imports EmberScraperModule.XMLScraper.MediaTags
Imports EmberScraperModule.XMLScraper.ScraperLib
Imports EmberScraperModule.XMLScraper.Utilities

Namespace XMLScraper
    Namespace ScraperXML

#Region "Enumerations"

        Public Enum ErrorType
            FunctionError
            WebError
            XmlParseError
            ContentError
            GeneralError
        End Enum

#End Region 'Enumerations

        Public Class ScraperLookup

#Region "Fields"

            Private _contenttype As String
            Private _createsearchurlfunctionname As String
            Private _getdetailsfunctionname As String
            Private _getresultsfunctionname As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal contentType__1 As String, ByVal createSearchUrl As String, ByVal getResults As String, ByVal getDetails As String)
                Me.Clear()
                Me._contenttype = contentType__1
                Me._createsearchurlfunctionname = createSearchUrl
                Me._getresultsfunctionname = getResults
                Me._getdetailsfunctionname = getDetails
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property ContentType() As String
                Get
                    Return Me._contenttype
                End Get
                Private Set(ByVal value As String)
                    Me._contenttype = value
                End Set
            End Property

            Public Property CreateSearchUrlFunctionName() As String
                Get
                    Return Me._createsearchurlfunctionname
                End Get
                Private Set(ByVal value As String)
                    Me._createsearchurlfunctionname = value
                End Set
            End Property

            Public Property GetDetailsFunctionName() As String
                Get
                    Return Me._getdetailsfunctionname
                End Get
                Private Set(ByVal value As String)
                    Me._getdetailsfunctionname = value
                End Set
            End Property

            Public Property GetResultsFunctionName() As String
                Get
                    Return Me._getresultsfunctionname
                End Get
                Private Set(ByVal value As String)
                    Me._getresultsfunctionname = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub Clear()
                Me._contenttype = String.Empty
                Me._createsearchurlfunctionname = String.Empty
                Me._getresultsfunctionname = String.Empty
                Me._getdetailsfunctionname = String.Empty
            End Sub

#End Region 'Methods

        End Class

        Friend Class GenericScraper

#Region "Fields"

            Protected ScraperInfoItem As ScraperInfo
            Protected strHTML As List(Of String)
            Protected WebpageDownloads As List(Of UrlInfo)

            Private _parser As ScraperParser
            Private _primary As String
            Private _scraperLookups As New Dictionary(Of MediaType, ScraperLookup)()
            Private _secondary As String
            Private _settings As ScraperSettings
            Private _tag As MediaTag

#End Region 'Fields

#Region "Constructors"

            Friend Sub New()
                Me.Clear()

                Me._scraperLookups.Add(MediaType.album, New ScraperLookup("albums", "CreateAlbumSearchUrl", "GetAlbumSearchResults", "GetAlbumDetails"))
                Me._scraperLookups.Add(MediaType.artist, New ScraperLookup("albums", "CreateArtistSearchUrl", "GetArtistSearchResults", "GetArtistDetails"))
                Me._scraperLookups.Add(MediaType.movie, New ScraperLookup("movies", "CreateSearchUrl", "GetSearchResults", "GetDetails"))
                Me._scraperLookups.Add(MediaType.musicvideo, New ScraperLookup("musicvideos", "CreateSearchUrl", "GetSearchResults", "GetDetails"))
                Me._scraperLookups.Add(MediaType.tvshow, New ScraperLookup("tvshows", "CreateSearchUrl", "GetSearchResults", "GetDetails"))
                Me._scraperLookups.Add(MediaType.tvepisode, New ScraperLookup("tvshows", Nothing, Nothing, "GetEpisodeDetails"))
            End Sub

#End Region 'Constructors

#Region "Properties"

            Friend Property Parser() As ScraperParser
                Get
                    Return Me._parser
                End Get
                Private Set(ByVal value As ScraperParser)
                    Me._parser = value
                End Set
            End Property

            Friend Property Primary() As String
                Get
                    Return Me._primary
                End Get
                Set(ByVal value As String)
                    Me._primary = value
                End Set
            End Property

            Friend Property Secondary() As String
                Get
                    Return Me._secondary
                End Get
                Set(ByVal value As String)
                    Me._secondary = value
                End Set
            End Property

            Friend Property Settings() As ScraperSettings
                Get
                    Return Me._settings
                End Get
                Private Set(ByVal value As ScraperSettings)
                    Me._settings = value
                End Set
            End Property

            Friend Property Tag() As MediaTag
                Get
                    Return Me._tag
                End Get
                Private Set(ByVal value As MediaTag)
                    Me._tag = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Protected Friend Sub CheckLink(ByVal scrapeResults As List(Of ScrapeResultsEntity))
                If scrapeResults.Count = 1 Then
                    If scrapeResults(0).Urls.Count > 1 Then
                        If WebpageDownloads.Count = 1 Then
                            scrapeResults(0).Urls.Clear()
                            scrapeResults(0).Urls.AddRange(WebpageDownloads)
                        End If
                    End If
                End If
            End Sub

            Friend Sub Clear()
                Me._primary = String.Empty
                Me._secondary = String.Empty
                Me._tag = Nothing
                Me._parser = New ScraperParser()
                WebpageDownloads = New List(Of UrlInfo)()
                strHTML = New List(Of String)()
            End Sub

            Friend Sub ErrorMessage(ByVal Err As ErrorType, ByVal strErrorMessage As String)
                Dim Message As String = String.Empty

                Select Case Err
                    Case ErrorType.FunctionError
                        Message = "Cannot continue to process " & strErrorMessage
                    Case ErrorType.WebError
                        Message = "Could not download webpage: " & strErrorMessage
                    Case ErrorType.XmlParseError
                        Message = "Could not parse to XML: " & strErrorMessage
                    Case ErrorType.ContentError
                        Message = "Content MisMatch: Scraper Content is """ & Parser.Content & """Expected Content is """ & strErrorMessage & """."
                    Case Else
                        Message = strErrorMessage
                End Select
            End Sub

            Protected Friend Function HttpGet() As Boolean
                If WebpageDownloads.Count > 0 Then
                    strHTML.Clear()
                    Dim strCurrHTML As String

                    For i As Integer = 0 To WebpageDownloads.Count - 1
                        Try
                            strCurrHTML = HttpRetrieve.GetPage(WebpageDownloads(i), ScraperInfoItem.ScraperCache)
                        Catch ex As Exception
                            ErrorMessage(ErrorType.WebError, WebpageDownloads(i).Serialize("url").ToString() & " (Error:" & ex.Message & ")")
                            Return False
                        End Try
                        strHTML.Add(strCurrHTML)
                    Next
                End If

                If strHTML.Count > 0 Then
                    For i As Integer = 0 To strHTML.Count - 1
                        Me._parser.setBuffer(i + 1, strHTML(i))
                    Next
                End If

                Return True
            End Function

            Protected Friend Function HttpGet(ByVal WebpageDownloadsIndex As Integer) As Boolean
                Try
                    If Not IsNothing(ScraperInfoItem) Then
                        Me._parser.setBuffer(1, HttpRetrieve.GetPage(WebpageDownloads(WebpageDownloadsIndex), ScraperInfoItem.ScraperCache))
                    Else
                        Me._parser.setBuffer(1, HttpRetrieve.GetPage(WebpageDownloads(WebpageDownloadsIndex), ""))
                    End If
                Catch generatedExceptionName As Exception
                    ErrorMessage(ErrorType.WebError, WebpageDownloads(WebpageDownloadsIndex).Serialize("url").ToString())
                    Return False
                End Try
                Return True
            End Function

            Protected Friend Function InternalCreateSearchUrl(ByVal searchType As MediaType) As XElement
                WebpageDownloads.Clear()
                strHTML.Clear()

                If Not VerifyProperContent(Me._parser, searchType) Then
                    Return Nothing
                End If

                Me._parser.setBuffer(1, Web.HttpUtility.UrlEncode(Me._primary, System.Text.Encoding.GetEncoding("ISO-8859-1")))

                If Not String.IsNullOrEmpty(Me._secondary) Then
                    Me._parser.setBuffer(2, UrlInfo.UrlEncode(Me._secondary))
                End If

                Dim functionName As String = Me._scraperLookups(searchType).CreateSearchUrlFunctionName

                Dim stringResults As String = Me._parser.ParseFunction(functionName)
                Dim results As XElement = ParseStringUrl(stringResults)
                If IsNothing(results) Then
                    ErrorMessage(ErrorType.FunctionError, functionName)
                    Return Nothing
                End If

                If results.HasElements Then
                    If Not PrepareHttp(results.Elements("url")) Then
                        Return Nothing
                    End If
                Else
                    WebpageDownloads.Add(New UrlInfo(results))
                    If Not HttpGet() Then
                        Return Nothing
                    End If
                End If
                Return InternalGetSearchResults(searchType)
            End Function

            Protected Friend Function InternalGetDetails(ByVal entity As ScrapeResultsEntity) As Boolean
                If Not VerifyProperContent(Me._parser, entity.TypeOfSearch) Then
                    Return False
                End If

                If Not PrepareHttp(entity.Urls) Then
                    Return False
                End If

                Me._tag = MediaTag.TagFromResultsType(entity.TypeOfSearch)

                Select Case entity.TypeOfSearch
                    Case MediaType.album

                    Case MediaType.artist
                        If Not String.IsNullOrEmpty(entity.Title) Then
                            Me._parser.setBuffer(strHTML.Count + 1, UrlInfo.UrlEncode(entity.Title))
                        End If
                    Case Else
                        If Not String.IsNullOrEmpty(entity.ID) Then
                            Me._parser.setBuffer(strHTML.Count + 1, entity.ID)
                        End If

                        If WebpageDownloads.Count > 0 Then
                            Me._parser.setBuffer(strHTML.Count + 2, WebpageDownloads(0).Url)
                        End If
                End Select

                Dim functionName As String = Me._scraperLookups(entity.TypeOfSearch).GetDetailsFunctionName

                Dim stringResults As String = Me._parser.ParseFunction(functionName)
                Dim results As XElement = ParseStringXML(stringResults)
                If IsNothing(results) Then
                    ErrorMessage(ErrorType.FunctionError, functionName)
                    Return False
                End If

                Me._tag.Deserialize(results)

                ParseCustomFunctions(results)

                If entity.TypeOfSearch = MediaType.tvshow Then
                    DirectCast(Me._tag, TvShowTag).ScraperName = Me._parser.Name
                End If

                Return True
            End Function

            Protected Friend Function InternalGetEpisodeList(ByVal GuideList As List(Of UrlInfo), ByVal listType As MediaType) As XElement
                WebpageDownloads.Clear()
                WebpageDownloads = GuideList
                Dim tempList As XElement
                Dim elementName As String
                Dim functionName As String

                If listType = MediaType.tvepisode Then
                    tempList = New XElement("episodelist")
                    functionName = "GetEpisodeList"
                    elementName = "episode"
                Else
                    tempList = New XElement("issuelist")
                    functionName = "GetIssueList"
                    elementName = "issue"
                End If

                For i As Integer = 0 To WebpageDownloads.Count - 1
                    If HttpGet(i) Then
                        Me._parser.setBuffer(2, WebpageDownloads(i).Url)

                        Dim stringResults As String = Me._parser.ParseFunction(functionName)
                        Dim results As XElement = ParseStringXML(stringResults)
                        If Not IsNothing(results) Then
                            tempList.Add(results.Elements(elementName))
                        End If
                    End If
                Next
                Return tempList
            End Function

            Protected Friend Function InternalGetSearchResults(ByVal resultsType As MediaType) As XElement
                If WebpageDownloads.Count > 0 Then
                    Me._parser.setBuffer(strHTML.Count + 1, WebpageDownloads(0).Url)
                End If

                Dim functionName As String = Me._scraperLookups(resultsType).GetResultsFunctionName
                Dim stringResults As String = Parser.ParseFunction(functionName)
                Dim results As XElement = ParseStringXML(stringResults)
                If IsNothing(results) Then
                    ErrorMessage(ErrorType.FunctionError, functionName)
                    Return Nothing
                End If

                If results.Elements("entity").Count() = 1 AndAlso WebpageDownloads.Count = 1 Then
                    If IsNothing(results.Element("entity").Element("url")) OrElse String.IsNullOrEmpty(results.Element("entity").Value) Then
                        results.Element("entity").Add(WebpageDownloads(0).Serialize("url"))
                    End If
                End If

                Return results
            End Function

            Friend Function InternalGetSettings() As ScraperSettings
                Me._settings = New ScraperSettings()

                If Not Parser.HasFunction("GetSettings") Then
                    Return Me._settings
                End If

                Dim stringResults As String = Me._parser.ParseFunction("GetSettings")
                Dim results As XElement = ParseStringXML(stringResults)
                If IsNothing(results) Then
                    'ErrorMessage(ErrorType.GeneralError, "Settings Cannot be loaded for " & Parser.XmlScraper.Root.Element("name").Value)
                    Return Nothing
                End If

                For Each item As XElement In results.Elements("setting")
                    Me._settings.Add(New ScraperSetting(item))
                Next

                ParseCustomSettings(results)

                Me._parser.Settings = Me._settings

                Return Me._settings
            End Function

            Friend Function InternalNfoUrl(ByVal strNfo As String, ByVal nfoType As MediaType) As ScrapeResultsEntity
                Dim entity As ScrapeResultsEntity = Nothing
                Me._parser.setBuffer(1, strNfo)
                Dim stringResults As String = Parser.ParseFunction("NfoUrl")
                If String.IsNullOrEmpty(stringResults) Then
                    Return Nothing
                End If

                Dim results As XElement = ParseStringUrl(stringResults)
                If IsNothing(results) Then
                    Return Nothing
                End If

                If results.HasElements Then
                    entity = New ScrapeResultsEntity(results, nfoType, Me._parser.Name)
                Else
                    entity = New ScrapeResultsEntity()
                    entity.Urls.Add(New UrlInfo(results))
                End If
                Return entity
            End Function

            Friend Sub Load(ByVal infoScraper As ScraperInfo, ByVal Common As List(Of IncludeInfo))
                Dim sTemp As String = String.Empty
                ScraperInfoItem = infoScraper
                WebpageDownloads = New List(Of UrlInfo)()
                strHTML = New List(Of String)()
                Me._parser = New ScraperParser(ScraperInfoItem)
                For Each item As XElement In Me._parser.XmlScraper.Root.Elements("include")
                    sTemp = item.Value
                    Dim tmp As IncludeInfo = Common.Find(Function(n) n.Name = sTemp)
                    Me._parser.XmlScraper.Root.Add(tmp.Document.Elements())
                Next
                Reset()
            End Sub

            Friend Sub Load(ByVal doc As XDocument)
                Me._parser = New ScraperParser()
                Me._parser.LoadFromXDocument(doc)
                WebpageDownloads = New List(Of UrlInfo)()
                strHTML = New List(Of String)()
            End Sub

            Protected Friend Function PrepareHttp(ByVal xUrlCollection As IEnumerable(Of XElement)) As Boolean
                If xUrlCollection.Count() > 0 Then
                    WebpageDownloads.Clear()
                    For i As Integer = 0 To xUrlCollection.Count() - 1
                        WebpageDownloads.Add(New UrlInfo(xUrlCollection.ElementAt(i)))
                    Next
                    Return HttpGet()
                Else
                    If strHTML.Count > 0 Then
                        For i As Integer = 0 To strHTML.Count - 1
                            Me._parser.setBuffer(i + 1, strHTML(i))
                        Next
                    End If
                    Return True
                End If
            End Function

            Protected Friend Function PrepareHttp(ByVal newUrlList As List(Of UrlInfo)) As Boolean
                If newUrlList.Count() > 0 Then
                    WebpageDownloads = New List(Of UrlInfo)(newUrlList.AsEnumerable())
                    Return HttpGet()
                Else
                    If strHTML.Count > 0 Then
                        For i As Integer = 0 To strHTML.Count - 1
                            Me._parser.setBuffer(i + 1, strHTML(i))
                        Next
                    End If
                    Return True
                End If
            End Function

            Protected Function ParseStringXML(ByVal strXML As String) As XElement
                Dim results As XElement = XmlUtilities.TryParse(strXML)

                If Not IsNothing(results) Then
                    Return results
                Else
                    ErrorMessage(ErrorType.XmlParseError, strXML & ". Will attempt to fix entities and try again.")
                    results = XmlUtilities.TryFixParse(strXML)

                    If Not IsNothing(results) Then
                        Return results
                    Else
                        ErrorMessage(ErrorType.GeneralError, "Repairing entities failed to fix xml parse issues")
                        Return Nothing
                    End If
                End If
            End Function

            Protected Sub Reset()
                Me._primary = String.Empty
                Me._secondary = String.Empty
                Me._tag = Nothing
            End Sub

            Private Sub ParseCustomFunctions(ByVal results As XElement)
                WebpageDownloads.Clear()
                strHTML.Clear()

                For Each item As XElement In results.Elements("url")
                    If Not IsNothing(item.Attribute("function")) Then
                        WebpageDownloads.Add(New UrlInfo(item))
                    End If
                Next
                Dim i As Integer = 0
                While i <= WebpageDownloads.Count - 1
                    If HttpGet(i) Then
                        Dim stringResults As String = Me._parser.ParseFunction(WebpageDownloads(i).Function)
                        results = ParseStringXML(stringResults)
                        If Not IsNothing(results) Then
                            Me._tag.Deserialize(results)

                            For Each item As XElement In results.Elements("url")
                                If Not IsNothing(item.Attribute("function")) Then
                                    WebpageDownloads.Add(New UrlInfo(item))
                                End If
                            Next
                        End If
                    Else
                        If WebpageDownloads.Count > i + 1 Then
                            ErrorMessage(ErrorType.FunctionError, WebpageDownloads(i).Function & ". Will cotinue to next custom function.")
                        Else
                            ErrorMessage(ErrorType.FunctionError, WebpageDownloads(i).Function)
                        End If
                    End If
                    i += 1
                End While
            End Sub

            Private Sub ParseCustomSettings(ByVal results As XElement)
                WebpageDownloads.Clear()
                strHTML.Clear()

                For Each item As XElement In results.Elements("url")
                    If Not IsNothing(item.Attribute("function")) Then
                        WebpageDownloads.Add(New UrlInfo(item))
                    End If
                Next
                Dim i As Integer = 0
                While i <= WebpageDownloads.Count - 1
                    If HttpGet(i) Then
                        Dim stringResults As String = Me._parser.ParseFunction(WebpageDownloads(i).[Function])
                        results = ParseStringXML(stringResults)
                        If Not IsNothing(results) Then
                            Me._settings.Deserialize(results)
                        End If

                        For Each item As XElement In results.Elements("url")
                            If Not IsNothing(item.Attribute("function")) Then
                                WebpageDownloads.Add(New UrlInfo(item))
                            End If
                        Next
                    Else
                        ErrorMessage(ErrorType.GeneralError, "Settings Cannot be loaded for " & Parser.XmlScraper.Root.Attribute("name").Value)
                        Me._settings = Nothing
                        Return
                    End If
                    i += 1
                End While
            End Sub

            Private Function ParseStringUrl(ByVal url As String) As XElement
                Dim results As XElement = XmlUtilities.TryParse(url)

                If IsNothing(results) Then
                    ErrorMessage(ErrorType.XmlParseError, url & ". Will try to parse as sibling elements")
                    If XmlUtilities.HasElements(url) Then
                        results = XmlUtilities.AddRootElementAndParse(url, "entity")
                    Else
                        ErrorMessage(ErrorType.GeneralError, "Could not parse CreateSearchUrl results as sibling elements: Last Resort: Parsing as string.")
                        results = New XElement("url", url)
                    End If
                End If

                Return results
            End Function

            Private Function VerifyProperContent(ByVal parser As ScraperParser, ByVal attemptedContent As MediaType) As Boolean
                Dim contentType As String = Me._scraperLookups(attemptedContent).ContentType

                If parser.Content <> contentType Then
                    ErrorMessage(ErrorType.ContentError, contentType)
                    Return False
                End If

                Return True
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
