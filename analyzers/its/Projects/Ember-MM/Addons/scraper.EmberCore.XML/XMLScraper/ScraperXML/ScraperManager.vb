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
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Xml.Linq

Imports Microsoft.VisualBasic

Imports EmberScraperModule.XMLScraper.MediaTags
Imports EmberScraperModule.XMLScraper.ScraperLib
Imports EmberScraperModule.XMLScraper.Utilities

Namespace XMLScraper
    Namespace ScraperXML

        Public NotInheritable Class ScraperManager

#Region "Fields"

            Private Common As List(Of IncludeInfo)
            Private pCache As String
            Private pFolder As String
            Private _allscrapers As List(Of ScraperInfo)

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal strCacheFolder As String)
                Me.Clear()
                pCache = strCacheFolder
                FileIO.VerifyCacheFolder(pCache)
            End Sub

            Public Sub New(ByVal strScraperFolder As String, ByVal strCacheFolder As String)
                Me.Clear()
                pCache = strCacheFolder
                FileIO.VerifyCacheFolder(pCache)
                pFolder = strScraperFolder
            End Sub

            Public Sub New()
                Me.Clear()
            End Sub

#End Region 'Constructors

#Region "Events"

            Public Event NewLogInfo As EventHandler(Of LogInfoEventArgs)

            Public Event NoResultsFound As EventHandler(Of DetailsRetrievedEventArgs)

            Public Event ProgressChanged As EventHandler(Of SearchProgressChangedEventArgs)

            Public Event RetrievedDetails As EventHandler(Of DetailsRetrievedEventArgs)

            Public Event RetrievedResults As EventHandler(Of SearchResultsRetrievedEventArgs)

#End Region 'Events

#Region "Properties"

            Public Property AllScrapers() As List(Of ScraperInfo)
                Get
                    Return Me._allscrapers
                End Get
                Private Set(ByVal value As List(Of ScraperInfo))
                    Me._allscrapers = value
                End Set
            End Property

            Public Property CacheFolder() As String
                Get
                    Return pCache
                End Get
                Set(ByVal value As String)
                    If Not String.IsNullOrEmpty(value) Then
                        If value.EndsWith(Path.DirectorySeparatorChar.ToString) Then
                            pCache = value
                        Else
                            pCache = String.Concat(value, Path.DirectorySeparatorChar)
                        End If

                        For Each item As ScraperInfo In Me._allscrapers
                            item.ScraperCache = pCache
                        Next
                    End If
                End Set
            End Property

            Public ReadOnly Property Movies() As List(Of ScraperInfo)
                Get
                    Return Me._allscrapers.FindAll(Function(n) n.ScraperContent = ScraperContent.movies)
                End Get
            End Property

            Public ReadOnly Property Music() As List(Of ScraperInfo)
                Get
                    Return Me._allscrapers.FindAll(Function(n) n.ScraperContent = ScraperContent.albums)
                End Get
            End Property

            Public ReadOnly Property MusicVideos() As List(Of ScraperInfo)
                Get
                    Return Me._allscrapers.FindAll(Function(n) n.ScraperContent = ScraperContent.musicvideos)
                End Get
            End Property

            Public Property ScraperFolder() As String
                Get
                    Return pFolder
                End Get
                Set(ByVal value As String)
                    If Not String.IsNullOrEmpty(value) Then
                        If value.EndsWith(Path.DirectorySeparatorChar.ToString()) Then
                            pFolder = value
                        Else
                            pFolder = String.Concat(value, Path.DirectorySeparatorChar)
                        End If
                        ReloadScrapers()
                    End If
                End Set
            End Property

            Public ReadOnly Property TvShows() As List(Of ScraperInfo)
                Get
                    Return Me._allscrapers.FindAll(Function(n) n.ScraperContent = ScraperContent.tvshows)
                End Get
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub Clear()
                Me._allscrapers = New List(Of ScraperInfo)
                Me.Common = New List(Of IncludeInfo)
                Me.pCache = String.Empty
                Me.pFolder = String.Empty
            End Sub

            Public Function GetDetails(ByVal resultsEntity As ScrapeResultsEntity) As MediaTag
                Dim iScraper As ScraperInfo = GetList(resultsEntity.TypeOfSearch).Find(Function(n) n.ScraperName = resultsEntity.ScraperName)
                If IsNothing(iScraper) Then
                    Return Nothing
                End If

                Dim tag As MediaTag = Details(iScraper, resultsEntity)

                If IsNothing(tag) Then
                    RaiseEvent NoResultsFound(Me, New DetailsRetrievedEventArgs(resultsEntity, Nothing))
                Else
                    RaiseEvent RetrievedDetails(Me, New DetailsRetrievedEventArgs(resultsEntity, tag))
                End If

                Return tag
            End Function

            Public Function GetList(ByVal type As MediaType) As List(Of ScraperInfo)
                Select Case type
                    Case MediaType.tvshow, MediaType.tvepisode
                        Return TvShows
                    Case MediaType.album
                        Return Music
                    Case MediaType.artist
                        Return Music.FindAll(Function(n) n.HasSecondarySearch)
                    Case MediaType.musicvideo
                        Return MusicVideos
                    Case Else
                        Return Movies
                End Select
            End Function

            Public Function GetResults(ByVal scraperName As String, ByVal primary As String, ByVal secondary As String, ByVal resultsType As MediaType) As List(Of ScrapeResultsEntity)
                Dim tempList As New List(Of ScrapeResultsEntity)

                Dim scraper As ScraperInfo = Me._allscrapers.Find(Function(n) [String].Compare(n.ScraperName, scraperName, StringComparison.OrdinalIgnoreCase) = 0)

                If Not IsNothing(scraper) Then
                    tempList = Search(scraper, primary, secondary, resultsType)
                Else
                    tempList = New List(Of ScrapeResultsEntity)()
                End If

                RaiseEvent RetrievedResults(Me, New SearchResultsRetrievedEventArgs(scraperName, primary, secondary, tempList))

                Return tempList
            End Function

            Public Sub LoadScrapers(ByVal strScraperFolder As String, ByVal strCacheFolder As String)
                Me.pCache = strCacheFolder
                Me.pFolder = strScraperFolder
                ReloadScrapers()
            End Sub

            Public Sub LoadScrapers(ByVal fname As String)
                Me._allscrapers = New List(Of ScraperInfo)()
                Me.Common = New List(Of IncludeInfo)()

                If Directory.Exists(pFolder) Then
                    Dim scraperDir As New DirectoryInfo(pFolder)

                    For Each item As FileInfo In scraperDir.GetFiles("*.xml", SearchOption.AllDirectories)
                        Dim doc As XDocument = XmlUtilities.TryLoadDocument(item.FullName)
                        If Path.GetFileName(item.FullName) = fname AndAlso XmlUtilities.IsScraper(doc) Then
                            Dim tempInfo As New ScraperInfo()
                            If tempInfo.Load(doc, item.FullName) Then
                                If Not IsNothing(tempInfo.Settings) Then
                                    tempInfo.ScraperCache = Me.pCache
                                    Me._allscrapers.Add(tempInfo)
                                End If
                            End If
                        ElseIf XmlUtilities.IsCommonFile(doc) Then
                            Common.Add(New IncludeInfo(doc, item.FullName))
                        Else
                            Continue For
                        End If
                    Next
                End If
                AllScrapers.Sort()
            End Sub

            Public Sub ReloadScrapers(Optional ByVal content As Integer = -1)
                Me._allscrapers = New List(Of ScraperInfo)()
                Me.Common = New List(Of IncludeInfo)()

                If Directory.Exists(pFolder) Then
                    Dim scraperDir As New DirectoryInfo(pFolder)

                    For Each item As FileInfo In scraperDir.GetFiles("*.xml", SearchOption.AllDirectories)
                        Dim doc As XDocument = XmlUtilities.TryLoadDocument(item.FullName)
                        If XmlUtilities.IsScraper(doc) Then
                            Dim tempInfo As New ScraperInfo()
                            If tempInfo.Load(doc, item.FullName) Then
                                If Not IsNothing(tempInfo.Settings) Then
                                    tempInfo.ScraperCache = Me.pCache
                                    If content = -1 OrElse content = tempInfo.ScraperContent Then
                                        Me._allscrapers.Add(tempInfo)
                                    End If

                                End If
                            End If
                        ElseIf XmlUtilities.IsCommonFile(doc) Then
                            Common.Add(New IncludeInfo(doc, item.FullName))
                        Else
                            Continue For
                        End If
                    Next
                End If
                AllScrapers.Sort()
            End Sub

            Private Shared Function LoadDetails(ByVal temp As XElement, ByVal detailsType As MediaType) As MediaTag
                Dim tagReturn As MediaTag = MediaTag.TagFromResultsType(detailsType)
                tagReturn.Deserialize(temp)
                Return tagReturn
            End Function

            Private Function Details(ByVal scraper As ScraperInfo, ByVal resultsEntity As ScrapeResultsEntity) As MediaTag
                Dim scrParser As GenericScraper = GetNewScraper(scraper, Common)
                Dim detailsType As MediaType = resultsEntity.TypeOfSearch

                If Not scrParser.PrepareHttp(resultsEntity.Urls) Then
                    Return Nothing
                End If

                If Not scrParser.InternalGetDetails(resultsEntity) Then
                    Return Nothing
                End If

                Return scrParser.Tag
            End Function

            Private Function GetNewScraper(ByVal info As ScraperInfo, ByVal common As List(Of IncludeInfo)) As GenericScraper
                Dim scraper As New GenericScraper
                scraper.Load(info, common)
                Return scraper
            End Function

            Private Function MultiSearch(ByVal scrapersToScrape As IEnumerable(Of ScraperInfo), ByVal Primary As String, ByVal Secondary As String, ByVal resultsType As MediaType) As List(Of ScrapeResultsEntity)
                Dim tempList As New List(Of ScrapeResultsEntity)()
                Dim count As Integer = 0
                For Each scInfoItem As ScraperInfo In scrapersToScrape
                    count += 1
                    RaiseEvent ProgressChanged(Me, New SearchProgressChangedEventArgs(scrapersToScrape.Count(), count, scInfoItem.ScraperName))
                    Dim temp As ScraperInfo = scInfoItem
                    Dim scrParser As GenericScraper = GetNewScraper(temp, Common)

                    scrParser.Primary = Primary
                    scrParser.Secondary = Secondary

                    Dim searchResults As XElement = scrParser.InternalCreateSearchUrl(resultsType)
                    If IsNothing(searchResults) Then
                        Continue For
                    End If

                    For Each item As XElement In searchResults.Elements("entity")
                        tempList.Add(New ScrapeResultsEntity(item, resultsType, scrParser.Parser.Name))
                    Next
                Next
                Return tempList
            End Function

            Private Function NfoUrl(ByVal nfoString As String, ByVal infoScraper As ScraperInfo, ByVal detailsType As MediaType) As Boolean
                Dim scrParser As GenericScraper = GetNewScraper(infoScraper, Common)
                Dim entity As ScrapeResultsEntity = scrParser.InternalNfoUrl(nfoString, detailsType)

                If IsNothing(entity) Then Return False

                If Not scrParser.InternalGetDetails(entity) Then Return False

                Return True
            End Function

            Private Function Search(ByVal scraper As ScraperInfo, ByVal primary As String, ByVal secondary As String, ByVal typeSearch As MediaType) As List(Of ScrapeResultsEntity)
                Dim scrParser As GenericScraper = GetNewScraper(scraper, Common)
                scrParser.Primary = primary
                scrParser.Secondary = secondary

                Dim tempResults As New List(Of ScrapeResultsEntity)()
                Dim searchResults As XElement = scrParser.InternalCreateSearchUrl(typeSearch)
                If IsNothing(searchResults) Then
                    Return tempResults
                End If

                Dim bSorted As Boolean = False
                Dim attSort As XAttribute = searchResults.Attribute("sorted")
                If Not IsNothing(attSort) Then
                    If attSort.Value = "yes" Then
                        bSorted = True
                    End If
                End If

                For Each item As XElement In searchResults.Elements("entity")
                    tempResults.Add(New ScrapeResultsEntity(item, typeSearch, scrParser.Parser.Name))
                Next

                Return tempResults
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
