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

Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Xml.Linq

Public Class Containers

#Region "Nested Types"


    <XmlRoot("CommandFile")> _
    Public Class InstallCommands

#Region "Fields"

        <XmlArray("Commands")> _
        <XmlArrayItem("Command")> _
        Public Command As New List(Of InstallCommand)

#End Region 'Fields

#Region "Methods"

        Public Sub Save(ByVal fpath As String)
            Dim xmlSer As New XmlSerializer(GetType(InstallCommands))
            Using xmlSW As New StreamWriter(fpath)
                xmlSer.Serialize(xmlSW, Me)
            End Using
        End Sub

        Public Shared Function Load(ByVal fpath As String) As Containers.InstallCommands
            If Not File.Exists(fpath) Then Return New Containers.InstallCommands
            Dim xmlSer As XmlSerializer
            xmlSer = New XmlSerializer(GetType(Containers.InstallCommands))
            Using xmlSW As New StreamReader(Path.Combine(Functions.AppPath, fpath))
                Return DirectCast(xmlSer.Deserialize(xmlSW), Containers.InstallCommands)
            End Using
        End Function
#End Region 'Methods

    End Class

    Public Class InstallCommand

#Region "Fields"

        <XmlElement("Description")> _
        Public CommandDescription As String
        <XmlElement("Execute")> _
        Public CommandExecute As String
        <XmlAttribute("Type")> _
        Public CommandType As String

#End Region 'Fields

    End Class



    Public Class ImgResult

#Region "Fields"

        Dim _fanart As New MediaContainers.Fanart
        Dim _imagepath As String
        Dim _posters As New List(Of String)

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property Fanart() As MediaContainers.Fanart
            Get
                Return _fanart
            End Get
            Set(ByVal value As MediaContainers.Fanart)
                _fanart = value
            End Set
        End Property

        Public Property ImagePath() As String
            Get
                Return _imagepath
            End Get
            Set(ByVal value As String)
                _imagepath = value
            End Set
        End Property

        Public Property Posters() As List(Of String)
            Get
                Return _posters
            End Get
            Set(ByVal value As List(Of String))
                _posters = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            _imagepath = String.Empty
            _posters.Clear()
            _fanart.Clear()
        End Sub

#End Region 'Methods

    End Class

    Public Class SettingsPanel

#Region "Fields"

        Dim _imageindex As Integer
        Dim _image As Image
        Dim _name As String
        Dim _order As Integer
        Dim _panel As Panel
        Dim _parent As String
        Dim _prefix As String
        Dim _text As String
        Dim _type As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property ImageIndex() As Integer
            Get
                Return Me._imageindex
            End Get
            Set(ByVal value As Integer)
                Me._imageindex = value
            End Set
        End Property

        <System.Xml.Serialization.XmlIgnore()> _
        Public Property Image() As Image
            Get
                Return Me._image
            End Get
            Set(ByVal value As Image)
                Me._image = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return Me._name
            End Get
            Set(ByVal value As String)
                Me._name = value
            End Set
        End Property

        Public Property Order() As Integer
            Get
                Return Me._order
            End Get
            Set(ByVal value As Integer)
                Me._order = value
            End Set
        End Property

        <System.Xml.Serialization.XmlIgnore()> _
        Public Property Panel() As Panel
            Get
                Return Me._panel
            End Get
            Set(ByVal value As Panel)
                Me._panel = value
            End Set
        End Property

        Public Property Parent() As String
            Get
                Return Me._parent
            End Get
            Set(ByVal value As String)
                Me._parent = value
            End Set
        End Property

        Public Property Prefix() As String
            Get
                Return Me._prefix
            End Get
            Set(ByVal value As String)
                Me._prefix = value
            End Set
        End Property

        Public Property Text() As String
            Get
                Return Me._text
            End Get
            Set(ByVal value As String)
                Me._text = value
            End Set
        End Property

        Public Property Type() As String
            Get
                Return Me._type
            End Get
            Set(ByVal value As String)
                Me._type = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._name = String.Empty
            Me._text = String.Empty
            Me._prefix = String.Empty
            Me._imageindex = 0
            Me._image = Nothing
            Me._type = String.Empty
            Me._panel = New Panel
            Me._order = 0
            Me._parent = String.Empty
        End Sub

#End Region 'Methods

    End Class

    Public Class TVLanguage

#Region "Fields"

        Private _longlang As String
        Private _shortlang As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property LongLang() As String
            Get
                Return Me._longlang
            End Get
            Set(ByVal value As String)
                Me._longlang = value
            End Set
        End Property

        Public Property ShortLang() As String
            Get
                Return Me._shortlang
            End Get
            Set(ByVal value As String)
                Me._shortlang = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._longlang = String.Empty
            Me._shortlang = String.Empty
        End Sub

#End Region 'Methods

    End Class

    Public Class Addon
        Private _id As Integer
        Private _name As String
        Private _author As String
        Private _description As String
        Private _category As String
        Private _version As Single
        Private _mineversion As Single
        Private _maxeversion As Single
        Private _screenshotpath As String
        Private _screenshotimage As Image
        Private _files As Generic.SortedList(Of String, String)
        Private _deletefiles As List(Of String)

        Public Property ID() As Integer
            Get
                Return Me._id
            End Get
            Set(ByVal value As Integer)
                Me._id = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return Me._name
            End Get
            Set(ByVal value As String)
                Me._name = value
            End Set
        End Property

        Public Property Author() As String
            Get
                Return Me._author
            End Get
            Set(ByVal value As String)
                Me._author = value
            End Set
        End Property

        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(ByVal value As String)
                Me._description = value
            End Set
        End Property

        Public Property Category() As String
            Get
                Return Me._category
            End Get
            Set(ByVal value As String)
                Me._category = value
            End Set
        End Property

        Public Property Version() As Single
            Get
                Return Me._version
            End Get
            Set(ByVal value As Single)
                Me._version = value
            End Set
        End Property

        Public Property MinEVersion() As Single
            Get
                Return Me._mineversion
            End Get
            Set(ByVal value As Single)
                Me._mineversion = value
            End Set
        End Property

        Public Property MaxEVersion() As Single
            Get
                Return Me._maxeversion
            End Get
            Set(ByVal value As Single)
                Me._maxeversion = value
            End Set
        End Property

        Public Property ScreenShotPath() As String
            Get
                Return Me._screenshotpath
            End Get
            Set(ByVal value As String)
                Me._screenshotpath = value
            End Set
        End Property

        Public Property ScreenShotImage() As Image
            Get
                Return Me._screenshotimage
            End Get
            Set(ByVal value As Image)
                Me._screenshotimage = value
            End Set
        End Property

        Public Property Files() As Generic.SortedList(Of String, String)
            Get
                Return Me._files
            End Get
            Set(ByVal value As Generic.SortedList(Of String, String))
                Me._files = value
            End Set
        End Property

        Public Property DeleteFiles() As List(Of String)
            Get
                Return Me._deletefiles
            End Get
            Set(ByVal value As List(Of String))
                Me._deletefiles = value
            End Set
        End Property

        Public Sub New()
            Me.Clear()
        End Sub

        Public Sub Clear()
            Me._id = -1
            Me._name = String.Empty
            Me._author = String.Empty
            Me._description = String.Empty
            Me._category = String.Empty
            Me._version = -1
            Me._mineversion = -1
            Me._maxeversion = -1
            Me._screenshotpath = String.Empty
            Me._screenshotimage = Nothing
            Me._files = New Generic.SortedList(Of String, String)
            Me._deletefiles = New List(Of String)
        End Sub
    End Class

#End Region 'Nested Types

End Class

Public Class Enums

#Region "Enumerations"

    Public Enum DefaultType As Integer
        All = 0
        MovieFilters = 1
        ShowFilters = 2
        EpFilters = 3
        ValidExts = 4
        ShowRegex = 5
    End Enum

    Public Enum DelType As Integer
        Movies = 0
        Shows = 1
        Seasons = 2
        Episodes = 3
    End Enum

    Public Enum FanartSize As Integer
        Lrg = 0
        Mid = 1
        Small = 2
    End Enum

    Public Enum ModType As Integer
        NFO = 0
        Poster = 1
        Fanart = 2
        Extra = 3
        Trailer = 4
        Meta = 5
        All = 6
        DoSearch = 7
        Actor = 8
    End Enum
    Public Enum PostScraperCapabilities
        Poster = 1
        Fanart = 2
        Trailer = 3
    End Enum


    Public Enum ModuleEventType As Integer
        Generic = 0
        Notification = 1
        MovieScraperRDYtoSave = 2       ' Called when scraper finishs but before save
        RenameMovie = 3                 ' Called when need to rename a Movie ... from several places
        RenameMovieManual = 4           ' Will call only First Register Module (use Master.currMovie)
        MovieFrameExtrator = 5
        TVFrameExtrator = 6
        RandomFrameExtrator = 7
        CommandLine = 8            ' Command Line Module Call
        MovieSync = 9
        ShowMovie = 10                  ' Called after displaying Movie  (not in place yet)
        ShowTVShow = 11                 ' Called after displaying TVShow (not in place yet)
        BeforeEditMovie = 12            ' Called when Manual editing or reading from nfo
        OnMovieNFOSave = 13
        OnMoviePosterSave = 14
        OnMovieFanartSave = 15
        OnMoviePosterDelete = 16
        OnMovieFanartDelete = 17
        TVImageNaming = 18
        MovieImageNaming = 19
        SyncModuleSettings = 20
        OnTVShowNFOSave = 21
        OnTVShowNFORead = 22
    End Enum

    Public Enum MovieScraperEventType As Integer
        NFOItem = 1
        PosterItem = 2
        FanartItem = 3
        TrailerItem = 4
        ThumbsItem = 5
        SortTitle = 6
        ListTitle = 7
    End Enum

    Public Enum Ordering As Integer
        Standard = 0
        DVD = 1
        Absolute = 2
    End Enum

    Public Enum PosterSize As Integer
        Xlrg = 0
        Lrg = 1
        Mid = 2
        Small = 3
        Wide = 4
    End Enum

    Public Enum ScrapeType As Integer
        SingleScrape = 0
        FullAuto = 1
        FullAsk = 2
        UpdateAuto = 3
        UpdateAsk = 4
        CleanFolders = 6
        NewAuto = 7
        NewAsk = 8
        MarkAuto = 9
        MarkAsk = 10
        FilterAuto = 11
        FilterAsk = 12
        CopyBD = 13
        FullSkip = 14
        None = 99 ' 
    End Enum

    Public Enum SeasonPosterType As Integer
        None = 0
        Poster = 1
        Wide = 2
    End Enum

    Public Enum ShowBannerType As Integer
        None = 0
        Blank = 1
        Graphical = 2
        Text = 3
    End Enum

    Public Enum TrailerQuality As Integer
        HD1080p = 0
        HD1080pVP8 = 1
        HD720p = 2
        HD720pVP8 = 3
        SQMP4 = 4
        HQFLV = 5
        HQVP8 = 6
        SQFLV = 7
        SQVP8 = 8
        OTHERS = 9
    End Enum

    Public Enum ImageType As Integer
        Posters = 0
        Fanart = 1
        ASPoster = 2
    End Enum

    Public Enum TVImageType As Integer
        All = 0
        ShowPoster = 1
        ShowFanart = 2
        SeasonPoster = 3
        SeasonFanart = 4
        AllSeasonPoster = 5
        EpisodePoster = 6
        EpisodeFanart = 7
    End Enum

    Public Enum TVScraperEventType As Integer
        Progress = 0
        SearchResultsDownloaded = 1
        StartingDownload = 2
        ShowDownloaded = 3
        SavingStarted = 4
        ScraperDone = 5
        LoadingEpisodes = 6
        Searching = 7
        SelectImages = 8
        Verifying = 9
        Cancelled = 10
        SaveAuto = 11
    End Enum

    Public Enum TVUpdateTime As Integer
        Week = 0
        BiWeekly = 1
        Month = 2
        Never = 3
        Always = 4
    End Enum

#End Region 'Enumerations

End Class

Public Class Functions

#Region "Methods"

    ''' <summary>
    ''' Force of habit
    ''' </summary>
    ''' <returns>Path of the directory containing the Ember executable</returns>
    Public Shared Function AppPath() As String
        Return System.AppDomain.CurrentDomain.BaseDirectory
    End Function

    Public Shared Function Check64Bit() As Boolean
        Return (IntPtr.Size = 8)
    End Function

    Public Shared Function CheckIfWindows() As Boolean
        Return Environment.OSVersion.ToString.ToLower.IndexOf("windows") > 0
    End Function

    Public Shared Function IsBetaEnabled() As Boolean
        If File.Exists(Path.Combine(AppPath, "Beta.Tester")) Then
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Check for the lastest version of Ember
    ''' </summary>
    ''' <returns>Latest version as integer</returns>
    Public Shared Function CheckNeedUpdate() As Boolean
        Dim sHTTP As New HTTP
        Dim needUpdate As Boolean = False
        Dim platform As String = "x86"
        Dim updateXML As String = sHTTP.DownloadData(String.Format("http://pcjco.dommel.be/emm-r/{0}/versions.xml", If(IsBetaEnabled(), "updatesbeta", "updates")))
        sHTTP = Nothing
        If updateXML.Length > 0 Then
            For Each v As ModulesManager.VersionItem In ModulesManager.VersionList
                Dim vl As ModulesManager.VersionItem = v
                Dim n As String = String.Empty
                Dim xmlUpdate As XDocument
                Try
                    xmlUpdate = XDocument.Parse(updateXML)
                Catch
                    Return False
                End Try
                Dim xUdpate = From xUp In xmlUpdate...<Config>...<Modules>...<File> Where (xUp.<Version>.Value <> "" AndAlso xUp.<Name>.Value = vl.AssemblyFileName AndAlso xUp.<Platform>.Value = platform) Select xUp.<Version>.Value
                Try
                    If Convert.ToInt16(xUdpate(0)) > Convert.ToInt16(v.Version) Then
                        v.NeedUpdate = True
                        needUpdate = True
                    End If
                Catch ex As Exception
                End Try
            Next
        End If
        Return needUpdate
    End Function

    Public Shared Function ConvertFromUnixTimestamp(ByVal timestamp As Double) As DateTime
        Dim origin As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
        Return origin.AddSeconds(timestamp)
    End Function

    Public Shared Function ConvertToUnixTimestamp(ByVal data As DateTime) As Double
        Dim origin As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
        Dim diff As System.TimeSpan = data - origin
        Return Math.Floor(diff.TotalSeconds)
    End Function

    Public Shared Function LocksToOptions() As Structures.ScrapeOptions
        Dim options As New Structures.ScrapeOptions
        With options
            .bCast = True
            .bDirector = True
            .bGenre = True
            .bMPAA = True
            .bCert = True
            .bMusicBy = True
            .bOtherCrew = True
            .bOutline = Not Master.eSettings.LockOutline
            .bPlot = Not Master.eSettings.LockPlot
            .bProducers = True
            .bRating = Not Master.eSettings.LockRating
            .bRelease = True
            .bRuntime = True
            .bStudio = Not Master.eSettings.LockStudio
            .bTagline = Not Master.eSettings.LockTagline
            .bTitle = Not Master.eSettings.LockTitle
            .bTrailer = Not Master.eSettings.LockTrailer
            .bVotes = True
            .bWriters = True
            .bYear = True
            .bTop250 = True
            .bCountry = True
            .bFullCrew = True
            .bFullCast = True
        End With
        Return options
    End Function

    Public Shared Sub CreateDefaultOptions()
        With Master.DefaultOptions
            .bCast = Master.eSettings.FieldCast
            .bDirector = Master.eSettings.FieldDirector
            .bGenre = Master.eSettings.FieldGenre
            .bMPAA = Master.eSettings.FieldMPAA
            .bCert = Master.eSettings.FieldCert
            .bMusicBy = Master.eSettings.FieldMusic
            .bOtherCrew = Master.eSettings.FieldCrew
            .bOutline = Master.eSettings.FieldOutline
            .bPlot = Master.eSettings.FieldPlot
            .bProducers = Master.eSettings.FieldProducers
            .bRating = Master.eSettings.FieldRating
            .bRelease = Master.eSettings.FieldRelease
            .bRuntime = Master.eSettings.FieldRuntime
            .bStudio = Master.eSettings.FieldStudio
            .bTagline = Master.eSettings.FieldTagline
            .bTitle = Master.eSettings.FieldTitle
            .bTrailer = Master.eSettings.FieldTrailer
            .bVotes = Master.eSettings.FieldVotes
            .bWriters = Master.eSettings.FieldWriters
            .bYear = Master.eSettings.FieldYear
            .bTop250 = Master.eSettings.Field250
            .bCountry = Master.eSettings.FieldCountry
            ' Why this 2 arent here?
            .bFullCrew = Master.eSettings.FullCrew
            .bFullCast = Master.eSettings.FullCast
        End With

        With Master.DefaultTVOptions
            .bShowTitle = Master.eSettings.ScraperShowTitle
            .bShowEpisodeGuide = Master.eSettings.ScraperShowEGU
            .bShowGenre = Master.eSettings.ScraperShowGenre
            .bShowMPAA = Master.eSettings.ScraperShowMPAA
            .bShowPlot = Master.eSettings.ScraperShowPlot
            .bShowPremiered = Master.eSettings.ScraperShowPremiered
            .bShowRating = Master.eSettings.ScraperShowRating
            .bShowStudio = Master.eSettings.ScraperShowStudio
            .bShowActors = Master.eSettings.ScraperShowActors
            .bEpTitle = Master.eSettings.ScraperEpTitle
            .bEpSeason = Master.eSettings.ScraperEpSeason
            .bEpEpisode = Master.eSettings.ScraperEpEpisode
            .bEpAired = Master.eSettings.ScraperEpAired
            .bEpRating = Master.eSettings.ScraperEpRating
            .bEpPlot = Master.eSettings.ScraperEpPlot
            .bEpDirector = Master.eSettings.ScraperEpDirector
            .bEpCredits = Master.eSettings.ScraperEpCredits
            .bEpActors = Master.eSettings.ScraperEpActors
        End With
    End Sub

    Public Shared Sub DGVDoubleBuffer(ByRef cDGV As DataGridView)
        Dim conType As Type = cDGV.GetType
        Dim pi As System.Reflection.PropertyInfo = conType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)
        pi.SetValue(cDGV, True, Nothing)
    End Sub

    Public Shared Function EmberAPIVersion() As String
        Return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion
    End Function

    ''' <summary>
    ''' Get the changelog for the latest version
    ''' </summary>
    ''' <returns>Changelog as string</returns>
    Public Shared Function GetChangelog() As String
        'Try
        '    Dim sHTTP As New HTTP
        '    Dim strChangelog As String = sHTTP.DownloadData(String.Format("http://pcjco.dommel.be/emm-r/{0}/WhatsNew.txt", If(IsBetaEnabled(), "updatesbeta", "updates")))
        '    sHTTP = Nothing

        '    If strChangelog.Length > 0 Then
        '        Return strChangelog
        '    End If
        'Catch ex As Exception
        '    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        'End Try
        Return "Unavailable"
    End Function

    ''' <summary>
    ''' Get the number of the last sequential extrathumb to make sure we're not overwriting current ones.
    ''' </summary>
    ''' <param name="sPath">Full path to extrathumbs directory</param>
    ''' <returns>Last detected number of the discovered extrathumbs.</returns>
    Public Shared Function GetExtraModifier(ByVal sPath As String) As Integer
        Dim iMod As Integer = 0
        Dim lThumbs As New List(Of String)

        Try
            If Directory.Exists(sPath) Then

                Try
                    lThumbs.AddRange(Directory.GetFiles(sPath, "thumb*.jpg"))
                Catch
                End Try

                If lThumbs.Count > 0 Then
                    Dim cur As Integer = 0
                    For Each t As String In lThumbs
                        cur = Convert.ToInt32(Regex.Match(t, "(\d+).jpg").Groups(1).ToString)
                        iMod = Math.Max(iMod, cur)
                    Next
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return iMod
    End Function

    Public Shared Function GetFFMpeg() As String
        If Master.isWindows Then
            Return String.Concat(Functions.AppPath, "Bin", Path.DirectorySeparatorChar, "ffmpeg.exe")
        Else
            Return "ffmpeg"
        End If
    End Function

    ''' <summary>
    ''' Get a list of paths to all sources stored in the database
    ''' </summary>
    Public Shared Sub GetListOfSources()
        Master.SourcesList.Clear()
        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
            SQLcommand.CommandText = "SELECT sources.Path FROM sources;"
            Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                While SQLreader.Read
                    Master.SourcesList.Add(SQLreader("Path").ToString)
                End While
            End Using
        End Using
    End Sub

    Public Shared Function GetSeasonDirectoryFromShowPath(ByVal ShowPath As String, ByVal iSeason As Integer) As String
        If Directory.Exists(ShowPath) Then
            Dim dInfo As New DirectoryInfo(ShowPath)

            For Each sDir As DirectoryInfo In dInfo.GetDirectories
                For Each rShow As Settings.TVShowRegEx In Master.eSettings.TVShowRegexes.Where(Function(s) s.SeasonFromDirectory = True)
                    For Each sMatch As Match In Regex.Matches(FileUtils.Common.GetDirectory(sDir.FullName), rShow.SeasonRegex, RegexOptions.IgnoreCase)
                        Try
                            If (IsNumeric(sMatch.Groups("season").Value) AndAlso iSeason = Convert.ToInt32(sMatch.Groups("season").Value)) OrElse (Regex.IsMatch(sMatch.Groups("season").Value, "specials?", RegexOptions.IgnoreCase) AndAlso iSeason = 0) Then
                                Return sDir.FullName
                            End If
                        Catch ex As Exception
                            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                        End Try
                    Next
                Next
            Next
        End If
        'no matches
        Return String.Empty
    End Function

    Public Shared Function HasModifier() As Boolean
        With Master.GlobalScrapeMod
            If .Extra OrElse .Fanart OrElse .Meta OrElse .NFO OrElse .Poster OrElse .Trailer Then Return True
        End With

        Return False
    End Function

    Public Shared Function IsSeasonDirectory(ByVal sPath As String) As Boolean
        For Each rShow As Settings.TVShowRegEx In Master.eSettings.TVShowRegexes.Where(Function(s) s.SeasonFromDirectory = True)
            If Regex.IsMatch(FileUtils.Common.GetDirectory(sPath), rShow.SeasonRegex, RegexOptions.IgnoreCase) Then Return True
        Next
        'no matches
        Return False
    End Function

    ''' <summary>
    ''' Convert a list(of T) to a string of separated values
    ''' </summary>
    ''' <param name="source">List(of T)</param>
    ''' <param name="separator">Character or string to use as a value separator</param>
    ''' <returns>String of separated values</returns>
    Public Shared Function ListToStringWithSeparator(Of T)(ByVal source As IList(Of T), ByVal separator As String) As String
        If source Is Nothing Then Throw New ArgumentNullException("Source parameter cannot be nothing")
        If String.IsNullOrEmpty(separator) Then Throw New ArgumentException("Separator parameter cannot be nothing or empty")

        Dim values As String() = source.Cast(Of Object)().Where(Function(n) n IsNot Nothing).Select(Function(s) s.ToString).ToArray

        Return String.Join(separator, values)
    End Function

    Public Shared Sub PNLDoubleBuffer(ByRef cPNL As Panel)
        Dim conType As Type = cPNL.GetType
        Dim pi As System.Reflection.PropertyInfo = conType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)
        pi.SetValue(cPNL, True, Nothing)
    End Sub

    ''' <summary>
    ''' Constrain a number to the nearest multiple
    ''' </summary>
    ''' <param name="iNumber">Number to quantize</param>
    ''' <param name="iMultiple">Multiple of constraint.</param>
    Public Shared Function Quantize(ByVal iNumber As Integer, ByVal iMultiple As Integer) As Integer
        Return Convert.ToInt32(System.Math.Round(iNumber / iMultiple, 0) * iMultiple)
    End Function

    Public Shared Function ReadStreamToEnd(ByVal rStream As Stream) As Byte()
        Dim StreamBuffer(4096) As Byte
        Dim BlockSize As Integer = 0
        Using mStream As MemoryStream = New MemoryStream()
            Do
                BlockSize = rStream.Read(StreamBuffer, 0, StreamBuffer.Length)
                If BlockSize > 0 Then mStream.Write(StreamBuffer, 0, BlockSize)
            Loop While BlockSize > 0
            Return mStream.ToArray
        End Using
    End Function

    Public Shared Function ScrapeModifierAndAlso(ByVal Options As Structures.ScrapeModifier, ByVal Options2 As Structures.ScrapeModifier) As Structures.ScrapeModifier
        Dim filterModifier As New Structures.ScrapeModifier
        filterModifier.DoSearch = Options.DoSearch AndAlso Options2.DoSearch
        filterModifier.Extra = Options.Extra AndAlso Options2.Extra
        filterModifier.Fanart = Options.Fanart AndAlso Options2.Fanart
        filterModifier.Meta = Options.Meta AndAlso Options2.Meta
        filterModifier.NFO = Options.NFO AndAlso Options2.NFO
        filterModifier.Poster = Options.Poster AndAlso Options2.Poster
        filterModifier.Trailer = Options.Trailer AndAlso Options2.Trailer
        filterModifier.Actors = Options.Actors AndAlso Options2.Actors
        Return filterModifier
    End Function

    Public Shared Function ScrapeOptionsAndAlso(ByVal Options As Structures.ScrapeOptions, ByVal Options2 As Structures.ScrapeOptions) As Structures.ScrapeOptions
        Dim filterOptions As New Structures.ScrapeOptions
        filterOptions.bTitle = Options.bTitle AndAlso Options2.bTitle
        filterOptions.bYear = Options.bYear AndAlso Options2.bYear
        filterOptions.bMPAA = Options.bMPAA AndAlso Options2.bMPAA
        filterOptions.bCert = Options.bCert AndAlso Options2.bCert
        filterOptions.bRelease = Options.bRelease AndAlso Options2.bRelease
        filterOptions.bRating = Options.bRating AndAlso Options2.bRating
        filterOptions.bTrailer = Options.bTrailer AndAlso Options2.bTrailer
        filterOptions.bVotes = Options.bVotes AndAlso Options2.bVotes
        filterOptions.bCast = Options.bCast AndAlso Options2.bCast
        filterOptions.bTagline = Options.bTagline AndAlso Options2.bTagline
        filterOptions.bDirector = Options.bDirector AndAlso Options2.bDirector
        filterOptions.bGenre = Options.bGenre AndAlso Options2.bGenre
        filterOptions.bOutline = Options.bOutline AndAlso Options2.bOutline
        filterOptions.bPlot = Options.bPlot AndAlso Options2.bPlot
        filterOptions.bRuntime = Options.bRuntime AndAlso Options2.bRuntime
        filterOptions.bStudio = Options.bStudio AndAlso Options2.bStudio
        filterOptions.bWriters = Options.bWriters AndAlso Options2.bWriters
        filterOptions.bProducers = Options.bProducers AndAlso Options2.bProducers
        filterOptions.bMusicBy = Options.bMusicBy AndAlso Options2.bMusicBy
        filterOptions.bOtherCrew = Options.bOtherCrew AndAlso Options2.bOtherCrew
        filterOptions.bTop250 = Options.bTop250 AndAlso Options2.bTop250
        filterOptions.bCountry = Options.bCountry AndAlso Options2.bCountry
        filterOptions.bFullCrew = Options.bFullCrew AndAlso Options2.bFullCrew
        filterOptions.bFullCast = Options.bFullCast AndAlso Options2.bFullCast
        Return filterOptions
    End Function

    Public Shared Sub SetScraperMod(ByVal MType As Enums.ModType, ByVal MValue As Boolean, Optional ByVal DoClear As Boolean = True)
        With Master.GlobalScrapeMod
            If DoClear Then
                .Extra = False
                .Fanart = False
                .Meta = False
                .NFO = False
                .Poster = False
                .Trailer = False
                .DoSearch = False
                .Actors = False
            End If

            Select Case MType
                Case Enums.ModType.All
                    .Extra = MValue
                    .Fanart = MValue
                    .Meta = MValue
                    .NFO = MValue
                    .Poster = MValue
                    .Trailer = If(Master.eSettings.UpdaterTrailers, MValue, False)
                    .Actors = MValue
                Case Enums.ModType.Extra
                    .Extra = MValue
                Case Enums.ModType.Fanart
                    .Fanart = MValue
                Case Enums.ModType.Meta
                    .Meta = MValue
                Case Enums.ModType.NFO
                    .NFO = MValue
                Case Enums.ModType.Poster
                    .Poster = MValue
                Case Enums.ModType.Trailer
                    .Trailer = MValue
                Case Enums.ModType.DoSearch
                    .DoSearch = MValue
                Case Enums.ModType.Actor
                    .Actors = MValue
            End Select

        End With
    End Sub

    ''' <summary>
    ''' Check version of the MediaInfo dll. If newer than 0.7.11 then don't try to scan disc images with it.
    ''' </summary>
    Public Shared Sub TestMediaInfoDLL()
        Try
            'just assume dll is there since we're distributing full package... if it's not, user has bigger problems
            Dim dllPath As String = String.Concat(AppPath, "Bin", Path.DirectorySeparatorChar, "MediaInfo.DLL")
            Dim fVersion As FileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(dllPath)
            If fVersion.FileMinorPart <= 7 AndAlso fVersion.FileBuildPart <= 11 Then
                Master.CanScanDiscImage = True
            Else
                Master.CanScanDiscImage = False
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

#End Region 'Methods

End Class

Public Class Structures

#Region "Nested Types"

    Public Structure CustomUpdaterStruct

#Region "Fields"

        Dim Canceled As Boolean
        Dim Options As ScrapeOptions
        Dim ScrapeType As Enums.ScrapeType

#End Region 'Fields

    End Structure

    Public Structure MovieSource
        Dim id As String
        Dim Name As String
        Dim Path As String
        Dim Recursive As Boolean
        Dim UseFolderName As Boolean
        Dim IsSingle As Boolean
    End Structure
    Public Structure TVSource
        Dim id As String
        Dim Name As String
        Dim Path As String
    End Structure

    Public Structure DBMovie

#Region "Fields"

        Dim ClearExtras As Boolean
        Dim DateAdd As Long
        Dim ExtraPath As String
        Dim FanartPath As String
        Dim Filename As String
        Dim FileSource As String
        Dim ID As Long
        Dim IsLock As Boolean
        Dim IsMark As Boolean
        Dim isSingle As Boolean
        Dim OriginalTitle As String
        Dim ListTitle As String
        Dim Movie As MediaContainers.Movie
        Dim NeedsSave As Boolean
        Dim NfoPath As String
        Dim OutOfTolerance As Boolean
        Dim PosterPath As String
        Dim Source As String
        Dim SubPath As String
        Dim TrailerPath As String
        Dim UseFolder As Boolean
        Dim JobLog As MediaLog
#End Region 'Fields

    End Structure

    Public Structure DBTV

#Region "Fields"

        Dim EpFanartPath As String
        Dim EpID As Long
        Dim EpNeedsSave As Boolean
        Dim EpNfoPath As String
        Dim EpPosterPath As String
        Dim Filename As String
        Dim IsLockEp As Boolean
        Dim IsLockSeason As Boolean
        Dim IsLockShow As Boolean
        Dim IsMarkEp As Boolean
        Dim IsMarkSeason As Boolean
        Dim IsMarkShow As Boolean
        Dim SeasonFanartPath As String
        Dim SeasonPosterPath As String
        Dim ShowFanartPath As String
        Dim ShowID As Long
        Dim ShowLanguage As String
        Dim ShowNeedsSave As Boolean
        Dim ShowNfoPath As String
        Dim ShowPath As String
        Dim ShowPosterPath As String
        Dim Source As String
        Dim TVEp As MediaContainers.EpisodeDetails
        Dim TVShow As MediaContainers.TVShow
        Dim Ordering As Enums.Ordering

#End Region 'Fields

    End Structure

    Public Structure Scans

#Region "Fields"

        Dim Movies As Boolean
        Dim TV As Boolean

#End Region 'Fields

    End Structure

    Public Structure ScrapeInfo

#Region "Fields"

        Dim CurrentImage As Image
        Dim Ordering As Enums.Ordering
        Dim iEpisode As Integer
        Dim ImageType As Enums.TVImageType
        Dim iSeason As Integer
        Dim Options As Structures.TVScrapeOptions
        Dim SelectedLang As String
        Dim ShowID As Integer
        Dim ShowTitle As String
        Dim TVDBID As String
        Dim WithCurrent As Boolean
        Dim ScrapeType As Enums.ScrapeType

#End Region 'Fields

    End Structure

    Public Structure ScrapeModifier

#Region "Fields"

        Dim DoSearch As Boolean
        Dim Extra As Boolean
        Dim Fanart As Boolean
        Dim Meta As Boolean
        Dim NFO As Boolean
        Dim Poster As Boolean
        Dim Trailer As Boolean
        Dim Actors As Boolean
#End Region 'Fields

    End Structure

    Public Structure ScrapeOptions

#Region "Fields"

        Dim bCast As Boolean
        Dim bCert As Boolean
        Dim bDirector As Boolean
        Dim bFullCast As Boolean

        ' Why this 2 arent here?
        Dim bFullCrew As Boolean
        Dim bGenre As Boolean
        Dim bMPAA As Boolean
        Dim bMusicBy As Boolean
        Dim bOtherCrew As Boolean
        Dim bOutline As Boolean
        Dim bPlot As Boolean
        Dim bProducers As Boolean
        Dim bRating As Boolean
        Dim bRelease As Boolean
        Dim bRuntime As Boolean
        Dim bStudio As Boolean
        Dim bTagline As Boolean
        Dim bTitle As Boolean
        Dim bTop250 As Boolean
        Dim bCountry As Boolean
        Dim bTrailer As Boolean
        Dim bVotes As Boolean
        Dim bWriters As Boolean
        Dim bYear As Boolean

#End Region 'Fields

    End Structure

    Public Structure SettingsResult

#Region "Fields"

        Dim DidCancel As Boolean
        Dim NeedsRefresh As Boolean
        Dim NeedsUpdate As Boolean
        Dim NeedsRestart As Boolean
#End Region 'Fields

    End Structure

    Public Structure TVScrapeOptions

#Region "Fields"

        Dim bEpActors As Boolean
        Dim bEpAired As Boolean
        Dim bEpCredits As Boolean
        Dim bEpDirector As Boolean
        Dim bEpEpisode As Boolean
        Dim bEpPlot As Boolean
        Dim bEpRating As Boolean
        Dim bEpSeason As Boolean
        Dim bEpTitle As Boolean
        Dim bShowActors As Boolean
        Dim bShowEpisodeGuide As Boolean
        Dim bShowGenre As Boolean
        Dim bShowMPAA As Boolean
        Dim bShowPlot As Boolean
        Dim bShowPremiered As Boolean
        Dim bShowRating As Boolean
        Dim bShowStudio As Boolean
        Dim bShowTitle As Boolean

#End Region 'Fields

    End Structure

    Public Structure ModulesMenus
        Dim IfNoMovies As Boolean
        Dim IfNoTVShow As Boolean
    End Structure

#End Region 'Nested Types

End Class