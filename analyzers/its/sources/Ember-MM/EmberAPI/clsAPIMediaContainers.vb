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

Imports System.Xml.Serialization

Namespace MediaContainers

    <XmlRoot("episodedetails")> _
    Public Class EpisodeDetails

        #Region "Fields"

        Private _title As String
        Private _aired As String
        Private _runtime As String
        Private _rating As String
        Private _season As Integer
        Private _episode As Integer
        Private _plot As String
        Private _credits As String
        Private _director As String
        Private _actors As New List(Of Person)
        Private _fileInfo As New MediaInfo.Fileinfo
        Private _poster As Images
        Private _posterurl As String
        Private _localfile As String
        Private _fanart As Images

        #End Region 'Fields

        #Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

        #End Region 'Constructors

        #Region "Properties"

        <XmlElement("title")> _
        Public Property Title() As String
            Get
                Return Me._title
            End Get
            Set(ByVal value As String)
                Me._title = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property TitleSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._title)
            End Get
        End Property

        <XmlElement("runtime")> _
        Public Property Runtime() As String
            Get
                Return Me._runtime
            End Get
            Set(ByVal value As String)
                Me._runtime = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property RuntimeSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._runtime)
            End Get
        End Property

        <XmlElement("aired")> _
        Public Property Aired() As String
            Get
                Return Me._aired
            End Get
            Set(ByVal value As String)
                Me._aired = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property AiredSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._aired)
            End Get
        End Property

        <XmlElement("rating")> _
        Public Property Rating() As String
            Get
                Return Me._rating.Replace(",", ".")
            End Get
            Set(ByVal value As String)
                Me._rating = value.Replace(",", ".")
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property RatingSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._rating)
            End Get
        End Property

        <XmlElement("season")> _
        Public Property Season() As Integer
            Get
                Return Me._season
            End Get
            Set(ByVal value As Integer)
                Me._season = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property SeasonSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._season.ToString)
            End Get
        End Property

        <XmlElement("episode")> _
        Public Property Episode() As Integer
            Get
                Return Me._episode
            End Get
            Set(ByVal value As Integer)
                Me._episode = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property EpisodeSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._episode.ToString)
            End Get
        End Property

        <XmlElement("plot")> _
        Public Property Plot() As String
            Get
                Return Me._plot
            End Get
            Set(ByVal value As String)
                Me._plot = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property PlotSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._plot)
            End Get
        End Property

        <XmlElement("credits")> _
        Public Property Credits() As String
            Get
                Return Me._credits
            End Get
            Set(ByVal value As String)
                Me._credits = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property CreditsSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._credits)
            End Get
        End Property

        <XmlElement("director")> _
        Public Property Director() As String
            Get
                Return Me._director
            End Get
            Set(ByVal value As String)
                Me._director = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property DirectorSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._director)
            End Get
        End Property

        <XmlElement("actor")> _
        Public Property Actors() As List(Of Person)
            Get
                Return Me._actors
            End Get
            Set(ByVal Value As List(Of Person))
                Me._actors = Value
            End Set
        End Property

        <XmlIgnore> _
        Public ReadOnly Property ActorsSpecified() As Boolean
            Get
                Return Me._actors.Count > 0
            End Get
        End Property

        <XmlElement("fileinfo")> _
        Public Property FileInfo() As MediaInfo.Fileinfo
            Get
                Return Me._fileInfo
            End Get
            Set(ByVal value As MediaInfo.Fileinfo)
                Me._fileInfo = value
            End Set
        End Property

        <XmlIgnore> _
        Public ReadOnly Property FileInfoSpecified() As Boolean
            Get
                If Me._fileInfo.StreamDetails.Video.Count > 0 OrElse _
                Me._fileInfo.StreamDetails.Audio.Count > 0 OrElse _
                 Me._fileInfo.StreamDetails.Subtitle.Count > 0 Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        <XmlIgnore()> _
        Public Property Poster() As Images
            Get
                Return Me._poster
            End Get
            Set(ByVal value As Images)
                Me._poster = value
            End Set
        End Property

        <XmlIgnore()> _
        Public Property PosterURL() As String
            Get
                Return Me._posterurl
            End Get
            Set(ByVal value As String)
                Me._posterurl = value
            End Set
        End Property

        <XmlIgnore()> _
        Public Property LocalFile() As String
            Get
                Return Me._localfile
            End Get
            Set(ByVal value As String)
                Me._localfile = value
            End Set
        End Property

        <XmlIgnore()> _
        Public Property Fanart() As Images
            Get
                Return Me._fanart
            End Get
            Set(ByVal value As Images)
                Me._fanart = value
            End Set
        End Property

        #End Region 'Properties

        #Region "Methods"

        Public Sub Clear()
            Me._title = String.Empty
            Me._season = -999
            Me._episode = -999
            Me._aired = String.Empty
            Me._rating = String.Empty
            Me._runtime = String.Empty
            Me._plot = String.Empty
            Me._director = String.Empty
            Me._credits = String.Empty
            Me._actors.Clear()
            Me._fileInfo = New MediaInfo.Fileinfo
            Me._posterurl = String.Empty
            Me._localfile = String.Empty
            Me._poster = New Images
            Me._fanart = New Images
        End Sub

        #End Region 'Methods

    End Class

    Public Class Fanart

        #Region "Fields"

        Private _thumb As New List(Of Thumb)
        Private _url As String

        #End Region 'Fields

        #Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

        #End Region 'Constructors

        #Region "Properties"

        <XmlElement("thumb")> _
        Public Property Thumb() As List(Of Thumb)
            Get
                Return Me._thumb
            End Get
            Set(ByVal value As List(Of Thumb))
                Me._thumb = value
            End Set
        End Property

        <XmlAttribute("url")> _
        Public Property URL() As String
            Get
                Return Me._url
            End Get
            Set(ByVal value As String)
                Me._url = value
            End Set
        End Property

        #End Region 'Properties

        #Region "Methods"

        Public Sub Clear()
            Me._thumb.Clear()
            Me._url = String.Empty
        End Sub

        #End Region 'Methods

    End Class

    <XmlRoot("movie")> _
    Public Class Movie
        Implements IComparable(Of Movie)

        #Region "Fields"

        Private _title As String
        Private _originaltitle As String
        Private _sorttitle As String
        Private _year As String
        Private _releaseDate As String
        Private _top250 As String
        Private _countries As New List(Of String)
        Private _rating As String
        Private _votes As String
        Private _mpaa As String
        Private _certification As String
        Private _genres As New List(Of String)
        Private _studio As String
        Private _directors As New List(Of String)
        Private _credits As New List(Of String)
        Private _tagline As String
        Private _outline As String
        Private _plot As String
        Private _runtime As String
        Private _trailer As String
        Private _playcount As String
        Private _watched As String
        Private _actors As New List(Of Person)
        Private _thumb As New List(Of String)
        Private _fanart As New Fanart
        Private _xsets As New List(Of [Set])
        Private _ysets As New SetContainer
        Private _fileInfo As New MediaInfo.Fileinfo
        Private _lev As Integer
        Private _videosource As String
#End Region 'Fields

#Region "Constructors"

        Public Sub New(ByVal sID As String, ByVal sTitle As String, ByVal sYear As String, ByVal iLev As Integer)
            Me.Clear()
            Me.MovieID.ID = sID
            Me._title = sTitle
            Me._year = sYear
            Me._lev = iLev
        End Sub

        Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"
        <XmlElement("id")> _
        Public MovieID As New _MovieID

        <XmlElement("title")> _
        Public Property Title() As String
            Get
                Return Me._title
            End Get
            Set(ByVal value As String)
                Me._title = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property TitleSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._title)
            End Get
        End Property

        <XmlElement("originaltitle")> _
        Public Property OriginalTitle() As String
            Get
                Return Me._originaltitle
            End Get
            Set(ByVal value As String)
                Me._originaltitle = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property OriginalTitleSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._originaltitle)
            End Get
        End Property

        <XmlElement("sorttitle")> _
        Public Property SortTitle() As String
            Get
                Return Me._sorttitle
            End Get
            Set(ByVal value As String)
                Me._sorttitle = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property SortTitleSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._sorttitle) AndAlso Not Me._sorttitle = StringUtils.FilterTokens(Me._title)
            End Get
        End Property

        <XmlIgnore()> _
        Public Property ID() As String
            Get
                Return Me.MovieID.ID
            End Get
            Set(ByVal value As String)
                Me.MovieID.ID = value
            End Set
        End Property
        <XmlIgnore()> _
        Public Property IDMovieDB() As String
            Get
                Return Me.MovieID.IDMovieDB
            End Get
            Set(ByVal value As String)
                Me.MovieID.IDMovieDB = value
            End Set
        End Property

        <XmlIgnore()> _
        Public Property IMDBID() As String
            Get
                Return MovieID.ID.Replace("tt", String.Empty).Trim
            End Get
            Set(ByVal value As String)
                Me.MovieID.ID = value
            End Set
        End Property

        <XmlElement("year")> _
        Public Property Year() As String
            Get
                Return Me._year
            End Get
            Set(ByVal value As String)
                Me._year = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property YearSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._year)
            End Get
        End Property

        <XmlElement("releasedate")> _
        Public Property ReleaseDate() As String
            Get
                Return Me._releaseDate
            End Get
            Set(ByVal value As String)
                Me._releaseDate = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property ReleaseDateSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._releaseDate)
            End Get
        End Property

        <XmlElement("top250")> _
        Public Property Top250() As String
            Get
                Return Me._top250
            End Get
            Set(ByVal value As String)
                Me._top250 = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property Top250Specified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._top250)
            End Get
        End Property

        <XmlElement("country")> _
        Public Property Countries() As List(Of String)
            Get
                Return _countries
            End Get
            Set(ByVal value As List(Of String))
                If IsNothing(value) Then
                    _countries.Clear()
                Else
                    _countries = value
                End If
            End Set
        End Property

        <Obsolete("This property is depreciated. Use Movie.Countries instead.")> _
        <XmlIgnore()> _
        Public Property LCountry() As List(Of String)
            Get
                Return Countries
            End Get
            Set(ByVal value As List(Of String))
                Countries = value
            End Set
        End Property

        <Obsolete("This property is depreciated. Use 'Movie.Country.Count > 0' instead.")> _
        <XmlIgnore()> _
        Public ReadOnly Property LCountrySpecified() As Boolean
            Get
                Return (_countries.Count > 0)
            End Get
        End Property


        <Obsolete("This property is depreciated. Use Movie.Countries [List(Of String)] instead.")> _
        <XmlIgnore()> _
        Public Property Country() As String
            Get
                Return String.Join(" / ", _countries.ToArray)
            End Get
            Set(ByVal value As String)
                _countries.Clear()
                AddCountry(value)
            End Set
        End Property

        <XmlElement("rating")> _
        Public Property Rating() As String
            Get
                Return Me._rating.Replace(",", ".")
            End Get
            Set(ByVal value As String)
                Me._rating = value.Replace(",", ".")
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property RatingSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._rating)
            End Get
        End Property

        <XmlElement("votes")> _
        Public Property Votes() As String
            Get
                Return Me._votes
            End Get
            Set(ByVal value As String)
                Me._votes = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property VotesSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._votes)
            End Get
        End Property

        <XmlElement("mpaa")> _
        Public Property MPAA() As String
            Get
                Return Me._mpaa
            End Get
            Set(ByVal value As String)
                Me._mpaa = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property MPAASpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._mpaa)
            End Get
        End Property

        <XmlElement("certification")> _
        Public Property Certification() As String
            Get
                Return Me._certification
            End Get
            Set(ByVal value As String)
                Me._certification = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property CertificationSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._certification)
            End Get
        End Property

        <XmlElement("genre")> _
        Public Property Genres() As List(Of String)
            Get
                Return _genres
            End Get
            Set(ByVal value As List(Of String))
                If IsNothing(value) Then
                    _genres.Clear()
                Else
                    _genres = value
                End If
            End Set
        End Property

        <Obsolete("This property is depreciated. Use Movie.Genres.Count instead.")> _
        <XmlIgnore()> _
        Public Property LGenre() As List(Of String)
            Get
                Return Genres
            End Get
            Set(ByVal value As List(Of String))
                Genres = value
            End Set
        End Property

        <Obsolete("This property is depreciated. Use 'Movie.Genres.Count > 0' instead.")> _
        <XmlIgnore()> _
        Public ReadOnly Property LGenreSpecified() As Boolean
            Get
                Return (_genres.Count > 0)
            End Get
        End Property

        <Obsolete("This property is depreciated. Use Movie.Genres [List(Of String)] instead.")> _
        <XmlIgnore()> _
        Public Property Genre() As String
            Get
                Return String.Join(" / ", _genres.ToArray)
            End Get
            Set(ByVal value As String)
                _genres.Clear()
                AddGenre(value)
            End Set
        End Property

        <XmlElement("studio")> _
        Public Property Studio() As String
            Get
                Return Me._studio
            End Get
            Set(ByVal value As String)
                Me._studio = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property StudioSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._studio)
            End Get
        End Property

        <XmlElement("director")> _
        Public Property Directors() As List(Of String)
            Get
                Return _directors
            End Get
            Set(ByVal value As List(Of String))
                If IsNothing(value) Then
                    _directors.Clear()
                Else
                    _directors = value
                End If
            End Set
        End Property

        <Obsolete("This property is depreciated. Use Movie.Directors [List(Of String)] instead.")> _
        <XmlIgnore()> _
        Public Property Director() As String
            Get
                Return String.Join(" / ", _directors.ToArray)
            End Get
            Set(ByVal value As String)
                _directors.Clear()
                AddDirector(value)
            End Set
        End Property

        <Obsolete("This property is depreciated. Use 'Movie.Directors.Count > 0' instead.")> _
        <XmlIgnore()> _
        Public ReadOnly Property DirectorSpecified() As Boolean
            Get
                Return (_directors.Count > 0)
            End Get
        End Property

        <XmlElement("credits")> _
        Public Property Credits() As List(Of String)
            Get
                Return _credits
            End Get
            Set(ByVal value As List(Of String))
                If IsNothing(value) Then
                    _credits.Clear()
                Else
                    _credits = value
                End If
            End Set
        End Property

        <Obsolete("This property is depreciated. Use Movie.Genres [List(Of String)] instead.")> _
        <XmlIgnore()> _
        Public Property OldCredits() As String
            Get
                Return String.Join(" / ", _credits.ToArray)
            End Get
            Set(ByVal value As String)
                _credits.Clear()
                AddCredit(value)
            End Set
        End Property

        <Obsolete("This property is depreciated. Use 'Movie.Credits.Count > 0' instead.")> _
        <XmlIgnore()> _
        Public ReadOnly Property CreditsSpecified() As Boolean
            Get
                Return (_credits.Count > 0)
            End Get
        End Property

        <XmlElement("tagline")> _
        Public Property Tagline() As String
            Get
                Return Me._tagline
            End Get
            Set(ByVal value As String)
                Me._tagline = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property TaglineSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._tagline)
            End Get
        End Property

        <XmlElement("outline")> _
        Public Property Outline() As String
            Get
                Return Me._outline
            End Get
            Set(ByVal value As String)
                Me._outline = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property OutlineSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._outline)
            End Get
        End Property

        <XmlElement("plot")> _
        Public Property Plot() As String
            Get
                Return Me._plot
            End Get
            Set(ByVal value As String)
                Me._plot = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property PlotSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._plot)
            End Get
        End Property

        <XmlElement("runtime")> _
        Public Property Runtime() As String
            Get
                Return Me._runtime
            End Get
            Set(ByVal value As String)
                Me._runtime = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property RuntimeSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._runtime)
            End Get
        End Property

        <XmlElement("trailer")> _
        Public Property Trailer() As String
            Get
                Return Me._trailer
            End Get
            Set(ByVal value As String)
                Me._trailer = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property TrailerSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._trailer)
            End Get
        End Property

        <XmlElement("playcount")> _
        Public Property PlayCount() As String
            Get
                Return Me._playcount
            End Get
            Set(ByVal value As String)
                Me._playcount = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property PlayCountSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._playcount)
            End Get
        End Property

        <XmlElement("watched")> _
        Public Property Watched() As String
            Get
                Return Me._watched
            End Get
            Set(ByVal value As String)
                Me._watched = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property WatchedSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._watched)
            End Get
        End Property

        <XmlElement("actor")> _
        Public Property Actors() As List(Of Person)
            Get
                Return Me._actors
            End Get
            Set(ByVal Value As List(Of Person))
                Me._actors = Value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property ActorsSpecified() As Boolean
            Get
                Return Me._actors.Count > 0
            End Get
        End Property

        <XmlElement("thumb")> _
        Public Property Thumb() As List(Of String)
            Get
                Return Me._thumb
            End Get
            Set(ByVal value As List(Of String))
                Me._thumb = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property ThumbSpecified() As Boolean
            Get
                Return Me._thumb.Count > 0
            End Get
        End Property

        <XmlElement("fanart")> _
        Public Property Fanart() As Fanart
            Get
                Return Me._fanart
            End Get
            Set(ByVal value As Fanart)
                Me._fanart = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property FanartSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._fanart.URL)
            End Get
        End Property

        <XmlIgnore()> _
        Public Property Sets() As List(Of [Set])
            Get
                Return If(Master.eSettings.YAMJSetsCompatible, Me._ysets.Sets, Me._xsets)
            End Get
            Set(ByVal value As List(Of [Set]))
                If Master.eSettings.YAMJSetsCompatible Then
                    Me._ysets.Sets = value
                Else
                    Me._xsets = value
                End If
            End Set
        End Property

        <XmlElement("set")> _
        Public Property XSets() As List(Of [Set])
            Get
                Return Me._xsets
            End Get
            Set(ByVal value As List(Of [Set]))
                _xsets = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property XSetsSpecified() As Boolean
            Get
                Return Me._xsets.Count > 0
            End Get
        End Property

        <XmlElement("sets")> _
        Public Property YSets() As SetContainer
            Get
                Return _ysets
            End Get
            Set(ByVal value As SetContainer)
                _ysets = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property YSetsSpecified() As Boolean
            Get
                Return _ysets.Sets.Count > 0
            End Get
        End Property

        <XmlElement("fileinfo")> _
        Public Property FileInfo() As MediaInfo.Fileinfo
            Get
                Return Me._fileInfo
            End Get
            Set(ByVal value As MediaInfo.Fileinfo)
                Me._fileInfo = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property FileInfoSpecified() As Boolean
            Get
                If Not IsNothing(Me._fileInfo.StreamDetails.Video) OrElse _
                Me._fileInfo.StreamDetails.Audio.Count > 0 OrElse _
                 Me._fileInfo.StreamDetails.Subtitle.Count > 0 Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        <XmlIgnore()> _
        Public Property Lev() As Integer
            Get
                Return Me._lev
            End Get
            Set(ByVal value As Integer)
                Me._lev = value
            End Set
        End Property

        <XmlElement("videoSource")> _
        Public Property VideoSource() As String
            Get
                Return Me._videosource
            End Get
            Set(ByVal value As String)
                Me._videosource = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property VideoSourceSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._videosource)
            End Get
        End Property

        Class _MovieID
            Private _imdbid As String
            Private _moviedb As String
            Sub New()
                Me.Clear()
            End Sub
            Public Sub Clear()
                _imdbid = String.Empty
                _moviedb = String.Empty
            End Sub
            <XmlText()> _
            Public Property ID() As String
                Get
                    Return If(Strings.Left(_imdbid, 2) = "tt", If(Not String.IsNullOrEmpty(_moviedb) AndAlso _imdbid.Trim = "tt-1", _imdbid.Replace("tt", String.Empty), _imdbid.Trim), If(Not _imdbid.Trim = "tt-1", String.Concat("tt", _imdbid), _imdbid))
                End Get
                Set(ByVal value As String)
                    _imdbid = If(Strings.Left(value, 2) = "tt", value.Trim, String.Concat("tt", value))
                End Set
            End Property
            <XmlIgnore()> _
            Public ReadOnly Property IDSpecified() As Boolean
                Get
                    Return Not String.IsNullOrEmpty(_imdbid) AndAlso Not _imdbid = "tt"
                End Get
            End Property
            <XmlAttribute("moviedb")> _
            Public Property IDMovieDB() As String
                Get
                    Return _moviedb
                End Get
                Set(ByVal value As String)
                    Me._moviedb = value
                End Set
            End Property
            <XmlIgnore()> _
            Public ReadOnly Property IDMovieDBSpecified() As Boolean
                Get
                    Return Not String.IsNullOrEmpty(Me._moviedb)
                End Get
            End Property
        End Class

#End Region 'Properties

#Region "Methods"

        Public Sub AddSet(ByVal SetName As String, ByVal Order As Integer)
            Dim tSet = From bSet As [Set] In Sets Where bSet.Set = SetName

            If tSet.Count > 0 Then
                Sets.Remove(tSet(0))
            End If

            Sets.Add(New [Set] With {.Set = SetName, .Order = If(Order > 0, Order.ToString, String.Empty)})
        End Sub

        Public Sub AddGenre(ByVal value As String)
            If String.IsNullOrEmpty(value) Then Return

            If value.Contains("/") Then
                Dim values As String() = value.Split(New [Char]() {"/"c})
                For Each genre As String In values
                    genre = genre.Trim
                    If Not _genres.Contains(genre) Then
                        _genres.Add(genre)
                    End If
                Next
            Else
                If Not _genres.Contains(value) Then
                    _genres.Add(value.Trim)
                End If
            End If
        End Sub

        Public Sub AddDirector(ByVal value As String)
            If String.IsNullOrEmpty(value) Then Return

            If value.Contains("/") Then
                Dim values As String() = value.Split(New [Char]() {"/"c})
                For Each director As String In values
                    director = director.Trim
                    If Not _directors.Contains(director) And Not value = "See more" Then
                        _directors.Add(director)
                    End If
                Next
            Else
                value = value.Trim
                If Not _directors.Contains(value) And Not value = "See more" Then
                    _directors.Add(value.Trim)
                End If
            End If
        End Sub

        Public Sub AddCredit(ByVal value As String)
            If String.IsNullOrEmpty(value) Then Return

            If value.Contains("/") Then
                Dim values As String() = value.Split(New [Char]() {"/"c})
                For Each credit As String In values
                    credit = credit.Trim
                    If Not _credits.Contains(credit) And Not value = "See more" Then
                        _credits.Add(credit)
                    End If
                Next
            Else
                value = value.Trim
                If Not _credits.Contains(value) And Not value = "See more" Then
                    _credits.Add(value.Trim)
                End If
            End If
        End Sub

        Public Sub AddCountry(ByVal value As String)
            If String.IsNullOrEmpty(value) Then Return

            If value.Contains("/") Then
                Dim values As String() = value.Split(New [Char]() {"/"c})
                For Each country As String In values
                    country = country.Trim
                    If Not _countries.Contains(country) Then
                        _countries.Add(country)
                    End If
                Next
            Else
                value = value.Trim
                If Not _countries.Contains(value) Then
                    _countries.Add(value.Trim)
                End If
            End If
        End Sub


        Public Sub Clear()
            'Me._imdbid = String.Empty
            Me._title = String.Empty
            Me._originaltitle = String.Empty
            Me._sorttitle = String.Empty
            Me._year = String.Empty
            Me._rating = String.Empty
            Me._votes = String.Empty
            Me._mpaa = String.Empty
            Me._top250 = String.Empty
            Me._countries.Clear()
            Me._outline = String.Empty
            Me._plot = String.Empty
            Me._tagline = String.Empty
            Me._trailer = String.Empty
            Me._certification = String.Empty
            Me._genres.Clear()
            Me._runtime = String.Empty
            Me._releaseDate = String.Empty
            Me._studio = String.Empty
            Me._directors.Clear()
            Me._credits.Clear()
            Me._playcount = String.Empty
            Me._watched = String.Empty
            Me._thumb.Clear()
            Me._fanart = New Fanart
            Me._actors.Clear()
            Me._fileInfo = New MediaInfo.Fileinfo
            Me._ysets = New SetContainer
            Me._xsets.Clear()
            Me._lev = 0
            Me._videosource = String.Empty
            Me.MovieID.Clear()
        End Sub

        Public Function CompareTo(ByVal other As Movie) As Integer Implements IComparable(Of Movie).CompareTo
            Dim retVal As Integer = (Me.Lev).CompareTo(other.Lev)
            If retVal = 0 Then
                retVal = (Me.Year).CompareTo(other.Year) * -1
            End If
            Return retVal
        End Function

        Public Sub RemoveSet(ByVal SetName As String)
            Dim tSet = From bSet As [Set] In Sets Where bSet.Set = SetName
            If tSet.Count > 0 Then
                Sets.Remove(tSet(0))
            End If
        End Sub

#End Region 'Methods
    End Class

    Public Class Person

#Region "Fields"

        Private _name As String
        Private _role As String
        Private _thumb As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New(ByVal sName As String)
            Me._name = sName
        End Sub

        Public Sub New(ByVal sName As String, ByVal sRole As String, ByVal sThumb As String)
            Me._name = sName
            Me._role = sRole
            Me._thumb = sThumb
        End Sub

        Public Sub New()
            Me.Clean()
        End Sub

#End Region 'Constructors

#Region "Properties"

        <XmlElement("name")> _
        Public Property Name() As String
            Get
                Return Me._name
            End Get
            Set(ByVal Value As String)
                Me._name = Value
            End Set
        End Property

        <XmlElement("role")> _
        Public Property Role() As String
            Get
                Return Me._role
            End Get
            Set(ByVal Value As String)
                Me._role = Value
            End Set
        End Property

        <XmlElement("thumb")> _
        Public Property Thumb() As String
            Get
                Return Me._thumb
            End Get
            Set(ByVal Value As String)
                Me._thumb = Value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clean()
            Me._name = String.Empty
            Me._role = String.Empty
            Me._thumb = String.Empty
        End Sub

        Public Overrides Function ToString() As String
            Return Me._name
        End Function

#End Region 'Methods

    End Class

    Public Class SetContainer

#Region "Fields"

        Private _set As New List(Of [Set])

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        <XmlElement("set")> _
        Public Property Sets() As List(Of [Set])
            Get
                Return _set
            End Get
            Set(ByVal value As List(Of [Set]))
                _set = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property SetsSpecified() As Boolean
            Get
                Return Me._set.Count > 0
            End Get
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._set = New List(Of [Set])
        End Sub

#End Region 'Methods

    End Class

    Public Class Thumb

#Region "Fields"

        Private _preview As String
        Private _text As String

#End Region 'Fields

#Region "Properties"

        <XmlAttribute("preview")> _
        Public Property Preview() As String
            Get
                Return Me._preview
            End Get
            Set(ByVal Value As String)
                Me._preview = Value
            End Set
        End Property

        <XmlText()> _
        Public Property [Text]() As String
            Get
                Return Me._text
            End Get
            Set(ByVal Value As String)
                Me._text = Value
            End Set
        End Property

#End Region 'Properties

    End Class

    <XmlRoot("tvshow")> _
    Public Class TVShow

#Region "Fields"

        Private _title As String
        Private _id As String
        Private _episodeguideurl As String
        Private _rating As String
        Private _genres As New List(Of String)
        Private _mpaa As String
        Private _premiered As String
        Private _studio As String
        Private _plot As String
        Private _actors As New List(Of Person)
        Private _boxeeTvDb As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        <XmlElement("title")> _
        Public Property Title() As String
            Get
                Return Me._title
            End Get
            Set(ByVal value As String)
                Me._title = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property TitleSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._title)
            End Get
        End Property

        <XmlElement("id")> _
        Public Property ID() As String
            Get
                Return Me._id
            End Get
            Set(ByVal value As String)
                If IsNumeric(value) Then Me._id = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property IDSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._id)
            End Get
        End Property

        <XmlElement("boxeeTvDb")> _
        Public Property BoxeeTvDb() As String
            Get
                Return Me._boxeeTvDb
            End Get
            Set(ByVal value As String)
                If IsNumeric(value) Then Me._boxeeTvDb = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property BoxeeIDSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._boxeeTvDb)
            End Get
        End Property

        <XmlElement("episodeguideurl")> _
        Public Property EpisodeGuideURL() As String
            Get
                Return Me._episodeguideurl
            End Get
            Set(ByVal value As String)
                Me._episodeguideurl = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property EpisodeGuideURLSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._episodeguideurl)
            End Get
        End Property

        <XmlElement("rating")> _
        Public Property Rating() As String
            Get
                Return Me._rating.Replace(",", ".")
            End Get
            Set(ByVal value As String)
                Me._rating = value.Replace(",", ".")
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property RatingSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._rating)
            End Get
        End Property

        <XmlElement("genre")> _
        Public Property Genres() As List(Of String)
            Get
                Return _genres
            End Get
            Set(ByVal value As List(Of String))
                If IsNothing(value) Then
                    _genres.Clear()
                Else
                    _genres = value
                End If
            End Set
        End Property

        <Obsolete("This property is depreciated. Use TVShow.Genres.Count instead.")> _
        <XmlIgnore()> _
        Public Property LGenre() As List(Of String)
            Get
                Return Genres
            End Get
            Set(ByVal value As List(Of String))
                Genres = value
            End Set
        End Property

        <Obsolete("This property is depreciated. Use 'TVShow.Genres.Count > 0' instead.")> _
        <XmlIgnore()> _
        Public ReadOnly Property LGenreSpecified() As Boolean
            Get
                Return (_genres.Count > 0)
            End Get
        End Property

        <Obsolete("This property is depreciated. Use TVShow.Genres [List(Of String)] instead.")> _
        <XmlIgnore()> _
        Public Property Genre() As String
            Get
                Return String.Join(" / ", _genres.ToArray)
            End Get
            Set(ByVal value As String)
                _genres.Clear()
                AddGenre(value)
            End Set
        End Property

        <XmlElement("mpaa")> _
        Public Property MPAA() As String
            Get
                Return Me._mpaa
            End Get
            Set(ByVal value As String)
                Me._mpaa = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property MPAASpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._mpaa)
            End Get
        End Property

        <XmlElement("premiered")> _
        Public Property Premiered() As String
            Get
                Return Me._premiered
            End Get
            Set(ByVal value As String)
                Me._premiered = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property PremieredSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._premiered)
            End Get
        End Property

        <XmlElement("studio")> _
        Public Property Studio() As String
            Get
                Return Me._studio
            End Get
            Set(ByVal value As String)
                Me._studio = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property StudioSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._studio)
            End Get
        End Property

        <XmlElement("plot")> _
        Public Property Plot() As String
            Get
                Return Me._plot
            End Get
            Set(ByVal value As String)
                Me._plot = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property PlotSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._plot)
            End Get
        End Property

        <XmlElement("actor")> _
        Public Property Actors() As List(Of Person)
            Get
                Return Me._actors
            End Get
            Set(ByVal Value As List(Of Person))
                Me._actors = Value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property ActorsSpecified() As Boolean
            Get
                Return Me._actors.Count > 0
            End Get
        End Property

        <XmlIgnore()> _
        Public Property TVDBID() As String
            Get
                Return Me._id
            End Get
            Set(ByVal value As String)
                Me._id = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub AddGenre(ByVal value As String)
            If String.IsNullOrEmpty(value) Then Return

            If value.Contains("/") Then
                Dim values As String() = value.Split(New [Char]() {"/"c})
                For Each genre As String In values
                    genre = genre.Trim
                    If Not _genres.Contains(genre) Then
                        _genres.Add(genre)
                    End If
                Next
            Else
                If Not _genres.Contains(value) Then
                    _genres.Add(value.Trim)
                End If
            End If
        End Sub

        Public Sub Clear()
            _title = String.Empty
            _id = String.Empty
            _boxeeTvDb = String.Empty
            _rating = String.Empty
            _episodeguideurl = String.Empty
            _plot = String.Empty
            _mpaa = String.Empty
            _genres.Clear()
            _premiered = String.Empty
            _studio = String.Empty
            _actors.Clear()
        End Sub

        Public Sub BlankId()
            Me._id = Nothing
        End Sub

        Public Sub BlankBoxeeId()
            Me._boxeeTvDb = Nothing
        End Sub

#End Region 'Methods

    End Class

    Public Class [Image]

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"
        Public Property Width As String        
        Public Property Height As String            
        Public Property Description As String            
        Public Property isChecked As Boolean
        Public Property URL As String
        Public Property WebImage As Images
        Public Property ParentID As String
#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._url = String.Empty
            Me._description = String.Empty
            Me._webimage = New Images
            Me._ischecked = False
        End Sub

#End Region 'Methods

    End Class

    Public Class [Set]

#Region "Fields"

        Private _order As String
        Private _set As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        <XmlAttribute("order")> _
        Public Property Order() As String
            Get
                Return _order
            End Get
            Set(ByVal value As String)
                _order = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property OrderSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._order)
            End Get
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property SetSpecified() As Boolean
            Get
                Return Not String.IsNullOrEmpty(Me._set)
            End Get
        End Property

        <XmlText()> _
        Public Property [Set]() As String
            Get
                Return _set
            End Get
            Set(ByVal value As String)
                _set = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            _set = String.Empty
            _order = String.Empty
        End Sub

#End Region 'Methods

    End Class

End Namespace

