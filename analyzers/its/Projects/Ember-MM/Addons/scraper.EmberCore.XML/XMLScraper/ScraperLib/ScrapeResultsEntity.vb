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
Imports System.Xml.Linq

Imports EmberScraperModule.XMLScraper.MediaTags
Imports EmberScraperModule.XMLScraper.Utilities

Namespace XMLScraper
    Namespace ScraperLib

        Public Class ScrapeResultsEntity
            Implements IComparable(Of ScrapeResultsEntity)

#Region "Fields"

            Friend ScraperName As String

            Private m_Description As String
            Private m_Genres As List(Of String)
            Private m_ID As String
            Private m_Number As Integer
            Private m_ReleaseDate As String
            Private m_ReleasedBy As String
            Private m_Relevance As Integer
            Private m_Thumb As Thumbnail
            Private m_Title As String
            Private m_TypeOfSearch As MediaType
            Private m_Urls As List(Of UrlInfo)
            Private m_Volume As Integer
            Private m_Year As Integer

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal searchResults As XElement, ByVal typeOfResults As MediaType, ByVal nameOfScraper As String)
                Me.New()
                Deserialize(searchResults)
                ScraperName = nameOfScraper
                TypeOfSearch = typeOfResults
            End Sub

            Friend Sub New()
                Urls = New List(Of UrlInfo)()
                Thumb = New Thumbnail()
            End Sub

            Private Sub New(ByVal entity As ScrapeResultsEntity)
                Me.New()
                Me.ScraperName = entity.ScraperName
                Me.Thumb = entity.Thumb
                Me.TypeOfSearch = entity.TypeOfSearch

                Me.Urls = New List(Of UrlInfo)(entity.Urls)
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Description() As String
                Get
                    Return m_Description
                End Get
                Private Set(ByVal value As String)
                    m_Description = Value
                End Set
            End Property

            Public Property Genres() As List(Of String)
                Get
                    Return m_Genres
                End Get
                Private Set(ByVal value As List(Of String))
                    m_Genres = Value
                End Set
            End Property

            Public Property ID() As String
                Get
                    Return m_ID
                End Get
                Private Set(ByVal value As String)
                    m_ID = Value
                End Set
            End Property

            Public Property Number() As Integer
                Get
                    Return m_Number
                End Get
                Private Set(ByVal value As Integer)
                    m_Number = Value
                End Set
            End Property

            Public Property ReleaseDate() As String
                Get
                    Return m_ReleaseDate
                End Get
                Private Set(ByVal value As String)
                    m_ReleaseDate = Value
                End Set
            End Property

            Public Property ReleasedBy() As String
                Get
                    Return m_ReleasedBy
                End Get
                Private Set(ByVal value As String)
                    m_ReleasedBy = Value
                End Set
            End Property

            Public Property Relevance() As Integer
                Get
                    Return m_Relevance
                End Get
                Friend Set(ByVal value As Integer)
                    m_Relevance = Value
                End Set
            End Property

            Public Property Thumb() As Thumbnail
                Get
                    Return m_Thumb
                End Get
                Set(ByVal value As Thumbnail)
                    m_Thumb = Value
                End Set
            End Property

            Public Property Title() As String
                Get
                    Return m_Title
                End Get
                Private Set(ByVal value As String)
                    m_Title = Value
                End Set
            End Property

            Public Property TypeOfSearch() As MediaType
                Get
                    Return m_TypeOfSearch
                End Get
                Friend Set(ByVal value As MediaType)
                    m_TypeOfSearch = Value
                End Set
            End Property

            Public Property Urls() As List(Of UrlInfo)
                Get
                    Return m_Urls
                End Get
                Private Set(ByVal value As List(Of UrlInfo))
                    m_Urls = Value
                End Set
            End Property

            Public Property Volume() As Integer
                Get
                    Return m_Volume
                End Get
                Private Set(ByVal value As Integer)
                    m_Volume = Value
                End Set
            End Property

            Public Property Year() As Integer
                Get
                    Return m_Year
                End Get
                Private Set(ByVal value As Integer)
                    m_Year = Value
                End Set
            End Property

            Friend ReadOnly Property ScraperType() As ScraperContent
                Get
                    Return GetScraperContent()
                End Get
            End Property

#End Region 'Properties

#Region "Methods"

            Public Shared Function BlankSerialize(ByVal type As MediaType) As XElement
                Dim element As New XElement("results", New XAttribute("sorted", "yes"))
                Dim entity As New XElement("entity")

                Dim title As XElement
                Dim id As XElement
                Dim description As XElement
                Dim url As XElement
                Dim thumb As XElement
                Dim volume As XElement
                Dim number As XElement
                Dim releasedate As XElement
                Dim genres As XElement
                Dim studio As XElement
                Dim year As XElement
                Dim country As XElement

                Select Case type
                    Case MediaType.album
                        title = New XElement("title")
                        id = New XElement("id")
                        url = UrlInfo.BlankSerialize(UrlInfo.UrlTypes.general)
                        thumb = Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general)
                        volume = Nothing
                        number = Nothing
                        releasedate = New XElement("releasedate")
                        genres = New XElement("genre")
                        studio = New XElement("artist")
                        year = New XElement("year")
                        country = Nothing
                        Exit Select
                    Case MediaType.artist
                        title = New XElement("title")
                        id = New XElement("id")
                        description = Nothing
                        url = UrlInfo.BlankSerialize(UrlInfo.UrlTypes.general)
                        thumb = Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general)
                        volume = Nothing
                        number = Nothing
                        releasedate = Nothing
                        genres = New XElement("genre")
                        studio = Nothing
                        year = Nothing
                        country = Nothing
                        Exit Select
                    Case MediaType.movie
                        title = New XElement("title")
                        id = New XElement("id")
                        description = Nothing
                        url = UrlInfo.BlankSerialize(UrlInfo.UrlTypes.general)
                        thumb = Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general)
                        volume = Nothing
                        number = Nothing
                        releasedate = New XElement("premiered")
                        genres = New XElement("genre")
                        studio = New XElement("studio")
                        year = New XElement("year")
                        country = New XElement("country")
                        Exit Select
                    Case MediaType.musicvideo
                        title = New XElement("title")
                        id = New XElement("id")
                        description = Nothing
                        url = UrlInfo.BlankSerialize(UrlInfo.UrlTypes.general)
                        thumb = Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general)
                        volume = Nothing
                        number = Nothing
                        releasedate = New XElement("premiered")
                        genres = New XElement("genre")
                        studio = New XElement("artist")
                        year = New XElement("year")
                        country = Nothing
                        Exit Select
                    Case MediaType.person
                        title = New XElement("name")
                        id = New XElement("id")
                        description = New XElement("role")
                        url = UrlInfo.BlankSerialize(UrlInfo.UrlTypes.general)
                        thumb = Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general)
                        volume = Nothing
                        number = Nothing
                        releasedate = New XElement("yearsactive")
                        genres = New XElement("genre")
                        studio = Nothing
                        year = Nothing
                        country = Nothing
                        Exit Select
                    Case MediaType.tvepisode
                        element = New XElement("episodeguide")
                        entity = New XElement("episode")

                        title = New XElement("title")
                        id = New XElement("id")
                        url = UrlInfo.BlankSerialize(UrlInfo.UrlTypes.general)
                        thumb = Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general)
                        volume = New XElement("season")
                        number = New XElement("epnum")
                        releasedate = New XElement("aired")
                        genres = New XElement("genre")
                        studio = New XElement("studio")
                        year = Nothing
                        country = Nothing
                        Exit Select
                    Case MediaType.tvshow
                        title = New XElement("title")
                        id = New XElement("id")
                        description = Nothing
                        url = UrlInfo.BlankSerialize(UrlInfo.UrlTypes.general)
                        thumb = Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general)
                        volume = Nothing
                        number = Nothing
                        releasedate = New XElement("premiered")
                        genres = New XElement("genre")
                        studio = New XElement("studio")
                        year = New XElement("year")
                        country = Nothing
                        Exit Select
                    Case Else
                        title = New XElement("title")
                        id = New XElement("id")
                        description = New XElement("description")
                        url = UrlInfo.BlankSerialize(UrlInfo.UrlTypes.general)
                        thumb = Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general)
                        volume = Nothing
                        number = Nothing
                        releasedate = New XElement("date")
                        genres = New XElement("category")
                        studio = Nothing
                        year = New XElement("year")
                        country = Nothing
                        Exit Select
                End Select

                If Not IsNothing(title) Then
                    entity.Add(title)
                End If

                If Not IsNothing(id) Then
                    entity.Add(id)
                End If

                If Not IsNothing(year) Then
                    entity.Add(New XElement("year"))
                End If

                If Not IsNothing(releasedate) Then
                    entity.Add(releasedate)
                End If

                If Not IsNothing(volume) Then
                    entity.Add(volume)
                End If

                If Not IsNothing(number) Then
                    entity.Add(number)
                End If

                If Not IsNothing(studio) Then
                    entity.Add(studio)
                End If

                If Not IsNothing(genres) Then
                    entity.Add(genres)
                    entity.Add(genres)
                End If

                If Not IsNothing(country) Then
                    entity.Add(country)
                End If

                entity.Add(New XElement("relevance"))

                entity.Add(thumb)
                entity.Add(url)
                entity.Add(url)

                element.Add(entity)

                Return element
            End Function

            Public Function Clone() As ScrapeResultsEntity
                Dim output As New ScrapeResultsEntity(Me)
                Return output
            End Function

            Public Function CompareByID(ByVal other As ScrapeResultsEntity) As Integer
                If IsNothing(Me) Then
                    If IsNothing(other) Then
                        Return 0
                    Else
                        Return 1
                    End If
                Else
                    If IsNothing(other) Then
                        Return -1
                    Else
                        Return Me.ID.CompareTo(other.ID)
                    End If
                End If
            End Function

            Public Function CompareByReleasedBy(ByVal other As ScrapeResultsEntity) As Integer
                If IsNothing(Me) Then
                    If IsNothing(other) Then
                        Return 0
                    Else
                        Return 1
                    End If
                Else
                    If IsNothing(other) Then
                        Return -1
                    Else
                        Return Me.ReleasedBy.CompareTo(other.ReleasedBy)
                    End If
                End If
            End Function

            Public Function CompareByTitle(ByVal other As ScrapeResultsEntity) As Integer
                If IsNothing(Me) Then
                    If IsNothing(other) Then
                        Return 0
                    Else
                        Return 1
                    End If
                Else
                    If IsNothing(other) Then
                        Return -1
                    Else
                        Return Me.Title.CompareTo(other.Title)
                    End If
                End If
            End Function

            Public Function CompareByYear(ByVal other As ScrapeResultsEntity) As Integer
                If IsNothing(Me) Then
                    If IsNothing(other) Then
                        Return 0
                    Else
                        Return 1
                    End If
                Else
                    If IsNothing(other) Then
                        Return -1
                    Else
                        Return Me.Year.CompareTo(other.Year)
                    End If
                End If
            End Function

            Public Function CompareTo(ByVal other As ScrapeResultsEntity) As Integer Implements IComparable(Of ScrapeResultsEntity).CompareTo
                If IsNothing(Me) Then
                    If IsNothing(other) Then
                        Return 0
                    Else
                        Return 1
                    End If
                Else
                    If IsNothing(other) Then
                        Return -1
                    Else
                        Return Me.Relevance.CompareTo(other.Relevance)
                    End If
                End If
            End Function

            Private Sub Deserialize(ByVal xResults As XElement)
                For Each item As XElement In xResults.Elements()
                    Select Case item.Name.ToString()
                        Case "name", "title"
                            Title = item.Value
                            Exit Select
                        Case "id"
                            ID = item.Value
                            Exit Select
                        Case "year"
                            Try
                                Year = Convert.ToInt32(item.Value)
                            Catch
                                Year = 0
                            End Try
                            Exit Select
                        Case "releasedate", "premiered", "date"
                            ReleaseDate = item.Value
                            Exit Select
                        Case "studio", "artist", "developer", "author", "label"
                            ReleasedBy = item.Value
                            Exit Select
                        Case "genre", "category", "keyword"
                            Genres.Add(item.Value)
                            Exit Select
                        Case "thumb"
                            Thumb = New Thumbnail(item)
                            Exit Select
                        Case "url"
                            Urls.Add(New UrlInfo(item))
                            Exit Select
                        Case "volume", "season"
                            Try
                                Volume = Convert.ToInt32(item.Value)
                            Catch
                                Volume = 0
                            End Try
                            Exit Select
                        Case "issue", "epnum", "episode"
                            Try
                                Number = Convert.ToInt32(item.Value)
                            Catch
                                Number = 0
                            End Try
                            Exit Select
                        Case "description", "article"
                            Description = item.Value
                            Exit Select
                        Case Else
                            Exit Select
                    End Select
                Next
            End Sub

            Private Function ExtractUrls() As List(Of UrlInfo)
                Return Urls
            End Function

            Private Function GetScraperContent() As ScraperContent
                Select Case TypeOfSearch
                    Case MediaType.album, MediaType.artist
                        Return ScraperContent.albums
                    Case MediaType.movie
                        Return ScraperContent.movies
                    Case MediaType.musicvideo
                        Return ScraperContent.musicvideos
                    Case MediaType.tvepisode, MediaType.tvshow
                        Return ScraperContent.tvshows
                End Select
            End Function

            Private Function Serialize() As XElement
                Dim elementname As String = "entity"

                If Me.TypeOfSearch = MediaType.tvepisode Then
                    elementname = "episode"
                End If

                Dim tmp As New XElement(elementname)

                Return tmp
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
