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
Imports EmberAPI
Imports EmberScraperModule.XMLScraper.ScraperXML
Imports System.Reflection
Imports System.Drawing
Imports System.Drawing.Imaging

Public Class EmberXMLScraperModule
    Implements Interfaces.EmberMovieScraperModule

#Region "Fields"

    Private _Name As String = "Ember XML Movie Scrapers"
    Private _PostScraperEnabled As Boolean = False
    Private _ScraperEnabled As Boolean = False
    Private _setup As frmXMLSettingsHolder
    Private _postsetup As frmXMLMediaSettingsHolder
    Private XMLManager As ScraperManager = Nothing
    Private scraperName As String = String.Empty
    Private scraperFileName As String = String.Empty
    Private ScrapersLoaded As Boolean = False
    Private lMediaTag As XMLScraper.MediaTags.MediaTag
    Private LastDBMovieID As Long = -1
    Friend WithEvents bwPopulate As New System.ComponentModel.BackgroundWorker
    Public Shared ConfigScrapeModifier As New Structures.ScrapeModifier
    Public Shared ConfigOptions As New Structures.ScrapeOptions
    Public Shared _AssemblyName As String
    Friend WithEvents bwGetTralier As New System.ComponentModel.BackgroundWorker
    Private Structure Arguments
        Dim DBMovie As Structures.DBMovie
    End Structure
#End Region 'Fields

#Region "Events"

    Public Event ModuleSettingsChanged() Implements Interfaces.EmberMovieScraperModule.ModuleSettingsChanged
    Public Event MovieScraperEvent(ByVal eType As Enums.MovieScraperEventType, ByVal Parameter As Object) Implements Interfaces.EmberMovieScraperModule.MovieScraperEvent
    Public Event SetupPostScraperChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.EmberMovieScraperModule.PostScraperSetupChanged
    Public Event SetupScraperChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.EmberMovieScraperModule.ScraperSetupChanged

#End Region 'Events

#Region "Properties"

    Public ReadOnly Property IsPostScraper() As Boolean Implements Interfaces.EmberMovieScraperModule.IsPostScraper
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property IsScraper() As Boolean Implements Interfaces.EmberMovieScraperModule.IsScraper
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property ModuleName() As String Implements Interfaces.EmberMovieScraperModule.ModuleName
        Get
            Return "Ember XML Scraper"
        End Get
    End Property

    Public ReadOnly Property ModuleVersion() As String Implements Interfaces.EmberMovieScraperModule.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

    Property PostScraperEnabled() As Boolean Implements Interfaces.EmberMovieScraperModule.PostScraperEnabled
        Get
            Return _PostScraperEnabled
        End Get
        Set(ByVal value As Boolean)
            _PostScraperEnabled = value
        End Set
    End Property

    Property ScraperEnabled() As Boolean Implements Interfaces.EmberMovieScraperModule.ScraperEnabled
        Get
            Return _ScraperEnabled
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled = value
            If _ScraperEnabled Then
                Enabled()
            Else
                Disabled()
            End If
        End Set
    End Property

#End Region 'Properties

#Region "Methods"
    Delegate Sub DelegateFunction()
    Sub Enabled()
    End Sub
    Sub Disabled()
    End Sub
    Sub Init(ByVal sAssemblyName As String) Implements Interfaces.EmberMovieScraperModule.Init
        _AssemblyName = sAssemblyName
        scraperName = AdvancedSettings.GetSetting("ScraperName", "NFO Scraper")
        scraperFileName = AdvancedSettings.GetSetting("ScraperFileName", "")
        PrepareScraper()
        XMLManager.LoadScrapers(scraperFileName)
        LoadScraperSettings()
    End Sub

    Function QueryPostScraperCapabilities(ByVal cap As Enums.PostScraperCapabilities) As Boolean Implements Interfaces.EmberMovieScraperModule.QueryPostScraperCapabilities
        Select Case cap
            Case Enums.PostScraperCapabilities.Fanart
                'If MySettings.UseTMDB Then Return True
            Case Enums.PostScraperCapabilities.Poster
                'If MySettings.UseIMPA OrElse MySettings.UseMPDB OrElse MySettings.UseTMDB Then Return True
            Case Enums.PostScraperCapabilities.Trailer
                'If MySettings.DownloadTrailers Then Return True
        End Select
        Return True
    End Function

    Sub LoadSettings()
        ConfigOptions.bTitle = AdvancedSettings.GetBooleanSetting("DoTitle", True)
        ConfigOptions.bYear = AdvancedSettings.GetBooleanSetting("DoYear", True)
        ConfigOptions.bMPAA = AdvancedSettings.GetBooleanSetting("DoMPAA", True)
        ConfigOptions.bRelease = AdvancedSettings.GetBooleanSetting("DoRelease", True)
        ConfigOptions.bRuntime = AdvancedSettings.GetBooleanSetting("DoRuntime", True)
        ConfigOptions.bRating = AdvancedSettings.GetBooleanSetting("DoRating", True)
        ConfigOptions.bVotes = AdvancedSettings.GetBooleanSetting("DoVotes", True)
        ConfigOptions.bStudio = AdvancedSettings.GetBooleanSetting("DoStudio", True)
        ConfigOptions.bTagline = AdvancedSettings.GetBooleanSetting("DoTagline", True)
        ConfigOptions.bOutline = AdvancedSettings.GetBooleanSetting("DoOutline", True)
        ConfigOptions.bPlot = AdvancedSettings.GetBooleanSetting("DoPlot", True)
        ConfigOptions.bCast = AdvancedSettings.GetBooleanSetting("DoCast", True)
        ConfigOptions.bDirector = AdvancedSettings.GetBooleanSetting("DoDirector", True)
        ConfigOptions.bWriters = AdvancedSettings.GetBooleanSetting("DoWriters", True)
        ConfigOptions.bProducers = AdvancedSettings.GetBooleanSetting("DoProducers", True)
        ConfigOptions.bGenre = AdvancedSettings.GetBooleanSetting("DoGenres", True)
        ConfigOptions.bTrailer = AdvancedSettings.GetBooleanSetting("DoTrailer", True)
        ConfigOptions.bMusicBy = AdvancedSettings.GetBooleanSetting("DoMusic", True)
        ConfigOptions.bOtherCrew = AdvancedSettings.GetBooleanSetting("DoOtherCrews", True)
        ConfigOptions.bFullCast = AdvancedSettings.GetBooleanSetting("DoFullCast", True)
        ConfigOptions.bFullCrew = AdvancedSettings.GetBooleanSetting("DoFullCrews", True)
        ConfigOptions.bTop250 = AdvancedSettings.GetBooleanSetting("DoTop250", True)
        ConfigOptions.bCountry = AdvancedSettings.GetBooleanSetting("DoCountry", True)
        ConfigOptions.bCert = AdvancedSettings.GetBooleanSetting("DoCert", True)

        ConfigScrapeModifier.DoSearch = True
        ConfigScrapeModifier.Meta = True
        ConfigScrapeModifier.NFO = True
        ConfigScrapeModifier.Extra = True
        ConfigScrapeModifier.Actors = True

        ConfigScrapeModifier.Poster = AdvancedSettings.GetBooleanSetting("DoPoster", True)
        ConfigScrapeModifier.Fanart = AdvancedSettings.GetBooleanSetting("DoFanart", True)
        ConfigScrapeModifier.Trailer = AdvancedSettings.GetBooleanSetting("DoTrailer", True)
    End Sub

    Sub SaveSettings()
        AdvancedSettings.SetBooleanSetting("DoFullCast", ConfigOptions.bFullCast)
        AdvancedSettings.SetBooleanSetting("DoFullCrews", ConfigOptions.bFullCrew)
        AdvancedSettings.SetBooleanSetting("DoTitle", ConfigOptions.bTitle)
        AdvancedSettings.SetBooleanSetting("DoYear", ConfigOptions.bYear)
        AdvancedSettings.SetBooleanSetting("DoMPAA", ConfigOptions.bMPAA)
        AdvancedSettings.SetBooleanSetting("DoRelease", ConfigOptions.bRelease)
        AdvancedSettings.SetBooleanSetting("DoRuntime", ConfigOptions.bRuntime)
        AdvancedSettings.SetBooleanSetting("DoRating", ConfigOptions.bRating)
        AdvancedSettings.SetBooleanSetting("DoVotes", ConfigOptions.bVotes)
        AdvancedSettings.SetBooleanSetting("DoStudio", ConfigOptions.bStudio)
        AdvancedSettings.SetBooleanSetting("DoTagline", ConfigOptions.bTagline)
        AdvancedSettings.SetBooleanSetting("DoOutline", ConfigOptions.bOutline)
        AdvancedSettings.SetBooleanSetting("DoPlot", ConfigOptions.bPlot)
        AdvancedSettings.SetBooleanSetting("DoCast", ConfigOptions.bCast)
        AdvancedSettings.SetBooleanSetting("DoDirector", ConfigOptions.bDirector)
        AdvancedSettings.SetBooleanSetting("DoWriters", ConfigOptions.bWriters)
        AdvancedSettings.SetBooleanSetting("DoProducers", ConfigOptions.bProducers)
        AdvancedSettings.SetBooleanSetting("DoGenres", ConfigOptions.bGenre)
        AdvancedSettings.SetBooleanSetting("DoTrailer", ConfigOptions.bTrailer)
        AdvancedSettings.SetBooleanSetting("DoMusic", ConfigOptions.bMusicBy)
        AdvancedSettings.SetBooleanSetting("DoOtherCrews", ConfigOptions.bOtherCrew)
        AdvancedSettings.SetBooleanSetting("DoCountry", ConfigOptions.bCountry)
        AdvancedSettings.SetBooleanSetting("DoTop250", ConfigOptions.bTop250)
        AdvancedSettings.SetBooleanSetting("DoCert", ConfigOptions.bCert)
        AdvancedSettings.SetBooleanSetting("FullCast", ConfigOptions.bFullCast)
        AdvancedSettings.SetBooleanSetting("FullCrew", ConfigOptions.bFullCrew)

        AdvancedSettings.SetBooleanSetting("DoPoster", ConfigScrapeModifier.Poster)
        AdvancedSettings.SetBooleanSetting("DoFanart", ConfigScrapeModifier.Fanart)
        'AdvancedSettings.SetBooleanSetting("DoTrailer", ConfigScrapeModifier.Trailer)
    End Sub

    Function GetMovieStudio(ByRef DBMovie As Structures.DBMovie, ByRef sStudio As List(Of String)) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.GetMovieStudio
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Function Scraper(ByRef DBMovie As Structures.DBMovie, ByRef ScrapeType As Enums.ScrapeType, ByRef Options As Structures.ScrapeOptions) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.Scraper
        Try
            lMediaTag = Nothing
            LoadSettings()
            LastDBMovieID = -1
            If Not ScrapersLoaded AndAlso Not String.IsNullOrEmpty(scraperFileName) Then
                XMLManager.LoadScrapers(scraperFileName)
                LoadScraperSettings()
                ScrapersLoaded = True
            End If
            If scraperName = String.Empty Then
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.Notification, New List(Of Object)(New Object() {"info", 5, Master.eLang.GetString(998, "XML Scraper"), String.Format(Master.eLang.GetString(998, "No XML Scraper Defined {0}."), vbNewLine), Nothing}))
                Return New Interfaces.ModuleResult With {.breakChain = False}
            End If
            Dim res As New List(Of XMLScraper.ScraperLib.ScrapeResultsEntity)

            If Master.GlobalScrapeMod.NFO Then

                If ScrapeType = Enums.ScrapeType.SingleScrape AndAlso Master.GlobalScrapeMod.DoSearch AndAlso _
                    ModulesManager.Instance.externalScrapersModules.OrderBy(Function(y) y.ScraperOrder).FirstOrDefault(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled).AssemblyName = _AssemblyName Then

                    DBMovie.ClearExtras = True
                    DBMovie.PosterPath = String.Empty
                    DBMovie.FanartPath = String.Empty
                    DBMovie.TrailerPath = String.Empty
                    DBMovie.ExtraPath = String.Empty
                    DBMovie.SubPath = String.Empty
                    DBMovie.NfoPath = String.Empty
                    DBMovie.Movie.Clear()
                End If
                Dim tmpTitle As String = DBMovie.Movie.Title
                If String.IsNullOrEmpty(tmpTitle) Then
                    If FileUtils.Common.isVideoTS(DBMovie.Filename) Then
                        tmpTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).Name, False)
                    ElseIf FileUtils.Common.isBDRip(DBMovie.Filename) Then
                        tmpTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).FullName).Name, False)
                    Else
                        tmpTitle = StringUtils.FilterName(If(DBMovie.isSingle, Directory.GetParent(DBMovie.Filename).Name, Path.GetFileNameWithoutExtension(DBMovie.Filename)))
                    End If
                End If
                Select Case ScrapeType
                    Case Enums.ScrapeType.FilterAuto, Enums.ScrapeType.FullAuto, Enums.ScrapeType.MarkAuto, Enums.ScrapeType.NewAuto, Enums.ScrapeType.UpdateAuto
                        res = XMLManager.GetResults(scraperName, tmpTitle, DBMovie.Movie.Year, XMLScraper.ScraperLib.MediaType.movie)
                        If res.Count > 0 Then
                            ' Get first and go
                            lMediaTag = XMLManager.GetDetails(res(0))
                            MapFields(DBMovie, DirectCast(lMediaTag, XMLScraper.MediaTags.MovieTag), Options)
                        End If
                    Case Else
                        res = XMLManager.GetResults(scraperName, tmpTitle, DBMovie.Movie.Year, XMLScraper.ScraperLib.MediaType.movie)
                        If res.Count > 1 Then  'OrElse (res.Count = 1 AndAlso ScrapeType = Enums.ScrapeType.SingleScrape) Then
                            Using dlg As New dlgSearchResults
                                Dim ScraperThumb As String = String.Empty
                                Dim s As ScraperInfo = XMLManager.AllScrapers.FirstOrDefault(Function(y) y.ScraperName = scraperName)
                                If Not IsNothing(s) Then
                                    If File.Exists(s.ScraperThumb) Then
                                        ScraperThumb = s.ScraperThumb
                                    End If
                                    lMediaTag = dlg.ShowDialog(res, DBMovie.Movie.Title, scraperName, ScraperThumb, XMLManager)
                                    If Not IsNothing(lMediaTag) Then
                                        MapFields(DBMovie, DirectCast(lMediaTag, XMLScraper.MediaTags.MovieTag), Options)
                                    Else
                                        Return New Interfaces.ModuleResult With {.breakChain = False, .Cancelled = True}
                                    End If
                                End If
                            End Using
                        ElseIf res.Count = 1 Then
                            lMediaTag = XMLManager.GetDetails(res(0))
                            MapFields(DBMovie, DirectCast(lMediaTag, XMLScraper.MediaTags.MovieTag), Options)
                        Else
                            Return New Interfaces.ModuleResult With {.breakChain = False, .Cancelled = True}
                        End If
                End Select
            ElseIf Master.GlobalScrapeMod.Trailer Then

                If ScrapeType = Enums.ScrapeType.SingleScrape AndAlso Master.GlobalScrapeMod.DoSearch AndAlso _
                    ModulesManager.Instance.externalScrapersModules.OrderBy(Function(y) y.ScraperOrder).FirstOrDefault(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled).AssemblyName = _AssemblyName Then

                    DBMovie.TrailerPath = String.Empty
                End If
                Dim tmpTitle As String = DBMovie.Movie.Title
                If String.IsNullOrEmpty(tmpTitle) Then
                    If FileUtils.Common.isVideoTS(DBMovie.Filename) Then
                        tmpTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).Name, False)
                    ElseIf FileUtils.Common.isBDRip(DBMovie.Filename) Then
                        tmpTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).FullName).Name, False)
                    Else
                        tmpTitle = StringUtils.FilterName(If(DBMovie.isSingle, Directory.GetParent(DBMovie.Filename).Name, Path.GetFileNameWithoutExtension(DBMovie.Filename)))
                    End If
                End If
                Select Case ScrapeType
                    Case Enums.ScrapeType.FilterAuto, Enums.ScrapeType.FullAuto, Enums.ScrapeType.MarkAuto, Enums.ScrapeType.NewAuto, Enums.ScrapeType.UpdateAuto
                        res = XMLManager.GetResults(scraperName, tmpTitle, DBMovie.Movie.Year, XMLScraper.ScraperLib.MediaType.movie)
                        If res.Count > 0 Then
                            ' Get first and go
                            lMediaTag = XMLManager.GetDetails(res(0))
                            LastDBMovieID = DBMovie.ID
                        End If
                    Case Else
                        res = XMLManager.GetResults(scraperName, tmpTitle, DBMovie.Movie.Year, XMLScraper.ScraperLib.MediaType.movie)
                        If res.Count > 1 Then  'OrElse (res.Count = 1 AndAlso ScrapeType = Enums.ScrapeType.SingleScrape) Then
                            Using dlg As New dlgSearchResults
                                Dim ScraperThumb As String = String.Empty
                                Dim s As ScraperInfo = XMLManager.AllScrapers.FirstOrDefault(Function(y) y.ScraperName = scraperName)
                                If Not IsNothing(s) Then
                                    If File.Exists(s.ScraperThumb) Then
                                        ScraperThumb = s.ScraperThumb
                                    End If
                                    lMediaTag = dlg.ShowDialog(res, DBMovie.Movie.Title, scraperName, ScraperThumb, XMLManager)
                                    If Not IsNothing(lMediaTag) Then
                                        LastDBMovieID = DBMovie.ID
                                    Else
                                        Return New Interfaces.ModuleResult With {.breakChain = False, .Cancelled = True}
                                    End If
                                End If
                            End Using
                        ElseIf res.Count = 1 Then
                            lMediaTag = XMLManager.GetDetails(res(0))
                            LastDBMovieID = DBMovie.ID
                        Else
                            Return New Interfaces.ModuleResult With {.breakChain = False, .Cancelled = True}
                        End If
                End Select
            End If
        Catch ex As Exception
        End Try
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function
    Sub MapFields(ByRef DBMovie As Structures.DBMovie, ByVal lMediaTag As XMLScraper.MediaTags.MovieTag, ByVal gOptions As Structures.ScrapeOptions)

        Dim Options As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(gOptions, ConfigOptions)
        Dim tTitle As String = String.Empty
        Dim OldTitle As String = DBMovie.Movie.Title

        Options = Functions.ScrapeOptionsAndAlso(Options, Functions.LocksToOptions())
        LastDBMovieID = DBMovie.ID
        If Options.bCert Then
            If Not String.IsNullOrEmpty(Master.eSettings.CertificationLang) Then
                DBMovie.Movie.Certification = (lMediaTag.Certifications.FirstOrDefault(Function(y) y.StartsWith(Master.eSettings.CertificationLang)))
                If Not DBMovie.Movie.Certification Is Nothing AndAlso DBMovie.Movie.Certification.IndexOf("(") >= 0 Then DBMovie.Movie.Certification = Web.HttpUtility.HtmlDecode(DBMovie.Movie.Certification.Substring(0, DBMovie.Movie.Certification.IndexOf("(")))
            Else
                DBMovie.Movie.Certification = Web.HttpUtility.HtmlDecode(Strings.Join(lMediaTag.Certifications.ToArray(), " / "))
            End If
        End If
        If Options.bDirector Then DBMovie.Movie.Director = Web.HttpUtility.HtmlDecode(Strings.Join(lMediaTag.Directors.ToArray(), " / "))
        If Options.bGenre AndAlso Not Master.eSettings.LockGenre Then DBMovie.Movie.Genre = Web.HttpUtility.HtmlDecode(Strings.Join(lMediaTag.Genres.ToArray(), " / "))
        If Options.bMPAA Then DBMovie.Movie.MPAA = Web.HttpUtility.HtmlDecode(lMediaTag.MPAA)
        If Options.bPlot Then
            If Not String.IsNullOrEmpty(lMediaTag.Plot) Then
                DBMovie.Movie.Plot = Web.HttpUtility.HtmlDecode(lMediaTag.Plot)
            ElseIf Master.eSettings.OutlineForPlot Then
                DBMovie.Movie.Plot = Web.HttpUtility.HtmlDecode(lMediaTag.Outline)
            End If
        End If
        If Options.bOutline AndAlso Not Master.eSettings.LockOutline Then DBMovie.Movie.Outline = Web.HttpUtility.HtmlDecode(lMediaTag.Outline)
        If Options.bRelease Then DBMovie.Movie.ReleaseDate = lMediaTag.Premiered
        If Options.bRating AndAlso Not Master.eSettings.LockRating Then DBMovie.Movie.Rating = Web.HttpUtility.HtmlDecode(lMediaTag.Rating.ToString)
        If Options.bRuntime Then DBMovie.Movie.Runtime = lMediaTag.Runtime
        'DBMovie.Movie.Sets = lMediaTag.Sets
        If Options.bStudio AndAlso Not Master.eSettings.LockStudio Then DBMovie.Movie.Studio = Web.HttpUtility.HtmlDecode(lMediaTag.Studio)
        If Options.bTagline AndAlso Not Master.eSettings.LockTagline Then DBMovie.Movie.Tagline = Web.HttpUtility.HtmlDecode(lMediaTag.Tagline)
        If Options.bTitle AndAlso Not Master.eSettings.LockTitle Then
            DBMovie.Movie.Title = Web.HttpUtility.HtmlDecode(lMediaTag.Title)
            Dim oldOTitle As String = DBMovie.Movie.OriginalTitle
            DBMovie.Movie.OriginalTitle = Web.HttpUtility.HtmlDecode(lMediaTag.OriginalTitle)
            If String.IsNullOrEmpty(DBMovie.Movie.Title) OrElse Not Master.eSettings.LockTitle Then
                If String.IsNullOrEmpty(oldOTitle) OrElse Not oldOTitle = DBMovie.Movie.OriginalTitle Then
                    DBMovie.Movie.SortTitle = String.Empty
                End If
            End If
        End If
        If Options.bTop250 Then
            If Not lMediaTag.Top250 = 0 Then
                DBMovie.Movie.Top250 = lMediaTag.Top250.ToString
            Else
                DBMovie.Movie.Top250 = String.Empty
            End If
        End If
        If Options.bCountry Then DBMovie.Movie.Country = Web.HttpUtility.HtmlDecode(Strings.Join(lMediaTag.Country.ToArray(), " / "))
        If Options.bVotes Then DBMovie.Movie.Votes = lMediaTag.Votes.ToString
        If Options.bWriters Then DBMovie.Movie.OldCredits = Web.HttpUtility.HtmlDecode(Strings.Join(lMediaTag.Writers.ToArray, " / "))
        If Options.bYear Then DBMovie.Movie.Year = lMediaTag.Year.ToString
        DBMovie.Movie.PlayCount = lMediaTag.PlayCount.ToString
        DBMovie.Movie.ID = If(String.IsNullOrEmpty(lMediaTag.ID), DBMovie.Movie.ID, lMediaTag.ID)
        If Options.bCast Then
            DBMovie.Movie.Actors.Clear()
            For Each p As XMLScraper.MediaTags.PersonTag In lMediaTag.Actors
                Dim person As New MediaContainers.Person
                person.Name = Web.HttpUtility.HtmlDecode(p.Name)
                person.Role = Web.HttpUtility.HtmlDecode(p.Role)
                person.Thumb = p.Thumb.Thumb
                DBMovie.Movie.Actors.Add(person)
            Next
        End If
        If Not String.IsNullOrEmpty(DBMovie.Movie.Title) Then
            tTitle = StringUtils.FilterTokens(DBMovie.Movie.Title)
            If Not OldTitle = DBMovie.Movie.Title OrElse String.IsNullOrEmpty(DBMovie.Movie.SortTitle) Then DBMovie.Movie.SortTitle = tTitle
            If Master.eSettings.DisplayYear AndAlso Not String.IsNullOrEmpty(DBMovie.Movie.Year) Then
                DBMovie.ListTitle = String.Format("{0} ({1})", tTitle, DBMovie.Movie.Year)
            Else
                DBMovie.ListTitle = tTitle
            End If
        Else
            If FileUtils.Common.isVideoTS(DBMovie.Filename) Then
                DBMovie.ListTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).Name)
            ElseIf FileUtils.Common.isBDRip(DBMovie.Filename) Then
                DBMovie.ListTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).FullName).Name)
            Else
                If DBMovie.UseFolder AndAlso DBMovie.isSingle Then
                    DBMovie.ListTitle = StringUtils.FilterName(Directory.GetParent(DBMovie.Filename).Name)
                Else
                    DBMovie.ListTitle = StringUtils.FilterName(Path.GetFileNameWithoutExtension(DBMovie.Filename))
                End If
            End If
            If Not OldTitle = DBMovie.Movie.Title OrElse String.IsNullOrEmpty(DBMovie.Movie.SortTitle) Then DBMovie.Movie.SortTitle = DBMovie.ListTitle
        End If
    End Sub

    Public Function PostScraper(ByRef DBMovie As Structures.DBMovie, ByVal ScrapeType As Enums.ScrapeType) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.PostScraper
        Dim saveModifier As Structures.ScrapeModifier = Master.GlobalScrapeMod
        Dim Poster As New Images
        Dim Fanart As New Images
        Dim tURL As String = String.Empty
        Dim Trailer As New Trailers
        Try
            LoadSettings()
            If DBMovie.ID <> LastDBMovieID Then
                Dim res As New List(Of XMLScraper.ScraperLib.ScrapeResultsEntity)
                res = XMLManager.GetResults(scraperName, DBMovie.Movie.Title, DBMovie.Movie.Year, XMLScraper.ScraperLib.MediaType.movie)
                If res.Count > 0 Then
                    ' Get first and go ... scraper is not a XML scraper ... mixed scrapers
                    lMediaTag = XMLManager.GetDetails(res(0))
                Else
                    Return New Interfaces.ModuleResult With {.breakChain = False, .BoolProperty = False}
                End If
            End If
            Dim mediaTag As XMLScraper.MediaTags.MovieTag = DirectCast(lMediaTag, XMLScraper.MediaTags.MovieTag)
            Master.GlobalScrapeMod = Functions.ScrapeModifierAndAlso(Master.GlobalScrapeMod, ConfigScrapeModifier)
            If Master.GlobalScrapeMod.Poster Then
                DBMovie.Movie.Thumb.Clear()
                For Each t As XMLScraper.MediaTags.Thumbnail In mediaTag.Thumbs
                    DBMovie.Movie.Thumb.Add(t.Thumb)
                Next
                If DBMovie.Movie.Thumb.Count > 0 Then
                    If ScrapeType = Enums.ScrapeType.FullAsk OrElse ScrapeType = Enums.ScrapeType.NewAsk OrElse ScrapeType = Enums.ScrapeType.MarkAsk OrElse ScrapeType = Enums.ScrapeType.UpdateAsk Then
                        Using dlgSelect As New dlgImgSelect
                            Dim pResults As Containers.ImgResult
                            pResults = dlgSelect.ShowDialog(DBMovie, Enums.ImageType.Posters)
                            DBMovie.PosterPath = pResults.ImagePath
                        End Using
                    Else
                        Poster.Clear()
                        Poster.FromWeb(DBMovie.Movie.Thumb(0))
                        DBMovie.PosterPath = Poster.SaveAsPoster(DBMovie)
                    End If
                    RaiseEvent MovieScraperEvent(Enums.MovieScraperEventType.PosterItem, True)
                End If
            End If
            If Master.GlobalScrapeMod.Fanart Then
                DBMovie.Movie.Fanart.Thumb.Clear()
                For Each t As XMLScraper.MediaTags.Thumbnail In mediaTag.Fanart.Thumbs
                    Dim url As String = t.Url
                    DBMovie.Movie.Fanart.Thumb.Add(New MediaContainers.Thumb With {.Preview = t.Preview, .Text = t.Url})
                Next
                If DBMovie.Movie.Fanart.Thumb.Count > 0 Then
                    If ScrapeType = Enums.ScrapeType.FullAsk OrElse ScrapeType = Enums.ScrapeType.NewAsk OrElse ScrapeType = Enums.ScrapeType.MarkAsk OrElse ScrapeType = Enums.ScrapeType.UpdateAsk Then
                        Using dlgSelect As New dlgImgSelect
                            Dim pResults As Containers.ImgResult
                            pResults = dlgSelect.ShowDialog(DBMovie, Enums.ImageType.Fanart)
                            DBMovie.FanartPath = pResults.ImagePath
                        End Using
                    Else
                        Fanart.Clear()
                        Fanart.FromWeb(DBMovie.Movie.Fanart.Thumb(0).Text)
                        DBMovie.FanartPath = Fanart.SaveAsFanart(DBMovie)
                    End If
                    RaiseEvent MovieScraperEvent(Enums.MovieScraperEventType.FanartItem, True)
                End If
            End If
            If Master.GlobalScrapeMod.Trailer Then
                If mediaTag.Trailers.Count > 0 Then
                    If ScrapeType = Enums.ScrapeType.FullAsk OrElse ScrapeType = Enums.ScrapeType.NewAsk OrElse ScrapeType = Enums.ScrapeType.MarkAsk OrElse ScrapeType = Enums.ScrapeType.UpdateAsk Then
                        Using dTrailer As New dlgTrailer
                            DBMovie.Movie.Trailer = dTrailer.ShowDialog(DBMovie.Filename, mediaTag.Trailers)
                        End Using
                    Else
                        tURL = Trailer.DownloadSingleTrailer(DBMovie.Filename, mediaTag.Trailers, DBMovie.isSingle, DBMovie.Movie.Trailer)
                        If Not String.IsNullOrEmpty(tURL) Then
                            If tURL.Substring(0, 7) = "http://" Then
                                DBMovie.Movie.Trailer = tURL
                                'doSave = True
                            Else
                                DBMovie.TrailerPath = tURL
                                RaiseEvent MovieScraperEvent(Enums.MovieScraperEventType.TrailerItem, True)
                            End If
                        End If
                    End If
                End If
            End If
            If Master.GlobalScrapeMod.Extra Then
                If Master.eSettings.AutoET AndAlso DBMovie.isSingle Then
                    Try
                        ScrapeImages.GetPreferredFAasET(mediaTag.Fanart.Thumbs, DBMovie.Movie.IMDBID, DBMovie.Filename)
                    Catch ex As Exception
                    End Try
                End If
            End If
            If Master.GlobalScrapeMod.Actors AndAlso AdvancedSettings.GetBooleanSetting("ScrapeActorsThumbs", False) Then
                For Each act As MediaContainers.Person In DBMovie.Movie.Actors
                    Dim img As New Images
                    img.FromWeb(act.Thumb)
                    img.SaveAsActorThumb(act, Directory.GetParent(DBMovie.Filename).FullName)
                Next
            End If
        Catch ex As Exception
        End Try
        Master.GlobalScrapeMod = saveModifier
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Function InjectSetupScraper() As Containers.SettingsPanel Implements Interfaces.EmberMovieScraperModule.InjectSetupScraper
        Dim Spanel As New Containers.SettingsPanel
        _setup = New frmXMLSettingsHolder
        _setup.cbEnabled.Checked = _ScraperEnabled
        _setup.orderChanged()
        LoadSettings()
        _setup.cbEnabled.Checked = _ScraperEnabled
        _setup.chkTitle.Checked = ConfigOptions.bTitle
        _setup.chkYear.Checked = ConfigOptions.bYear
        _setup.chkMPAA.Checked = ConfigOptions.bMPAA
        _setup.chkRelease.Checked = ConfigOptions.bRelease
        _setup.chkRuntime.Checked = ConfigOptions.bRuntime
        _setup.chkRating.Checked = ConfigOptions.bRating
        _setup.chkVotes.Checked = ConfigOptions.bVotes
        _setup.chkStudio.Checked = ConfigOptions.bStudio
        _setup.chkTagline.Checked = ConfigOptions.bTagline
        _setup.chkOutline.Checked = ConfigOptions.bOutline
        _setup.chkPlot.Checked = ConfigOptions.bPlot
        _setup.chkCast.Checked = ConfigOptions.bCast
        _setup.chkDirector.Checked = ConfigOptions.bDirector
        _setup.chkWriters.Checked = ConfigOptions.bWriters
        _setup.chkGenre.Checked = ConfigOptions.bGenre
        _setup.chkCrew.Checked = ConfigOptions.bOtherCrew
        _setup.chkTop250.Checked = ConfigOptions.bTop250
        _setup.chkCountry.Checked = ConfigOptions.bCountry
        _setup.chkCertification.Checked = ConfigOptions.bCert
        _setup.chkFullCast.Checked = ConfigOptions.bFullCast

        _setup.chkFullCrew.Checked = ConfigOptions.bFullCrew
        _setup.chkTrailer.Checked = ConfigOptions.bTrailer
        _setup.chkMusicBy.Checked = ConfigOptions.bMusicBy
        _setup.chkProducers.Checked = ConfigOptions.bProducers

        If _setup.cbScraper.Items.Count = 0 Then
            _setup.cbScraper.Items.Add(scraperName)
            _setup.cbScraper.SelectedIndex = 0
        Else
            _setup.cbScraper.SelectedIndex = _setup.cbScraper.Items.IndexOf(scraperName)
        End If
        PopulateScraperSettings()
        Spanel.Name = String.Concat(Me._Name, "Scraper")
        Spanel.Text = Master.eLang.GetString(0, "Ember XML Movie Scrapers")
        Spanel.Prefix = "XMLMovieInfo_"
        Spanel.Order = 110
        Spanel.Parent = "pnlMovieData"
        Spanel.Type = Master.eLang.GetString(36, "Movies", True)
        Spanel.ImageIndex = If(_ScraperEnabled, 9, 10)
        Spanel.Panel = _setup.pnlSettings
        AddHandler _setup.SetupScraperChanged, AddressOf Handle_SetupScraperChanged
        AddHandler _setup.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _setup.PopulateScrapers, AddressOf PopulateSettings

        Return Spanel
    End Function

    Private Sub Handle_SetupScraperChanged(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled = state
        RaiseEvent SetupScraperChanged(String.Concat(Me._Name, "Scraper"), state, difforder)
    End Sub

    Private Sub Handle_ModuleSettingsChanged()
        PopulateScraperSettings()
        Dim s As ScraperInfo = XMLManager.AllScrapers.FirstOrDefault(Function(y) y.ScraperName = _setup.cbScraper.SelectedItem.ToString)
        If Not IsNothing(s) Then scraperFileName = s.FileName
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Function InjectSetupPostScraper() As Containers.SettingsPanel Implements Interfaces.EmberMovieScraperModule.InjectSetupPostScraper
        Dim Spanel As New Containers.SettingsPanel
        _postsetup = New frmXMLMediaSettingsHolder
        _postsetup.cbEnabled.Checked = _PostScraperEnabled
        _postsetup.orderChanged()
        Spanel.Name = String.Concat(Me._Name, "PostScraper")
        Spanel.Text = Master.eLang.GetString(0, "Ember XML Movie Scrapers")
        Spanel.Prefix = "XMLMovieMedia_"
        Spanel.Order = 110
        Spanel.Parent = "pnlMovieMedia"
        Spanel.Type = Master.eLang.GetString(36, "Movies", True)
        Spanel.ImageIndex = If(Me._PostScraperEnabled, 9, 10)
        Spanel.Panel = _postsetup.pnlSettings
        AddHandler _postsetup.SetupScraperChanged, AddressOf Handle_SetupPostScraperChanged
        'AddHandler _postsetup.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        'AddHandler _postsetup.PopulateScrapers, AddressOf PopulateSettings
        Return Spanel
    End Function

    Private Sub Handle_SetupPostScraperChanged(ByVal state As Boolean, ByVal difforder As Integer)
        PostScraperEnabled = state
        RaiseEvent SetupScraperChanged(String.Concat(Me._Name, "PostScraper"), state, difforder)
    End Sub

    Sub SaveSetupPostScraper(ByVal DoDispose As Boolean) Implements Interfaces.EmberMovieScraperModule.SaveSetupPostScraper
    End Sub

    Sub SaveSetupScraper(ByVal DoDispose As Boolean) Implements Interfaces.EmberMovieScraperModule.SaveSetupScraper
        ScraperEnabled = _setup.cbEnabled.Checked
        If Not _setup.cbScraper.SelectedItem Is Nothing Then
            scraperName = _setup.cbScraper.SelectedItem.ToString
            Dim s As ScraperInfo = XMLManager.AllScrapers.FirstOrDefault(Function(y) y.ScraperName = scraperName)
            If Not IsNothing(s) Then
                scraperFileName = s.FileName
                AdvancedSettings.SetSetting("ScraperName", scraperName)
                AdvancedSettings.SetSetting("ScraperFileName", scraperFileName)
                For Each ss As XMLScraper.ScraperLib.ScraperSetting In s.Settings.Where(Function(u) Not u.Hidden)
                    If ss.ID IsNot Nothing Then
                        For Each r As DataGridViewRow In _setup.dgvSettings.Rows
                            If r.Cells(1).Tag.ToString = ss.ID.ToString Then
                                AdvancedSettings.SetSetting(String.Concat(scraperFileName, ".", ss.ID.ToString), r.Cells(1).Value.ToString)
                                ss.Parameter = r.Cells(1).Value.ToString
                                Exit For
                            End If
                        Next
                    End If
                Next
            End If
        End If
        ConfigOptions.bTitle = _setup.chkTitle.Checked
        ConfigOptions.bYear = _setup.chkYear.Checked
        ConfigOptions.bMPAA = _setup.chkMPAA.Checked
        ConfigOptions.bRelease = _setup.chkRelease.Checked
        ConfigOptions.bRuntime = _setup.chkRuntime.Checked
        ConfigOptions.bRating = _setup.chkRating.Checked
        ConfigOptions.bVotes = _setup.chkVotes.Checked
        ConfigOptions.bStudio = _setup.chkStudio.Checked
        ConfigOptions.bTagline = _setup.chkTagline.Checked
        ConfigOptions.bOutline = _setup.chkOutline.Checked
        ConfigOptions.bPlot = _setup.chkPlot.Checked
        ConfigOptions.bCast = _setup.chkCast.Checked
        ConfigOptions.bDirector = _setup.chkDirector.Checked
        ConfigOptions.bWriters = _setup.chkWriters.Checked
        ConfigOptions.bGenre = _setup.chkGenre.Checked
        ConfigOptions.bTop250 = _setup.chkTop250.Checked
        ConfigOptions.bCountry = _setup.chkCountry.Checked
        ConfigOptions.bCert = _setup.chkCertification.Checked
        ' Bellow Not used in XBMC XML Scrapers ?????
        ConfigOptions.bTrailer = _setup.chkTrailer.Checked
        ConfigOptions.bMusicBy = _setup.chkMusicBy.Checked
        ConfigOptions.bOtherCrew = _setup.chkCrew.Checked
        ConfigOptions.bProducers = _setup.chkProducers.Checked
        ConfigOptions.bFullCrew = _setup.chkFullCrew.Checked
        ConfigOptions.bFullCast = _setup.chkFullCast.Checked
        SaveSettings()
        'ModulesManager.Instance.SaveSettings()
        If DoDispose Then
            RemoveHandler _setup.SetupScraperChanged, AddressOf Handle_SetupScraperChanged
            RemoveHandler _setup.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _setup.PopulateScrapers, AddressOf PopulateSettings
            _setup.Dispose()
        End If
    End Sub

    Sub LoadScraperSettings()
        Dim s As ScraperInfo = XMLManager.AllScrapers.FirstOrDefault(Function(y) y.ScraperName = scraperName)
        If Not s Is Nothing Then
            For Each ss As XMLScraper.ScraperLib.ScraperSetting In s.Settings.Where(Function(u) Not u.Hidden)
                If Not IsNothing(ss) Then
                    ss.Parameter = If(Not ss.ID Is Nothing, AdvancedSettings.GetSetting(String.Concat(scraperFileName, ".", ss.ID.ToString), ss.Default), ss.Default)
                End If
            Next
        End If
    End Sub

    Sub PopulateScraperSettings()
        Try
            If Not _setup.cbScraper.SelectedItem Is Nothing Then
                scraperName = _setup.cbScraper.SelectedItem.ToString
                Dim s As ScraperInfo = XMLManager.AllScrapers.FirstOrDefault(Function(y) y.ScraperName = scraperName)
                If Not IsNothing(s) Then
                    scraperFileName = s.FileName
                    _setup.pbPoster.Image = Nothing
                    _setup.pbPoster.Load(s.ScraperThumb)
                    _setup.lblLanguage.Text = Localization.ISOGetLangByCode2(s.ScraperLanguage)
                    _setup.dgvSettings.Rows.Clear()
                    For Each ss As XMLScraper.ScraperLib.ScraperSetting In s.Settings.Where(Function(u) Not u.Hidden)
                        If Not ss.Label Is Nothing Then
                            If ss.Values.Count > 0 Then
                                Dim i As Integer = _setup.dgvSettings.Rows.Add(ss.Label.ToString)
                                Dim dcb As DataGridViewComboBoxCell = DirectCast(_setup.dgvSettings.Rows(i).Cells(1), DataGridViewComboBoxCell)
                                dcb.DataSource = ss.Values.ToArray
                                dcb.Value = ss.Parameter.ToString
                                dcb.Tag = ss.ID.ToString
                            Else
                                Select Case ss.Type
                                    Case XMLScraper.ScraperLib.ScraperSetting.ScraperSettingType.bool
                                        Dim i As Integer = _setup.dgvSettings.Rows.Add(ss.Label.ToString)
                                        Dim cCell As New DataGridViewCheckBoxCell()
                                        _setup.dgvSettings.Rows(i).Cells(1) = cCell
                                        Dim dcb As DataGridViewCheckBoxCell = DirectCast(_setup.dgvSettings.Rows(i).Cells(1), DataGridViewCheckBoxCell)
                                        dcb.Value = If(ss.Parameter.ToString.ToLower = "true", True, False)
                                        cCell.Tag = ss.ID.ToString
                                    Case XMLScraper.ScraperLib.ScraperSetting.ScraperSettingType.text
                                        Dim i As Integer = _setup.dgvSettings.Rows.Add(ss.Label.ToString)
                                        Dim cCell As New DataGridViewTextBoxCell
                                        _setup.dgvSettings.Rows(i).Cells(1) = cCell
                                        Dim dcb As DataGridViewTextBoxCell = DirectCast(_setup.dgvSettings.Rows(i).Cells(1), DataGridViewTextBoxCell)
                                        dcb.Value = ss.Parameter.ToString
                                        cCell.Tag = ss.ID.ToString
                                    Case XMLScraper.ScraperLib.ScraperSetting.ScraperSettingType.int
                                        Dim i As Integer = _setup.dgvSettings.Rows.Add(ss.Label.ToString)
                                        Dim cCell As New DataGridViewTextBoxCell
                                        _setup.dgvSettings.Rows(i).Cells(1) = cCell
                                        Dim dcb As DataGridViewTextBoxCell = DirectCast(_setup.dgvSettings.Rows(i).Cells(1), DataGridViewTextBoxCell)
                                        dcb.Value = ss.Parameter.ToString
                                        cCell.Tag = ss.ID.ToString
                                    Case Else
                                        Dim i As Integer = _setup.dgvSettings.Rows.Add(ss.Label.ToString)
                                        _setup.dgvSettings.Rows(i).Cells(1).Tag = ss.ID.ToString
                                End Select
                                AddHandler _setup.dgvSettings.CellContentClick, AddressOf XMLScraperSettingClick
                            End If
                        End If
                    Next
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Sub XMLScraperSettingClick(ByVal sender As Object, ByVal e As DataGridViewCellEventArgs)
        Try
            If Not _setup.dgvSettings.Rows(e.RowIndex).Cells(e.ColumnIndex).Tag Is Nothing Then
                RaiseEvent ModuleSettingsChanged()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Function SelectImageOfType(ByRef mMovie As Structures.DBMovie, ByVal _DLType As Enums.ImageType, ByRef pResults As Containers.ImgResult, Optional ByVal _isEdit As Boolean = False, Optional ByVal preload As Boolean = False) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.SelectImageOfType

        Dim Poster As New Images
        Dim Fanart As New Images
        Try

            LoadSettings()
            If mMovie.ID <> LastDBMovieID Then
                Dim res As New List(Of XMLScraper.ScraperLib.ScrapeResultsEntity)
                res = XMLManager.GetResults(scraperName, mMovie.Movie.Title, mMovie.Movie.Year, XMLScraper.ScraperLib.MediaType.movie)
                If res.Count > 0 Then
                    ' Get first and go
                    lMediaTag = XMLManager.GetDetails(res(0))
                Else
                    Return New Interfaces.ModuleResult With {.breakChain = False, .BoolProperty = False}
                End If
            End If
            Dim mediaTag As XMLScraper.MediaTags.MovieTag = DirectCast(lMediaTag, XMLScraper.MediaTags.MovieTag)
            Master.GlobalScrapeMod = Functions.ScrapeModifierAndAlso(Master.GlobalScrapeMod, ConfigScrapeModifier)
            If Master.GlobalScrapeMod.Poster AndAlso _DLType = Enums.ImageType.Posters Then
                mMovie.Movie.Thumb.Clear()
                For Each t As XMLScraper.MediaTags.Thumbnail In mediaTag.Thumbs
                    mMovie.Movie.Thumb.Add(t.Thumb)
                Next
                If mMovie.Movie.Thumb.Count > 0 Then
                    'Poster.Clear()
                    'Poster.FromWeb(mMovie.Movie.Thumb(0))
                    Using dlgSelect As New dlgImgSelect
                        pResults = dlgSelect.ShowDialog(mMovie, Enums.ImageType.Posters, True)
                    End Using
                    'pResults.Posters = mMovie.Movie.Thumb
                    'pResults.ImagePath = Poster.SaveAsPoster(mMovie)
                End If
            End If
            If Master.GlobalScrapeMod.Fanart AndAlso _DLType = Enums.ImageType.Fanart Then
                mMovie.Movie.Fanart.Thumb.Clear()
                For Each t As XMLScraper.MediaTags.Thumbnail In mediaTag.Fanart.Thumbs
                    Dim url As String = t.Url
                    mMovie.Movie.Fanart.Thumb.Add(New MediaContainers.Thumb With {.Preview = t.Preview, .Text = t.Url})
                Next
                If mMovie.Movie.Fanart.Thumb.Count > 0 Then
                    'Fanart.Clear()
                    'Fanart.FromWeb(mMovie.Movie.Fanart.Thumb(0).Text)
                    Using dlgSelect As New dlgImgSelect
                        pResults = dlgSelect.ShowDialog(mMovie, Enums.ImageType.Fanart, True)
                    End Using
                    'pResults.Fanart.Thumb = mMovie.Movie.Fanart.Thumb
                    'pResults.ImagePath = Fanart.SaveAsFanart(mMovie)
                End If
            End If
        Catch ex As Exception
        End Try
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Function DownloadTrailer(ByRef DBMovie As Structures.DBMovie, ByRef sURL As String) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.DownloadTrailer
        Try
            LoadSettings()
            If DBMovie.ID <> LastDBMovieID Then
                Dim dlg As New dlgPleaseWait
                dlg.Show()
                Try
                    If Not bwGetTralier.IsBusy Then
                        Me.bwGetTralier.RunWorkerAsync(New Arguments With {.DBMovie = DBMovie})
                    End If
                    While bwGetTralier.IsBusy
                        If dlg.DialogResult = DialogResult.Cancel Then
                            bwGetTralier.CancelAsync()
                            Exit While
                        End If
                        Application.DoEvents()
                        Threading.Thread.Sleep(50)
                    End While
                Catch ex As Exception
                End Try
                dlg.Close()
                dlg = Nothing
            End If
            If lMediaTag Is Nothing Then
                Return New Interfaces.ModuleResult With {.breakChain = False, .BoolProperty = False}
            Else
                Dim mediaTag As XMLScraper.MediaTags.MovieTag = DirectCast(lMediaTag, XMLScraper.MediaTags.MovieTag)
                Using dTrailer As New dlgTrailer
                    sURL = dTrailer.ShowDialog(DBMovie.Filename, mediaTag.Trailers)
                End Using
            End If
        Catch ex As Exception
        End Try

        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Private Sub bwGetTralier_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwGetTralier.DoWork
        Try
            Dim Args As Arguments = DirectCast(e.Argument, Arguments)
            Dim res As New List(Of XMLScraper.ScraperLib.ScrapeResultsEntity)
            res = XMLManager.GetResults(scraperName, Args.DBMovie.Movie.Title, Args.DBMovie.Movie.Year, XMLScraper.ScraperLib.MediaType.movie)
            If res.Count > 0 Then
                ' Get first and go ... scraper is not a XML scraper ... mixed scrapers
                lMediaTag = XMLManager.GetDetails(res(0))
            Else
                lMediaTag = Nothing
            End If
        Catch ex As Exception
            lMediaTag = Nothing
        End Try
    End Sub

    Sub PopulateSettings()
        bwPopulate.WorkerReportsProgress = True
        bwPopulate.RunWorkerAsync()
    End Sub
    Private Sub bwPopulate_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwPopulate.DoWork
        XMLManager.ReloadScrapers(XMLScraper.ScraperLib.ScraperContent.movies)
        ScrapersLoaded = True
    End Sub
    Private Sub bwPopulate_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwPopulate.RunWorkerCompleted
        PoupulateForm()
        LoadScraperSettings()
        PopulateScraperSettings()
        _setup.parentRunning = False
    End Sub

    Sub PoupulateForm()
        If _setup Is Nothing Then Return
        If _setup.InvokeRequired Then
            _setup.Invoke(New DelegateFunction(AddressOf PoupulateForm))
            Return
        End If
        PopulateScraperSettings()
        _setup.cbScraper.Items.Clear()
        For Each s As ScraperInfo In XMLManager.AllScrapers
            _setup.cbScraper.Items.Add(s.ScraperName)
        Next
        If _setup.cbScraper.Items.Count = 0 Then
            _setup.cbScraper.Items.Add(scraperName)
            _setup.cbScraper.SelectedIndex = 0
        Else
            _setup.cbScraper.SelectedIndex = _setup.cbScraper.Items.IndexOf(scraperName)
        End If
    End Sub
    Sub PrepareScraper()
        If XMLManager Is Nothing Then
            Dim mName As String = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location)
            Dim tPath As String = Path.Combine(Functions.AppPath, String.Concat("Modules", Path.DirectorySeparatorChar, mName))
            Dim cPath As String = Path.Combine(Functions.AppPath, String.Concat("Modules", Path.DirectorySeparatorChar, mName, Path.DirectorySeparatorChar, "Cache"))
            If Not Directory.Exists(tPath) Then
                Directory.CreateDirectory(tPath)
            End If
            If Not Directory.Exists(cPath) Then
                Directory.CreateDirectory(cPath)
            End If
            XMLManager = New ScraperManager(tPath, String.Concat(cPath, Path.DirectorySeparatorChar))
        End If
    End Sub
    Public Sub PostScraperOrderChanged() Implements EmberAPI.Interfaces.EmberMovieScraperModule.PostScraperOrderChanged
        _postsetup.orderChanged()
    End Sub
    Public Sub ScraperOrderChanged() Implements EmberAPI.Interfaces.EmberMovieScraperModule.ScraperOrderChanged
        _setup.orderChanged()
    End Sub
#End Region 'Methods


End Class