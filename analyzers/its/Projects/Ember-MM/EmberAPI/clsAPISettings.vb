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
Imports System.Xml.Serialization
Imports System.Net
Imports System.Drawing
Imports System.Windows.Forms

<Serializable()> _
Public Class Settings

#Region "Fields"

    Private _actorlimit As Integer
    Private _allsbanner As Boolean
    Private _allsbannertype As Enums.ShowBannerType
    Private _allsposterheight As Integer
    Private _allsposterQuality As Integer
    Private _allspostersize As Enums.PosterSize
    Private _allsposterwidth As Integer
    Private _allwaysdisplaygenrestext As Boolean
    Private _alwaysgetenglishtvimages As Boolean
    Private _clickscrape As Boolean
    Private _askcheckboxscrape As Boolean
    Private _autobd As Boolean
    Private _autodetectvts As Boolean
    Private _autoET As Boolean
    Private _autoETsize As Enums.FanartSize
    Private _autothumbnospoilers As Boolean
    Private _autothumbs As Integer
    Private _bdpath As String
    Private _castimagesonly As Boolean
    Private _certificationLang As String
    Private _checkupdates As Boolean
    Private _cleandb As Boolean
    Private _cleandotfanartJpg As Boolean
    Private _cleanextrathumbs As Boolean
    Private _cleanfanartJpg As Boolean
    Private _cleanfolderJpg As Boolean
    Private _cleanmoviefanartJpg As Boolean
    Private _cleanmovieJpg As Boolean
    Private _cleanmovienameJpg As Boolean
    Private _cleanmovieNfo As Boolean
    Private _cleanmovieNfoB As Boolean
    Private _cleanmovieTbn As Boolean
    Private _cleanmovieTbnB As Boolean
    Private _cleanposterJpg As Boolean
    Private _cleanposterTbn As Boolean
    Private _cleanwhitelistexts As List(Of String)
    Private _cleanwhitelistvideo As Boolean
    Private _dashtrailer As Boolean
    Private _deletealltrailers As Boolean
    Private _displayallseason As Boolean
    Private _displaymissingepisodes As Boolean
    Private _displayyear As Boolean
    Private _downloadtrailers As Boolean
    Private _orderdefault As Enums.Ordering
    Private _emberModules As List(Of ModulesManager._XMLEmberModuleClass)
    Private _enableifoscan As Boolean
    Private _epfanartheight As Integer
    Private _epfanartQuality As Integer
    Private _epfanartsize As Enums.FanartSize
    Private _epfanartwidth As Integer
    Private _epfiltercustom As List(Of String)
    Private _episodedashfanart As Boolean
    Private _episodedotfanart As Boolean
    Private _episodefanartcol As Boolean
    Private _episodejpg As Boolean
    Private _episodedashthumbjpg As Boolean
    Private _episodenfocol As Boolean
    Private _episodepostercol As Boolean
    Private _episodetbn As Boolean
    Private _eplockplot As Boolean
    Private _eplockrating As Boolean
    Private _eplocktitle As Boolean
    Private _epposterheight As Integer
    Private _epposterQuality As Integer
    Private _epposterwidth As Integer
    Private _epproperCase As Boolean
    Private _etheight As Integer
    Private _etnative As Boolean
    Private _etpadding As Boolean
    Private _etwidth As Integer
    Private _expertcleaner As Boolean
    Private _externaltvdbapikey As String
    Private _fanartheight As Integer
    Private _fanartjpg As Boolean
    Private _fanartprefsizeonly As Boolean
    Private _fanartQuality As Integer
    Private _fanartsize As Enums.FanartSize
    Private _fanartwidth As Integer
    Private _field250 As Boolean
    Private _fieldcountry As Boolean
    Private _fieldcast As Boolean
    Private _fieldcert As Boolean
    Private _fieldcrew As Boolean
    Private _fielddirector As Boolean
    Private _fieldgenre As Boolean
    Private _fieldmpaa As Boolean
    Private _fieldmusic As Boolean
    Private _fieldoutline As Boolean
    Private _fieldplot As Boolean
    Private _fieldproducers As Boolean
    Private _fieldrating As Boolean
    Private _fieldrelease As Boolean
    Private _fieldruntime As Boolean
    Private _fieldstudio As Boolean
    Private _fieldtagline As Boolean
    Private _fieldtitle As Boolean
    Private _fieldtrailer As Boolean
    Private _fieldvotes As Boolean
    Private _fieldwriters As Boolean
    Private _fieldyear As Boolean
    Private _filterCustom As List(Of String)
    Private _filterPanelState As Boolean
    Private _flaglang As String
    Private _folderjpg As Boolean
    Private _forcetitle As String
    Private _fullcast As Boolean
    Private _fullcrew As Boolean
    Private _genrefilter As String
    Private _genrelimit As Integer
    Private _ignorelastscan As Boolean
    Private _infopanelanim As Boolean
    Private _infopanelstate As Integer
    Private _language As String
    Private _levtolerance As Integer
    Private _lockgenre As Boolean
    Private _lockoutline As Boolean
    Private _lockplot As Boolean
    Private _lockrating As Boolean
    Private _lockstudio As Boolean
    Private _locktagline As Boolean
    Private _locktitle As Boolean
    Private _locktrailer As Boolean
    Private _logerrors As Boolean
    Private _marknew As Boolean
    Private _marknewepisodes As Boolean
    Private _marknewshows As Boolean
    Private _metadatapertype As List(Of MetadataPerType)
    Private _missingfilterextras As Boolean
    Private _missingfilterfanart As Boolean
    Private _missingfilternfo As Boolean
    Private _missingfilterposter As Boolean
    Private _missingfiltersubs As Boolean
    Private _missingfiltertrailer As Boolean
    Private _movieextraCol As Boolean
    Private _moviefanartCol As Boolean
    Private _movieinfoCol As Boolean
    Private _moviejpg As Boolean
    Private _movienamedotfanartjpg As Boolean
    Private _movienamefanartjpg As Boolean
    Private _movienamejpg As Boolean
    Private _movienamedashposterjpg As Boolean
    Private _movienamemultionly As Boolean
    Private _movienamenfo As Boolean
    Private _movienametbn As Boolean
    Private _movienfo As Boolean
    Private _movieposterCol As Boolean
    Private _moviesubCol As Boolean
    Private _movietbn As Boolean
    Private _movietheme As String
    Private _movietrailerCol As Boolean
    Private _nodisplayfanart As Boolean
    Private _nodisplayposter As Boolean
    Private _noepfilters As Boolean
    Private _nofilterepisode As Boolean
    Private _nofilters As Boolean
    Private _nosaveimagestonfo As Boolean
    Private _noshowfilters As Boolean
    Private _nostackexts As List(Of String)
    Private _notokens As Boolean
    Private _onlytvimagesforselectedlangauge As Boolean
    Private _onlyvalueforcert As Boolean
    Private _outlineforplot As Boolean
    Private _overwriteallsPoster As Boolean
    Private _overwriteEpFanart As Boolean
    Private _overwriteEpPoster As Boolean
    Private _overwriteFanart As Boolean
    Private _overwritenfo As Boolean
    Private _overwritePoster As Boolean
    Private _overwriteSeasonFanart As Boolean
    Private _overwriteSeasonPoster As Boolean
    Private _overwriteShowFanart As Boolean
    Private _overwriteShowPoster As Boolean
    Private _overwritetrailer As Boolean
    Private _persistimagecache As Boolean
    Private _posterheight As Integer
    Private _posterjpg As Boolean
    Private _PosterPrefSizeOnly As Boolean
    Private _posterQuality As Integer
    Private _postersize As Enums.PosterSize
    Private _postertbn As Boolean
    Private _posterwidth As Integer
    Private _properCase As Boolean
    Private _proxycredentials As NetworkCredential
    Private _proxyport As Integer
    Private _proxyuri As String
    Private _resizeallsposter As Boolean
    Private _resizeepfanart As Boolean
    Private _resizeepposter As Boolean
    Private _resizefanart As Boolean
    Private _resizeposter As Boolean
    Private _resizeseasonfanart As Boolean
    Private _resizeseasonposter As Boolean
    Private _resizeshowfanart As Boolean
    Private _resizeshowposter As Boolean
    Private _runtimemask As String
    Private _scanmediainfo As Boolean
    Private _scanordermodify As Boolean
    Private _scantvmediainfo As Boolean
    Private _scmainstate As Integer
    Private _scshowstate As Integer
    Private _scseasonstate As Integer
    Private _scraperepactors As Boolean
    Private _scraperepaired As Boolean
    Private _scraperepcredits As Boolean
    Private _scraperepdirector As Boolean
    Private _scraperepepisode As Boolean
    Private _scraperepplot As Boolean
    Private _scrapereprating As Boolean
    Private _scraperepseason As Boolean
    Private _scrapereptitle As Boolean
    Private _scrapershowactors As Boolean
    Private _scrapershowegu As Boolean
    Private _scrapershowgenre As Boolean
    Private _scrapershowmpaa As Boolean
    Private _scrapershowplot As Boolean
    Private _scrapershowpremiered As Boolean
    Private _scrapershowrating As Boolean
    Private _scrapershowstudio As Boolean
    Private _scrapershowtitle As Boolean
    Private _seasonalljpg As Boolean
    Private _seasonalltbn As Boolean
    Private _seasonallposterjpg As Boolean
    Private _seasondashfanart As Boolean
    Private _seasonxxdashfanartjpg As Boolean
    Private _seasondotfanart As Boolean
    Private _seasonfanartcol As Boolean
    Private _seasonfanartheight As Integer
    Private _seasonfanartjpg As Boolean
    Private _seasonfanartQuality As Integer
    Private _seasonfanartsize As Enums.FanartSize
    Private _seasonfanartwidth As Integer
    Private _seasonfolderjpg As Boolean
    Private _seasonnamejpg As Boolean
    Private _seasonnametbn As Boolean
    Private _seasonpostercol As Boolean
    Private _seasonposterheight As Integer
    Private _seasonposterjpg As Boolean
    Private _seasonposterQuality As Integer
    Private _seasonpostersize As Enums.SeasonPosterType
    Private _seasonpostertbn As Boolean
    Private _seasonposterwidth As Integer
    Private _seasonx As Boolean
    Private _seasonxx As Boolean
    Private _seasonxxdashposterjpg As Boolean
    Private _sets As New List(Of String)
    Private _showbanner As Boolean
    Private _showbannertype As Enums.ShowBannerType
    Private _showdashfanart As Boolean
    Private _showdims As Boolean
    Private _showdotfanart As Boolean
    Private _showfanartcol As Boolean
    Private _showfanartheight As Integer
    Private _showfanartjpg As Boolean
    Private _showfanartQuality As Integer
    Private _showfanartsize As Enums.FanartSize
    Private _showfanartwidth As Integer
    Private _showfiltercustom As List(Of String)
    Private _showfolderjpg As Boolean
    Private _showinfopanelstate As Integer
    Private _showjpg As Boolean
    Private _showlockgenre As Boolean
    Private _showlockplot As Boolean
    Private _showlockrating As Boolean
    Private _showlockstudio As Boolean
    Private _showlocktitle As Boolean
    Private _shownfocol As Boolean
    Private _showpostercol As Boolean
    Private _showposterheight As Integer
    Private _showposterjpg As Boolean
    Private _showbannerjpg As Boolean
    Private _showposterQuality As Integer
    Private _showpostersize As Enums.PosterSize
    Private _showpostertbn As Boolean
    Private _showposterwidth As Integer
    Private _showproperCase As Boolean
    Private _showratingregion As String
    Private _showtbn As Boolean
    Private _singlescrapeimages As Boolean
    Private _singlescrapetrailer As Boolean
    Private _skiplessthan As Integer
    Private _skipstacksizecheck As Boolean
    Private _skiplessthanep As Integer
    Private _sortbeforescan As Boolean
    Private _sortpath As String
    Private _sorttokens As List(Of String)
    Private _sourcefromfolder As Boolean
    Private _trailerquality As Enums.TrailerQuality
    Private _trailertimeout As Integer
    Private _tvcleandb As Boolean
    Private _tvdblanguage As String
    Private _tvdblanguages As List(Of Containers.TVLanguage)
    Private _tvdbmirror As String
    Private _tveptheme As String
    Private _tvflaglang As String
    Private _tvignorelastscan As Boolean
    Private _tvmetadatapertype As List(Of MetadataPerType)
    Private _tvscanordermodify As Boolean
    Private _tvshowregexes As List(Of TVShowRegEx)
    Private _tvshowtheme As String
    Private _tvupdatetime As Enums.TVUpdateTime
    Private _updatertrailers As Boolean
    Private _updatertrailersnodownload As Boolean
    Private _usecertformpaa As Boolean
    Private _useetasfa As Boolean
    Private _useimgcache As Boolean
    Private _useimgcacheupdater As Boolean
    Private _usemiduration As Boolean
    Private _validexts As List(Of String)
    Private _version As String
    Private _videotsparent As Boolean
    Private _windowloc As New Point
    Private _windowsize As New Size
    Private _windowstate As FormWindowState
    Private _yamjsetscompatible As Boolean
    Private _username As String
    Private _password As String

#End Region 'Fields

#Region "Constructors"

    Public Sub New()
        Me.Clear()
    End Sub

#End Region 'Constructors

#Region "Enumerations"

    Public Enum EpRetrieve As Integer
        FromDirectory = 0
        FromFilename = 1
        FromSeasonResult = 2
    End Enum

#End Region 'Enumerations

#Region "Properties"

    Public Property ActorLimit() As Integer
        Get
            Return Me._actorlimit
        End Get
        Set(ByVal value As Integer)
            Me._actorlimit = value
        End Set
    End Property

    Public Property AllSPosterHeight() As Integer
        Get
            Return Me._allsposterheight
        End Get
        Set(ByVal value As Integer)
            Me._allsposterheight = value
        End Set
    End Property

    Public Property AllSPosterQuality() As Integer
        Get
            Return Me._allsposterQuality
        End Get
        Set(ByVal value As Integer)
            Me._allsposterQuality = value
        End Set
    End Property

    Public Property AllSPosterWidth() As Integer
        Get
            Return Me._allsposterwidth
        End Get
        Set(ByVal value As Integer)
            Me._allsposterwidth = value
        End Set
    End Property

    Public Property AllwaysDisplayGenresText() As Boolean
        Get
            Return Me._allwaysdisplaygenrestext
        End Get
        Set(ByVal value As Boolean)
            Me._allwaysdisplaygenrestext = value
        End Set
    End Property

    Public Property AlwaysGetEnglishTVImages() As Boolean
        Get
            Return Me._alwaysgetenglishtvimages
        End Get
        Set(ByVal value As Boolean)
            Me._alwaysgetenglishtvimages = value
        End Set
    End Property

    Public Property ClickScrape() As Boolean
        Get
            Return Me._clickscrape
        End Get
        Set(ByVal value As Boolean)
            Me._clickscrape = value
        End Set
    End Property

    Public Property AskCheckboxScrape() As Boolean
        Get
            Return Me._askcheckboxscrape
        End Get
        Set(ByVal value As Boolean)
            Me._askcheckboxscrape = value
        End Set
    End Property

    Public Property AutoBD() As Boolean
        Get
            Return Me._autobd
        End Get
        Set(ByVal value As Boolean)
            Me._autobd = value
        End Set
    End Property

    Public Property AutoDetectVTS() As Boolean
        Get
            Return Me._autodetectvts
        End Get
        Set(ByVal value As Boolean)
            Me._autodetectvts = value
        End Set
    End Property

    Public Property AutoET() As Boolean
        Get
            Return Me._autoET
        End Get
        Set(ByVal value As Boolean)
            Me._autoET = value
        End Set
    End Property

    Public Property AutoETSize() As Enums.FanartSize
        Get
            Return Me._autoETsize
        End Get
        Set(ByVal value As Enums.FanartSize)
            Me._autoETsize = value
        End Set
    End Property

    Public Property AutoThumbs() As Integer
        Get
            Return Me._autothumbs
        End Get
        Set(ByVal value As Integer)
            Me._autothumbs = value
        End Set
    End Property

    Public Property AutoThumbsNoSpoilers() As Boolean
        Get
            Return Me._autothumbnospoilers
        End Get
        Set(ByVal value As Boolean)
            Me._autothumbnospoilers = value
        End Set
    End Property

    Public Property BDPath() As String
        Get
            Return Me._bdpath
        End Get
        Set(ByVal value As String)
            Me._bdpath = value
        End Set
    End Property

    Public Property CastImagesOnly() As Boolean
        Get
            Return Me._castimagesonly
        End Get
        Set(ByVal value As Boolean)
            Me._castimagesonly = value
        End Set
    End Property

    Public Property CertificationLang() As String
        Get
            Return Me._certificationLang
        End Get
        Set(ByVal value As String)
            Me._certificationLang = value
        End Set
    End Property

    Public Property CheckUpdates() As Boolean
        Get
            Return Me._checkupdates
        End Get
        Set(ByVal value As Boolean)
            Me._checkupdates = value
        End Set
    End Property

    Public Property CleanDB() As Boolean
        Get
            Return Me._cleandb
        End Get
        Set(ByVal value As Boolean)
            Me._cleandb = value
        End Set
    End Property

    Public Property CleanDotFanartJPG() As Boolean
        Get
            Return Me._cleandotfanartJpg
        End Get
        Set(ByVal value As Boolean)
            Me._cleandotfanartJpg = value
        End Set
    End Property

    Public Property CleanExtraThumbs() As Boolean
        Get
            Return Me._cleanextrathumbs
        End Get
        Set(ByVal value As Boolean)
            Me._cleanextrathumbs = value
        End Set
    End Property

    Public Property CleanFanartJPG() As Boolean
        Get
            Return Me._cleanfanartJpg
        End Get
        Set(ByVal value As Boolean)
            Me._cleanfanartJpg = value
        End Set
    End Property

    Public Property CleanFolderJPG() As Boolean
        Get
            Return Me._cleanfolderJpg
        End Get
        Set(ByVal value As Boolean)
            Me._cleanfolderJpg = value
        End Set
    End Property

    Public Property CleanMovieFanartJPG() As Boolean
        Get
            Return Me._cleanmoviefanartJpg
        End Get
        Set(ByVal value As Boolean)
            Me._cleanmoviefanartJpg = value
        End Set
    End Property

    Public Property CleanMovieJPG() As Boolean
        Get
            Return Me._cleanmovieJpg
        End Get
        Set(ByVal value As Boolean)
            Me._cleanmovieJpg = value
        End Set
    End Property

    Public Property CleanMovieNameJPG() As Boolean
        Get
            Return Me._cleanmovienameJpg
        End Get
        Set(ByVal value As Boolean)
            Me._cleanmovienameJpg = value
        End Set
    End Property

    Public Property CleanMovieNFO() As Boolean
        Get
            Return Me._cleanmovieNfo
        End Get
        Set(ByVal value As Boolean)
            Me._cleanmovieNfo = value
        End Set
    End Property

    Public Property CleanMovieNFOB() As Boolean
        Get
            Return Me._cleanmovieNfoB
        End Get
        Set(ByVal value As Boolean)
            Me._cleanmovieNfoB = value
        End Set
    End Property

    Public Property CleanMovieTBN() As Boolean
        Get
            Return _cleanmovieTbn
        End Get
        Set(ByVal value As Boolean)
            _cleanmovieTbn = value
        End Set
    End Property

    Public Property CleanMovieTBNB() As Boolean
        Get
            Return Me._cleanmovieTbnB
        End Get
        Set(ByVal value As Boolean)
            Me._cleanmovieTbnB = value
        End Set
    End Property

    Public Property CleanPosterJPG() As Boolean
        Get
            Return Me._cleanposterJpg
        End Get
        Set(ByVal value As Boolean)
            Me._cleanposterJpg = value
        End Set
    End Property

    Public Property CleanPosterTBN() As Boolean
        Get
            Return Me._cleanposterTbn
        End Get
        Set(ByVal value As Boolean)
            Me._cleanposterTbn = value
        End Set
    End Property

    Public Property CleanWhitelistExts() As List(Of String)
        Get
            Return Me._cleanwhitelistexts
        End Get
        Set(ByVal value As List(Of String))
            Me._cleanwhitelistexts = value
        End Set
    End Property

    Public Property CleanWhitelistVideo() As Boolean
        Get
            Return Me._cleanwhitelistvideo
        End Get
        Set(ByVal value As Boolean)
            Me._cleanwhitelistvideo = value
        End Set
    End Property

    Public Property DashTrailer() As Boolean
        Get
            Return Me._dashtrailer
        End Get
        Set(ByVal value As Boolean)
            Me._dashtrailer = value
        End Set
    End Property

    Public Property DeleteAllTrailers() As Boolean
        Get
            Return Me._deletealltrailers
        End Get
        Set(ByVal value As Boolean)
            Me._deletealltrailers = value
        End Set
    End Property

    Public Property DisplayAllSeason() As Boolean
        Get
            Return Me._displayallseason
        End Get
        Set(ByVal value As Boolean)
            Me._displayallseason = value
        End Set
    End Property

    Public Property DisplayMissingEpisodes() As Boolean
        Get
            Return Me._displaymissingepisodes
        End Get
        Set(ByVal value As Boolean)
            Me._displaymissingepisodes = value
        End Set
    End Property

    Public Property DisplayYear() As Boolean
        Get
            Return Me._displayyear
        End Get
        Set(ByVal value As Boolean)
            Me._displayyear = value
        End Set
    End Property

    Public Property DownloadTrailers() As Boolean
        Get
            Return Me._downloadtrailers
        End Get
        Set(ByVal value As Boolean)
            Me._downloadtrailers = value
        End Set
    End Property

    Public Property OrderDefault() As Enums.Ordering
        Get
            Return Me._orderdefault
        End Get
        Set(ByVal value As Enums.Ordering)
            Me._orderdefault = value
        End Set
    End Property

    <XmlArray("EmberModules")> _
    <XmlArrayItem("Module")> _
    Public Property EmberModules() As List(Of ModulesManager._XMLEmberModuleClass)
        Get
            Return Me._emberModules
        End Get
        Set(ByVal value As List(Of ModulesManager._XMLEmberModuleClass))
            Me._emberModules = value
        End Set
    End Property

    Public Property EnableIFOScan() As Boolean
        Get
            Return Me._enableifoscan
        End Get
        Set(ByVal value As Boolean)
            Me._enableifoscan = value
        End Set
    End Property

    Public Property EpFanartHeight() As Integer
        Get
            Return Me._epfanartheight
        End Get
        Set(ByVal value As Integer)
            Me._epfanartheight = value
        End Set
    End Property

    Public Property EpFanartQuality() As Integer
        Get
            Return Me._epfanartQuality
        End Get
        Set(ByVal value As Integer)
            Me._epfanartQuality = value
        End Set
    End Property

    Public Property EpFanartWidth() As Integer
        Get
            Return Me._epfanartwidth
        End Get
        Set(ByVal value As Integer)
            Me._epfanartwidth = value
        End Set
    End Property

    Public Property EpFilterCustom() As List(Of String)
        Get
            Return Me._epfiltercustom
        End Get
        Set(ByVal value As List(Of String))
            Me._epfiltercustom = value
        End Set
    End Property

    Public Property EpisodeDashFanart() As Boolean
        Get
            Return Me._episodedashfanart
        End Get
        Set(ByVal value As Boolean)
            Me._episodedashfanart = value
        End Set
    End Property

    Public Property EpisodeDotFanart() As Boolean
        Get
            Return Me._episodedotfanart
        End Get
        Set(ByVal value As Boolean)
            Me._episodedotfanart = value
        End Set
    End Property

    Public Property EpisodeFanartCol() As Boolean
        Get
            Return Me._episodefanartcol
        End Get
        Set(ByVal value As Boolean)
            Me._episodefanartcol = value
        End Set
    End Property

    Public Property EpisodeJPG() As Boolean
        Get
            Return Me._episodejpg
        End Get
        Set(ByVal value As Boolean)
            Me._episodejpg = value
        End Set
    End Property

    Public Property EpisodeDashThumbJPG() As Boolean
        Get
            Return Me._episodedashthumbjpg
        End Get
        Set(ByVal value As Boolean)
            Me._episodedashthumbjpg = value
        End Set
    End Property

    Public Property EpisodeNfoCol() As Boolean
        Get
            Return Me._episodenfocol
        End Get
        Set(ByVal value As Boolean)
            Me._episodenfocol = value
        End Set
    End Property

    Public Property EpisodePosterCol() As Boolean
        Get
            Return Me._episodepostercol
        End Get
        Set(ByVal value As Boolean)
            Me._episodepostercol = value
        End Set
    End Property

    Public Property EpisodeTBN() As Boolean
        Get
            Return Me._episodetbn
        End Get
        Set(ByVal value As Boolean)
            Me._episodetbn = value
        End Set
    End Property

    Public Property EpLockPlot() As Boolean
        Get
            Return Me._eplockplot
        End Get
        Set(ByVal value As Boolean)
            Me._eplockplot = value
        End Set
    End Property

    Public Property EpLockRating() As Boolean
        Get
            Return Me._eplockrating
        End Get
        Set(ByVal value As Boolean)
            Me._eplockrating = value
        End Set
    End Property

    Public Property EpLockTitle() As Boolean
        Get
            Return Me._eplocktitle
        End Get
        Set(ByVal value As Boolean)
            Me._eplocktitle = value
        End Set
    End Property

    Public Property EpPosterHeight() As Integer
        Get
            Return Me._epposterheight
        End Get
        Set(ByVal value As Integer)
            Me._epposterheight = value
        End Set
    End Property

    Public Property EpPosterQuality() As Integer
        Get
            Return Me._epposterQuality
        End Get
        Set(ByVal value As Integer)
            Me._epposterQuality = value
        End Set
    End Property

    Public Property EpPosterWidth() As Integer
        Get
            Return Me._epposterwidth
        End Get
        Set(ByVal value As Integer)
            Me._epposterwidth = value
        End Set
    End Property

    Public Property EpProperCase() As Boolean
        Get
            Return Me._epproperCase
        End Get
        Set(ByVal value As Boolean)
            Me._epproperCase = value
        End Set
    End Property

    Public Property ETHeight() As Integer
        Get
            Return Me._etheight
        End Get
        Set(ByVal value As Integer)
            Me._etheight = value
        End Set
    End Property

    Public Property ETNative() As Boolean
        Get
            Return Me._etnative
        End Get
        Set(ByVal value As Boolean)
            Me._etnative = value
        End Set
    End Property

    Public Property ETPadding() As Boolean
        Get
            Return Me._etpadding
        End Get
        Set(ByVal value As Boolean)
            Me._etpadding = value
        End Set
    End Property

    Public Property ETWidth() As Integer
        Get
            Return Me._etwidth
        End Get
        Set(ByVal value As Integer)
            Me._etwidth = value
        End Set
    End Property

    Public Property ExpertCleaner() As Boolean
        Get
            Return Me._expertcleaner
        End Get
        Set(ByVal value As Boolean)
            Me._expertcleaner = value
        End Set
    End Property

    Public Property ExternalTVDBAPIKey() As String
        Get
            Return Me._externaltvdbapikey
        End Get
        Set(ByVal value As String)
            Me._externaltvdbapikey = value
        End Set
    End Property

    Public Property FanartHeight() As Integer
        Get
            Return Me._fanartheight
        End Get
        Set(ByVal value As Integer)
            Me._fanartheight = value
        End Set
    End Property

    Public Property FanartJPG() As Boolean
        Get
            Return Me._fanartjpg
        End Get
        Set(ByVal value As Boolean)
            Me._fanartjpg = value
        End Set
    End Property

    Public Property FanartPrefSizeOnly() As Boolean
        Get
            Return Me._fanartprefsizeonly
        End Get
        Set(ByVal value As Boolean)
            Me._fanartprefsizeonly = value
        End Set
    End Property

    Public Property FanartQuality() As Integer
        Get
            Return Me._fanartQuality
        End Get
        Set(ByVal value As Integer)
            Me._fanartQuality = value
        End Set
    End Property

    Public Property FanartWidth() As Integer
        Get
            Return Me._fanartwidth
        End Get
        Set(ByVal value As Integer)
            Me._fanartwidth = value
        End Set
    End Property

    Public Property Field250() As Boolean
        Get
            Return Me._field250
        End Get
        Set(ByVal value As Boolean)
            Me._field250 = value
        End Set
    End Property

    Public Property FieldCountry() As Boolean
        Get
            Return Me._fieldcountry
        End Get
        Set(ByVal value As Boolean)
            Me._fieldcountry = value
        End Set
    End Property

    Public Property FieldCast() As Boolean
        Get
            Return Me._fieldcast
        End Get
        Set(ByVal value As Boolean)
            Me._fieldcast = value
        End Set
    End Property

    Public Property FieldCert() As Boolean
        Get
            Return Me._fieldcert
        End Get
        Set(ByVal value As Boolean)
            Me._fieldcert = value
        End Set
    End Property

    Public Property FieldCrew() As Boolean
        Get
            Return Me._fieldcrew
        End Get
        Set(ByVal value As Boolean)
            Me._fieldcrew = value
        End Set
    End Property

    Public Property FieldDirector() As Boolean
        Get
            Return Me._fielddirector
        End Get
        Set(ByVal value As Boolean)
            Me._fielddirector = value
        End Set
    End Property

    Public Property FieldGenre() As Boolean
        Get
            Return Me._fieldgenre
        End Get
        Set(ByVal value As Boolean)
            Me._fieldgenre = value
        End Set
    End Property

    Public Property FieldMPAA() As Boolean
        Get
            Return Me._fieldmpaa
        End Get
        Set(ByVal value As Boolean)
            Me._fieldmpaa = value
        End Set
    End Property

    Public Property FieldMusic() As Boolean
        Get
            Return Me._fieldmusic
        End Get
        Set(ByVal value As Boolean)
            Me._fieldmusic = value
        End Set
    End Property

    Public Property FieldOutline() As Boolean
        Get
            Return Me._fieldoutline
        End Get
        Set(ByVal value As Boolean)
            Me._fieldoutline = value
        End Set
    End Property

    Public Property FieldPlot() As Boolean
        Get
            Return Me._fieldplot
        End Get
        Set(ByVal value As Boolean)
            Me._fieldplot = value
        End Set
    End Property

    Public Property FieldProducers() As Boolean
        Get
            Return Me._fieldproducers
        End Get
        Set(ByVal value As Boolean)
            Me._fieldproducers = value
        End Set
    End Property

    Public Property FieldRating() As Boolean
        Get
            Return Me._fieldrating
        End Get
        Set(ByVal value As Boolean)
            Me._fieldrating = value
        End Set
    End Property

    Public Property FieldRelease() As Boolean
        Get
            Return Me._fieldrelease
        End Get
        Set(ByVal value As Boolean)
            Me._fieldrelease = value
        End Set
    End Property

    Public Property FieldRuntime() As Boolean
        Get
            Return Me._fieldruntime
        End Get
        Set(ByVal value As Boolean)
            Me._fieldruntime = value
        End Set
    End Property

    Public Property FieldStudio() As Boolean
        Get
            Return Me._fieldstudio
        End Get
        Set(ByVal value As Boolean)
            Me._fieldstudio = value
        End Set
    End Property

    Public Property FieldTagline() As Boolean
        Get
            Return Me._fieldtagline
        End Get
        Set(ByVal value As Boolean)
            Me._fieldtagline = value
        End Set
    End Property

    Public Property FieldTitle() As Boolean
        Get
            Return Me._fieldtitle
        End Get
        Set(ByVal value As Boolean)
            Me._fieldtitle = value
        End Set
    End Property

    Public Property FieldTrailer() As Boolean
        Get
            Return Me._fieldtrailer
        End Get
        Set(ByVal value As Boolean)
            Me._fieldtrailer = value
        End Set
    End Property

    Public Property FieldVotes() As Boolean
        Get
            Return Me._fieldvotes
        End Get
        Set(ByVal value As Boolean)
            Me._fieldvotes = value
        End Set
    End Property

    Public Property FieldWriters() As Boolean
        Get
            Return Me._fieldwriters
        End Get
        Set(ByVal value As Boolean)
            Me._fieldwriters = value
        End Set
    End Property

    Public Property FieldYear() As Boolean
        Get
            Return Me._fieldyear
        End Get
        Set(ByVal value As Boolean)
            Me._fieldyear = value
        End Set
    End Property

    Public Property FilterCustom() As List(Of String)
        Get
            Return Me._filterCustom
        End Get
        Set(ByVal value As List(Of String))
            Me._filterCustom = value
        End Set
    End Property

    Public Property FilterPanelState() As Boolean
        Get
            Return Me._filterPanelState
        End Get
        Set(ByVal value As Boolean)
            Me._filterPanelState = value
        End Set
    End Property

    Public Property FlagLang() As String
        Get
            Return Me._flaglang
        End Get
        Set(ByVal value As String)
            Me._flaglang = value
        End Set
    End Property

    Public Property FolderJPG() As Boolean
        Get
            Return Me._folderjpg
        End Get
        Set(ByVal value As Boolean)
            Me._folderjpg = value
        End Set
    End Property

    Public Property ForceTitle() As String
        Get
            Return Me._forcetitle
        End Get
        Set(ByVal value As String)
            Me._forcetitle = value
        End Set
    End Property

    Public Property FullCast() As Boolean
        Get
            Return Me._fullcast
        End Get
        Set(ByVal value As Boolean)
            Me._fullcast = value
        End Set
    End Property

    Public Property FullCrew() As Boolean
        Get
            Return Me._fullcrew
        End Get
        Set(ByVal value As Boolean)
            Me._fullcrew = value
        End Set
    End Property

    Public Property GenreFilter() As String
        Get
            Return Me._genrefilter
        End Get
        Set(ByVal value As String)
            Me._genrefilter = value
        End Set
    End Property

    Public Property GenreLimit() As Integer
        Get
            Return Me._genrelimit
        End Get
        Set(ByVal value As Integer)
            Me._genrelimit = value
        End Set
    End Property

    Public Property IgnoreLastScan() As Boolean
        Get
            Return Me._ignorelastscan
        End Get
        Set(ByVal value As Boolean)
            Me._ignorelastscan = value
        End Set
    End Property

    Public Property InfoPanelAnim() As Boolean
        Get
            Return Me._infopanelanim
        End Get
        Set(ByVal value As Boolean)
            Me._infopanelanim = value
        End Set
    End Property

    Public Property InfoPanelState() As Integer
        Get
            Return Me._infopanelstate
        End Get
        Set(ByVal value As Integer)
            Me._infopanelstate = value
        End Set
    End Property

    Public Property IsAllSBanner() As Boolean
        Get
            Return Me._allsbanner
        End Get
        Set(ByVal value As Boolean)
            Me._allsbanner = value
        End Set
    End Property

    Public Property IsShowBanner() As Boolean
        Get
            Return Me._showbanner
        End Get
        Set(ByVal value As Boolean)
            Me._showbanner = value
        End Set
    End Property

    Public Property Language() As String
        Get
            Return Me._language
        End Get
        Set(ByVal value As String)
            Me._language = value
        End Set
    End Property

    Public Property LevTolerance() As Integer
        Get
            Return Me._levtolerance
        End Get
        Set(ByVal value As Integer)
            Me._levtolerance = value
        End Set
    End Property

    Public Property LockGenre() As Boolean
        Get
            Return Me._lockgenre
        End Get
        Set(ByVal value As Boolean)
            Me._lockgenre = value
        End Set
    End Property

    Public Property LockOutline() As Boolean
        Get
            Return Me._lockoutline
        End Get
        Set(ByVal value As Boolean)
            Me._lockoutline = value
        End Set
    End Property

    Public Property LockPlot() As Boolean
        Get
            Return Me._lockplot
        End Get
        Set(ByVal value As Boolean)
            Me._lockplot = value
        End Set
    End Property

    Public Property LockRating() As Boolean
        Get
            Return Me._lockrating
        End Get
        Set(ByVal value As Boolean)
            Me._lockrating = value
        End Set
    End Property

    Public Property LockStudio() As Boolean
        Get
            Return Me._lockstudio
        End Get
        Set(ByVal value As Boolean)
            Me._lockstudio = value
        End Set
    End Property

    Public Property LockTagline() As Boolean
        Get
            Return Me._locktagline
        End Get
        Set(ByVal value As Boolean)
            Me._locktagline = value
        End Set
    End Property

    Public Property LockTitle() As Boolean
        Get
            Return Me._locktitle
        End Get
        Set(ByVal value As Boolean)
            Me._locktitle = value
        End Set
    End Property

    Public Property LockTrailer() As Boolean
        Get
            Return Me._locktrailer
        End Get
        Set(ByVal value As Boolean)
            Me._locktrailer = value
        End Set
    End Property

    Public Property LogErrors() As Boolean
        Get
            Return Me._logerrors
        End Get
        Set(ByVal value As Boolean)
            Me._logerrors = value
        End Set
    End Property

    Public Property MarkNew() As Boolean
        Get
            Return Me._marknew
        End Get
        Set(ByVal value As Boolean)
            Me._marknew = value
        End Set
    End Property

    Public Property MarkNewEpisodes() As Boolean
        Get
            Return Me._marknewepisodes
        End Get
        Set(ByVal value As Boolean)
            Me._marknewepisodes = value
        End Set
    End Property

    Public Property MarkNewShows() As Boolean
        Get
            Return Me._marknewshows
        End Get
        Set(ByVal value As Boolean)
            Me._marknewshows = value
        End Set
    End Property

    Public Property MetadataPerFileType() As List(Of MetadataPerType)
        Get
            Return Me._metadatapertype
        End Get
        Set(ByVal value As List(Of MetadataPerType))
            Me._metadatapertype = value
        End Set
    End Property

    Public Property MissingFilterExtras() As Boolean
        Get
            Return Me._missingfilterextras
        End Get
        Set(ByVal value As Boolean)
            Me._missingfilterextras = value
        End Set
    End Property

    Public Property MissingFilterFanart() As Boolean
        Get
            Return Me._missingfilterfanart
        End Get
        Set(ByVal value As Boolean)
            Me._missingfilterfanart = value
        End Set
    End Property

    Public Property MissingFilterNFO() As Boolean
        Get
            Return Me._missingfilternfo
        End Get
        Set(ByVal value As Boolean)
            Me._missingfilternfo = value
        End Set
    End Property

    Public Property MissingFilterPoster() As Boolean
        Get
            Return Me._missingfilterposter
        End Get
        Set(ByVal value As Boolean)
            Me._missingfilterposter = value
        End Set
    End Property

    Public Property MissingFilterSubs() As Boolean
        Get
            Return Me._missingfiltersubs
        End Get
        Set(ByVal value As Boolean)
            Me._missingfiltersubs = value
        End Set
    End Property

    Public Property MissingFilterTrailer() As Boolean
        Get
            Return Me._missingfiltertrailer
        End Get
        Set(ByVal value As Boolean)
            Me._missingfiltertrailer = value
        End Set
    End Property

    Public Property MovieExtraCol() As Boolean
        Get
            Return Me._movieextraCol
        End Get
        Set(ByVal value As Boolean)
            Me._movieextraCol = value
        End Set
    End Property

    Public Property MovieFanartCol() As Boolean
        Get
            Return Me._moviefanartCol
        End Get
        Set(ByVal value As Boolean)
            Me._moviefanartCol = value
        End Set
    End Property

    Public Property MovieInfoCol() As Boolean
        Get
            Return Me._movieinfoCol
        End Get
        Set(ByVal value As Boolean)
            Me._movieinfoCol = value
        End Set
    End Property

    Public Property MovieJPG() As Boolean
        Get
            Return Me._moviejpg
        End Get
        Set(ByVal value As Boolean)
            Me._moviejpg = value
        End Set
    End Property

    Public Property MovieNameDotFanartJPG() As Boolean
        Get
            Return Me._movienamedotfanartjpg
        End Get
        Set(ByVal value As Boolean)
            Me._movienamedotfanartjpg = value
        End Set
    End Property

    Public Property MovieNameFanartJPG() As Boolean
        Get
            Return Me._movienamefanartjpg
        End Get
        Set(ByVal value As Boolean)
            Me._movienamefanartjpg = value
        End Set
    End Property

    Public Property MovieNameJPG() As Boolean
        Get
            Return Me._movienamejpg
        End Get
        Set(ByVal value As Boolean)
            Me._movienamejpg = value
        End Set
    End Property

    Public Property MovieNameDashPosterJPG() As Boolean
        Get
            Return Me._movienamedashposterjpg
        End Get
        Set(ByVal value As Boolean)
            Me._movienamedashposterjpg = value
        End Set
    End Property

    Public Property MovieNameMultiOnly() As Boolean
        Get
            Return Me._movienamemultionly
        End Get
        Set(ByVal value As Boolean)
            Me._movienamemultionly = value
        End Set
    End Property

    Public Property MovieNameNFO() As Boolean
        Get
            Return Me._movienamenfo
        End Get
        Set(ByVal value As Boolean)
            Me._movienamenfo = value
        End Set
    End Property

    Public Property MovieNameTBN() As Boolean
        Get
            Return Me._movienametbn
        End Get
        Set(ByVal value As Boolean)
            Me._movienametbn = value
        End Set
    End Property

    Public Property MovieNFO() As Boolean
        Get
            Return Me._movienfo
        End Get
        Set(ByVal value As Boolean)
            Me._movienfo = value
        End Set
    End Property

    Public Property MoviePosterCol() As Boolean
        Get
            Return Me._movieposterCol
        End Get
        Set(ByVal value As Boolean)
            Me._movieposterCol = value
        End Set
    End Property

    Public Property MovieSubCol() As Boolean
        Get
            Return Me._moviesubCol
        End Get
        Set(ByVal value As Boolean)
            Me._moviesubCol = value
        End Set
    End Property

    Public Property MovieTBN() As Boolean
        Get
            Return Me._movietbn
        End Get
        Set(ByVal value As Boolean)
            Me._movietbn = value
        End Set
    End Property

    Public Property MovieTheme() As String
        Get
            Return Me._movietheme
        End Get
        Set(ByVal value As String)
            Me._movietheme = value
        End Set
    End Property

    Public Property MovieTrailerCol() As Boolean
        Get
            Return Me._movietrailerCol
        End Get
        Set(ByVal value As Boolean)
            Me._movietrailerCol = value
        End Set
    End Property

    Public Property NoDisplayFanart() As Boolean
        Get
            Return Me._nodisplayfanart
        End Get
        Set(ByVal value As Boolean)
            Me._nodisplayfanart = value
        End Set
    End Property

    Public Property NoDisplayPoster() As Boolean
        Get
            Return Me._nodisplayposter
        End Get
        Set(ByVal value As Boolean)
            Me._nodisplayposter = value
        End Set
    End Property

    Public Property NoEpFilters() As Boolean
        Get
            Return Me._noepfilters
        End Get
        Set(ByVal value As Boolean)
            Me._noepfilters = value
        End Set
    End Property

    Public Property NoFilterEpisode() As Boolean
        Get
            Return Me._nofilterepisode
        End Get
        Set(ByVal value As Boolean)
            Me._nofilterepisode = value
        End Set
    End Property

    Public Property NoFilters() As Boolean
        Get
            Return Me._nofilters
        End Get
        Set(ByVal value As Boolean)
            Me._nofilters = value
        End Set
    End Property

    Public Property NoSaveImagesToNfo() As Boolean
        Get
            Return Me._nosaveimagestonfo
        End Get
        Set(ByVal value As Boolean)
            Me._nosaveimagestonfo = value
        End Set
    End Property

    Public Property NoShowFilters() As Boolean
        Get
            Return Me._noshowfilters
        End Get
        Set(ByVal value As Boolean)
            Me._noshowfilters = value
        End Set
    End Property

    Public Property NoStackExts() As List(Of String)
        Get
            Return Me._nostackexts
        End Get
        Set(ByVal value As List(Of String))
            Me._nostackexts = value
        End Set
    End Property

    Public Property NoTokens() As Boolean
        Get
            Return Me._notokens
        End Get
        Set(ByVal value As Boolean)
            Me._notokens = value
        End Set
    End Property

    Public Property OnlyGetTVImagesForSelectedLanguage() As Boolean
        Get
            Return Me._onlytvimagesforselectedlangauge
        End Get
        Set(ByVal value As Boolean)
            Me._onlytvimagesforselectedlangauge = value
        End Set
    End Property

    Public Property OnlyValueForCert() As Boolean
        Get
            Return Me._onlyvalueforcert
        End Get
        Set(ByVal value As Boolean)
            Me._onlyvalueforcert = value
        End Set
    End Property

    Public Property OutlineForPlot() As Boolean
        Get
            Return Me._outlineforplot
        End Get
        Set(ByVal value As Boolean)
            Me._outlineforplot = value
        End Set
    End Property

    Public Property OverwriteAllSPoster() As Boolean
        Get
            Return Me._overwriteallsPoster
        End Get
        Set(ByVal value As Boolean)
            Me._overwriteallsPoster = value
        End Set
    End Property

    Public Property OverwriteEpFanart() As Boolean
        Get
            Return Me._overwriteEpFanart
        End Get
        Set(ByVal value As Boolean)
            Me._overwriteEpFanart = value
        End Set
    End Property

    Public Property OverwriteEpPoster() As Boolean
        Get
            Return Me._overwriteEpPoster
        End Get
        Set(ByVal value As Boolean)
            Me._overwriteEpPoster = value
        End Set
    End Property

    Public Property OverwriteFanart() As Boolean
        Get
            Return Me._overwriteFanart
        End Get
        Set(ByVal value As Boolean)
            Me._overwriteFanart = value
        End Set
    End Property

    Public Property OverwriteNfo() As Boolean
        Get
            Return Me._overwritenfo
        End Get
        Set(ByVal value As Boolean)
            Me._overwritenfo = value
        End Set
    End Property

    Public Property OverwritePoster() As Boolean
        Get
            Return Me._overwritePoster
        End Get
        Set(ByVal value As Boolean)
            Me._overwritePoster = value
        End Set
    End Property

    Public Property OverwriteSeasonFanart() As Boolean
        Get
            Return Me._overwriteSeasonFanart
        End Get
        Set(ByVal value As Boolean)
            Me._overwriteSeasonFanart = value
        End Set
    End Property

    Public Property OverwriteSeasonPoster() As Boolean
        Get
            Return Me._overwriteSeasonPoster
        End Get
        Set(ByVal value As Boolean)
            Me._overwriteSeasonPoster = value
        End Set
    End Property

    Public Property OverwriteShowFanart() As Boolean
        Get
            Return Me._overwriteShowFanart
        End Get
        Set(ByVal value As Boolean)
            Me._overwriteShowFanart = value
        End Set
    End Property

    Public Property OverwriteShowPoster() As Boolean
        Get
            Return Me._overwriteShowPoster
        End Get
        Set(ByVal value As Boolean)
            Me._overwriteShowPoster = value
        End Set
    End Property

    Public Property OverwriteTrailer() As Boolean
        Get
            Return Me._overwritetrailer
        End Get
        Set(ByVal value As Boolean)
            Me._overwritetrailer = value
        End Set
    End Property

    Public Property PersistImgCache() As Boolean
        Get
            Return Me._persistimagecache
        End Get
        Set(ByVal value As Boolean)
            Me._persistimagecache = value
        End Set
    End Property

    Public Property PosterHeight() As Integer
        Get
            Return Me._posterheight
        End Get
        Set(ByVal value As Integer)
            Me._posterheight = value
        End Set
    End Property

    Public Property PosterJPG() As Boolean
        Get
            Return Me._posterjpg
        End Get
        Set(ByVal value As Boolean)
            Me._posterjpg = value
        End Set
    End Property

    Public Property PosterPrefSizeOnly() As Boolean
        Get
            Return Me._PosterPrefSizeOnly
        End Get
        Set(ByVal value As Boolean)
            Me._PosterPrefSizeOnly = value
        End Set
    End Property

    Public Property PosterQuality() As Integer
        Get
            Return Me._posterQuality
        End Get
        Set(ByVal value As Integer)
            Me._posterQuality = value
        End Set
    End Property

    Public Property PosterTBN() As Boolean
        Get
            Return Me._postertbn
        End Get
        Set(ByVal value As Boolean)
            Me._postertbn = value
        End Set
    End Property

    Public Property PosterWidth() As Integer
        Get
            Return Me._posterwidth
        End Get
        Set(ByVal value As Integer)
            Me._posterwidth = value
        End Set
    End Property

    Public Property PreferredAllSBannerType() As Enums.ShowBannerType
        Get
            Return Me._allsbannertype
        End Get
        Set(ByVal value As Enums.ShowBannerType)
            Me._allsbannertype = value
        End Set
    End Property

    Public Property PreferredAllSPosterSize() As Enums.PosterSize
        Get
            Return Me._allspostersize
        End Get
        Set(ByVal value As Enums.PosterSize)
            Me._allspostersize = value
        End Set
    End Property

    Public Property PreferredEpFanartSize() As Enums.FanartSize
        Get
            Return Me._epfanartsize
        End Get
        Set(ByVal value As Enums.FanartSize)
            Me._epfanartsize = value
        End Set
    End Property

    Public Property PreferredFanartSize() As Enums.FanartSize
        Get
            Return Me._fanartsize
        End Get
        Set(ByVal value As Enums.FanartSize)
            Me._fanartsize = value
        End Set
    End Property

    Public Property PreferredPosterSize() As Enums.PosterSize
        Get
            Return Me._postersize
        End Get
        Set(ByVal value As Enums.PosterSize)
            Me._postersize = value
        End Set
    End Property

    Public Property PreferredSeasonFanartSize() As Enums.FanartSize
        Get
            Return Me._seasonfanartsize
        End Get
        Set(ByVal value As Enums.FanartSize)
            Me._seasonfanartsize = value
        End Set
    End Property

    Public Property PreferredSeasonPosterSize() As Enums.SeasonPosterType
        Get
            Return Me._seasonpostersize
        End Get
        Set(ByVal value As Enums.SeasonPosterType)
            Me._seasonpostersize = value
        End Set
    End Property

    Public Property PreferredShowBannerType() As Enums.ShowBannerType
        Get
            Return Me._showbannertype
        End Get
        Set(ByVal value As Enums.ShowBannerType)
            Me._showbannertype = value
        End Set
    End Property

    Public Property PreferredShowFanartSize() As Enums.FanartSize
        Get
            Return Me._showfanartsize
        End Get
        Set(ByVal value As Enums.FanartSize)
            Me._showfanartsize = value
        End Set
    End Property

    Public Property PreferredShowPosterSize() As Enums.PosterSize
        Get
            Return Me._showpostersize
        End Get
        Set(ByVal value As Enums.PosterSize)
            Me._showpostersize = value
        End Set
    End Property

    Public Property PreferredTrailerQuality() As Enums.TrailerQuality
        Get
            Return Me._trailerquality
        End Get
        Set(ByVal value As Enums.TrailerQuality)
            Me._trailerquality = value
        End Set
    End Property

    Public Property ProperCase() As Boolean
        Get
            Return Me._properCase
        End Get
        Set(ByVal value As Boolean)
            Me._properCase = value
        End Set
    End Property

    Public Property ProxyCreds() As NetworkCredential
        Get
            Return Me._proxycredentials
        End Get
        Set(ByVal value As NetworkCredential)
            Me._proxycredentials = value
        End Set
    End Property

    Public Property ProxyPort() As Integer
        Get
            Return Me._proxyport
        End Get
        Set(ByVal value As Integer)
            Me._proxyport = value
        End Set
    End Property

    Public Property ProxyURI() As String
        Get
            Return Me._proxyuri
        End Get
        Set(ByVal value As String)
            Me._proxyuri = value
        End Set
    End Property

    Public Property ResizeAllSPoster() As Boolean
        Get
            Return Me._resizeallsposter
        End Get
        Set(ByVal value As Boolean)
            Me._resizeallsposter = value
        End Set
    End Property

    Public Property ResizeEpFanart() As Boolean
        Get
            Return Me._resizeepfanart
        End Get
        Set(ByVal value As Boolean)
            Me._resizeepfanart = value
        End Set
    End Property

    Public Property ResizeEpPoster() As Boolean
        Get
            Return Me._resizeepposter
        End Get
        Set(ByVal value As Boolean)
            Me._resizeepposter = value
        End Set
    End Property

    Public Property ResizeFanart() As Boolean
        Get
            Return Me._resizefanart
        End Get
        Set(ByVal value As Boolean)
            Me._resizefanart = value
        End Set
    End Property

    Public Property ResizePoster() As Boolean
        Get
            Return Me._resizeposter
        End Get
        Set(ByVal value As Boolean)
            Me._resizeposter = value
        End Set
    End Property

    Public Property ResizeSeasonFanart() As Boolean
        Get
            Return Me._resizeseasonfanart
        End Get
        Set(ByVal value As Boolean)
            Me._resizeseasonfanart = value
        End Set
    End Property

    Public Property ResizeSeasonPoster() As Boolean
        Get
            Return Me._resizeseasonposter
        End Get
        Set(ByVal value As Boolean)
            Me._resizeseasonposter = value
        End Set
    End Property

    Public Property ResizeShowFanart() As Boolean
        Get
            Return Me._resizeshowfanart
        End Get
        Set(ByVal value As Boolean)
            Me._resizeshowfanart = value
        End Set
    End Property

    Public Property ResizeShowPoster() As Boolean
        Get
            Return Me._resizeshowposter
        End Get
        Set(ByVal value As Boolean)
            Me._resizeshowposter = value
        End Set
    End Property

    Public Property RuntimeMask() As String
        Get
            Return Me._runtimemask
        End Get
        Set(ByVal value As String)
            Me._runtimemask = value
        End Set
    End Property

    Public Property ScanMediaInfo() As Boolean
        Get
            Return Me._scanmediainfo
        End Get
        Set(ByVal value As Boolean)
            Me._scanmediainfo = value
        End Set
    End Property

    Public Property ScanOrderModify() As Boolean
        Get
            Return Me._scanordermodify
        End Get
        Set(ByVal value As Boolean)
            Me._scanordermodify = value
        End Set
    End Property

    Public Property ScanTVMediaInfo() As Boolean
        Get
            Return Me._scantvmediainfo
        End Get
        Set(ByVal value As Boolean)
            Me._scantvmediainfo = value
        End Set
    End Property

    Public Property ScraperEpActors() As Boolean
        Get
            Return Me._scraperepactors
        End Get
        Set(ByVal value As Boolean)
            Me._scraperepactors = value
        End Set
    End Property

    Public Property ScraperEpAired() As Boolean
        Get
            Return Me._scraperepaired
        End Get
        Set(ByVal value As Boolean)
            Me._scraperepaired = value
        End Set
    End Property

    Public Property ScraperEpCredits() As Boolean
        Get
            Return Me._scraperepcredits
        End Get
        Set(ByVal value As Boolean)
            Me._scraperepcredits = value
        End Set
    End Property

    Public Property ScraperEpDirector() As Boolean
        Get
            Return Me._scraperepdirector
        End Get
        Set(ByVal value As Boolean)
            Me._scraperepdirector = value
        End Set
    End Property

    Public Property ScraperEpEpisode() As Boolean
        Get
            Return Me._scraperepepisode
        End Get
        Set(ByVal value As Boolean)
            Me._scraperepepisode = value
        End Set
    End Property

    Public Property ScraperEpPlot() As Boolean
        Get
            Return Me._scraperepplot
        End Get
        Set(ByVal value As Boolean)
            Me._scraperepplot = value
        End Set
    End Property

    Public Property ScraperEpRating() As Boolean
        Get
            Return Me._scrapereprating
        End Get
        Set(ByVal value As Boolean)
            Me._scrapereprating = value
        End Set
    End Property

    Public Property ScraperEpSeason() As Boolean
        Get
            Return Me._scraperepseason
        End Get
        Set(ByVal value As Boolean)
            Me._scraperepseason = value
        End Set
    End Property

    Public Property ScraperEpTitle() As Boolean
        Get
            Return Me._scrapereptitle
        End Get
        Set(ByVal value As Boolean)
            Me._scrapereptitle = value
        End Set
    End Property

    Public Property ScraperShowActors() As Boolean
        Get
            Return Me._scrapershowactors
        End Get
        Set(ByVal value As Boolean)
            Me._scrapershowactors = value
        End Set
    End Property

    Public Property ScraperShowEGU() As Boolean
        Get
            Return Me._scrapershowegu
        End Get
        Set(ByVal value As Boolean)
            Me._scrapershowegu = value
        End Set
    End Property

    Public Property ScraperShowGenre() As Boolean
        Get
            Return Me._scrapershowgenre
        End Get
        Set(ByVal value As Boolean)
            Me._scrapershowgenre = value
        End Set
    End Property

    Public Property ScraperShowMPAA() As Boolean
        Get
            Return Me._scrapershowmpaa
        End Get
        Set(ByVal value As Boolean)
            Me._scrapershowmpaa = value
        End Set
    End Property

    Public Property ScraperShowPlot() As Boolean
        Get
            Return Me._scrapershowplot
        End Get
        Set(ByVal value As Boolean)
            Me._scrapershowplot = value
        End Set
    End Property

    Public Property ScraperShowPremiered() As Boolean
        Get
            Return Me._scrapershowpremiered
        End Get
        Set(ByVal value As Boolean)
            Me._scrapershowpremiered = value
        End Set
    End Property

    Public Property ScraperShowRating() As Boolean
        Get
            Return Me._scrapershowrating
        End Get
        Set(ByVal value As Boolean)
            Me._scrapershowrating = value
        End Set
    End Property

    Public Property ScraperShowStudio() As Boolean
        Get
            Return Me._scrapershowstudio
        End Get
        Set(ByVal value As Boolean)
            Me._scrapershowstudio = value
        End Set
    End Property

    Public Property ScraperShowTitle() As Boolean
        Get
            Return Me._scrapershowtitle
        End Get
        Set(ByVal value As Boolean)
            Me._scrapershowtitle = value
        End Set
    End Property

    Public Property SeasonAllJPG() As Boolean
        Get
            Return Me._seasonalljpg
        End Get
        Set(ByVal value As Boolean)
            Me._seasonalljpg = value
        End Set
    End Property

    Public Property SeasonAllTBN() As Boolean
        Get
            Return Me._seasonalltbn
        End Get
        Set(ByVal value As Boolean)
            Me._seasonalltbn = value
        End Set
    End Property

    Public Property SeasonAllPosterJPG() As Boolean
        Get
            Return Me._seasonallposterjpg
        End Get
        Set(ByVal value As Boolean)
            Me._seasonallposterjpg = value
        End Set
    End Property

    Public Property SeasonDashFanart() As Boolean
        Get
            Return Me._seasondashfanart
        End Get
        Set(ByVal value As Boolean)
            Me._seasondashfanart = value
        End Set
    End Property

    Public Property SeasonXXDashFanartJPG() As Boolean
        Get
            Return Me._seasonxxdashfanartjpg
        End Get
        Set(ByVal value As Boolean)
            Me._seasonxxdashfanartjpg = value
        End Set
    End Property

    Public Property SeasonDotFanart() As Boolean
        Get
            Return Me._seasondotfanart
        End Get
        Set(ByVal value As Boolean)
            Me._seasondotfanart = value
        End Set
    End Property

    Public Property SeasonFanartCol() As Boolean
        Get
            Return Me._seasonfanartcol
        End Get
        Set(ByVal value As Boolean)
            Me._seasonfanartcol = value
        End Set
    End Property

    Public Property SeasonFanartHeight() As Integer
        Get
            Return Me._seasonfanartheight
        End Get
        Set(ByVal value As Integer)
            Me._seasonfanartheight = value
        End Set
    End Property

    Public Property SeasonFanartJPG() As Boolean
        Get
            Return Me._seasonfanartjpg
        End Get
        Set(ByVal value As Boolean)
            Me._seasonfanartjpg = value
        End Set
    End Property

    Public Property SeasonFanartQuality() As Integer
        Get
            Return Me._seasonfanartQuality
        End Get
        Set(ByVal value As Integer)
            Me._seasonfanartQuality = value
        End Set
    End Property

    Public Property SeasonFanartWidth() As Integer
        Get
            Return Me._seasonfanartwidth
        End Get
        Set(ByVal value As Integer)
            Me._seasonfanartwidth = value
        End Set
    End Property

    Public Property SeasonFolderJPG() As Boolean
        Get
            Return Me._seasonfolderjpg
        End Get
        Set(ByVal value As Boolean)
            Me._seasonfolderjpg = value
        End Set
    End Property

    Public Property SeasonNameJPG() As Boolean
        Get
            Return Me._seasonnamejpg
        End Get
        Set(ByVal value As Boolean)
            Me._seasonnamejpg = value
        End Set
    End Property

    Public Property SeasonNameTBN() As Boolean
        Get
            Return Me._seasonnametbn
        End Get
        Set(ByVal value As Boolean)
            Me._seasonnametbn = value
        End Set
    End Property

    Public Property SeasonPosterCol() As Boolean
        Get
            Return Me._seasonpostercol
        End Get
        Set(ByVal value As Boolean)
            Me._seasonpostercol = value
        End Set
    End Property

    Public Property SeasonPosterHeight() As Integer
        Get
            Return Me._seasonposterheight
        End Get
        Set(ByVal value As Integer)
            Me._seasonposterheight = value
        End Set
    End Property

    Public Property SeasonPosterJPG() As Boolean
        Get
            Return Me._seasonposterjpg
        End Get
        Set(ByVal value As Boolean)
            Me._seasonposterjpg = value
        End Set
    End Property

    Public Property SeasonPosterQuality() As Integer
        Get
            Return Me._seasonposterQuality
        End Get
        Set(ByVal value As Integer)
            Me._seasonposterQuality = value
        End Set
    End Property

    Public Property SeasonPosterTBN() As Boolean
        Get
            Return Me._seasonpostertbn
        End Get
        Set(ByVal value As Boolean)
            Me._seasonpostertbn = value
        End Set
    End Property

    Public Property SeasonPosterWidth() As Integer
        Get
            Return Me._seasonposterwidth
        End Get
        Set(ByVal value As Integer)
            Me._seasonposterwidth = value
        End Set
    End Property

    Public Property SeasonX() As Boolean
        Get
            Return Me._seasonx
        End Get
        Set(ByVal value As Boolean)
            Me._seasonx = value
        End Set
    End Property

    Public Property SeasonXX() As Boolean
        Get
            Return Me._seasonxx
        End Get
        Set(ByVal value As Boolean)
            Me._seasonxx = value
        End Set
    End Property

    Public Property SeasonXXDashPosterJPG() As Boolean
        Get
            Return Me._seasonxxdashposterjpg
        End Get
        Set(ByVal value As Boolean)
            Me._seasonxxdashposterjpg = value
        End Set
    End Property

    Public Property Sets() As List(Of String)
        Get
            Return Me._sets
        End Get
        Set(ByVal value As List(Of String))
            Me._sets = value
        End Set
    End Property

    Public Property ShowDashFanart() As Boolean
        Get
            Return Me._showdashfanart
        End Get
        Set(ByVal value As Boolean)
            Me._showdashfanart = value
        End Set
    End Property

    Public Property ShowDims() As Boolean
        Get
            Return Me._showdims
        End Get
        Set(ByVal value As Boolean)
            Me._showdims = value
        End Set
    End Property

    Public Property ShowDotFanart() As Boolean
        Get
            Return Me._showdotfanart
        End Get
        Set(ByVal value As Boolean)
            Me._showdotfanart = value
        End Set
    End Property

    Public Property ShowFanartCol() As Boolean
        Get
            Return Me._showfanartcol
        End Get
        Set(ByVal value As Boolean)
            Me._showfanartcol = value
        End Set
    End Property

    Public Property ShowFanartHeight() As Integer
        Get
            Return Me._showfanartheight
        End Get
        Set(ByVal value As Integer)
            Me._showfanartheight = value
        End Set
    End Property

    Public Property ShowFanartJPG() As Boolean
        Get
            Return Me._showfanartjpg
        End Get
        Set(ByVal value As Boolean)
            Me._showfanartjpg = value
        End Set
    End Property

    Public Property ShowFanartQuality() As Integer
        Get
            Return Me._showfanartQuality
        End Get
        Set(ByVal value As Integer)
            Me._showfanartQuality = value
        End Set
    End Property

    Public Property ShowFanartWidth() As Integer
        Get
            Return Me._showfanartwidth
        End Get
        Set(ByVal value As Integer)
            Me._showfanartwidth = value
        End Set
    End Property

    Public Property ShowFilterCustom() As List(Of String)
        Get
            Return Me._showfiltercustom
        End Get
        Set(ByVal value As List(Of String))
            Me._showfiltercustom = value
        End Set
    End Property

    Public Property ShowFolderJPG() As Boolean
        Get
            Return Me._showfolderjpg
        End Get
        Set(ByVal value As Boolean)
            Me._showfolderjpg = value
        End Set
    End Property

    Public Property ShowInfoPanelState() As Integer
        Get
            Return Me._showinfopanelstate
        End Get
        Set(ByVal value As Integer)
            Me._showinfopanelstate = value
        End Set
    End Property

    Public Property ShowJPG() As Boolean
        Get
            Return Me._showjpg
        End Get
        Set(ByVal value As Boolean)
            Me._showjpg = value
        End Set
    End Property

    Public Property ShowLockGenre() As Boolean
        Get
            Return Me._showlockgenre
        End Get
        Set(ByVal value As Boolean)
            Me._showlockgenre = value
        End Set
    End Property

    Public Property ShowLockPlot() As Boolean
        Get
            Return Me._showlockplot
        End Get
        Set(ByVal value As Boolean)
            Me._showlockplot = value
        End Set
    End Property

    Public Property ShowLockRating() As Boolean
        Get
            Return Me._showlockrating
        End Get
        Set(ByVal value As Boolean)
            Me._showlockrating = value
        End Set
    End Property

    Public Property ShowLockStudio() As Boolean
        Get
            Return Me._showlockstudio
        End Get
        Set(ByVal value As Boolean)
            Me._showlockstudio = value
        End Set
    End Property

    Public Property ShowLockTitle() As Boolean
        Get
            Return Me._showlocktitle
        End Get
        Set(ByVal value As Boolean)
            Me._showlocktitle = value
        End Set
    End Property

    Public Property ShowNfoCol() As Boolean
        Get
            Return Me._shownfocol
        End Get
        Set(ByVal value As Boolean)
            Me._shownfocol = value
        End Set
    End Property

    Public Property ShowPosterCol() As Boolean
        Get
            Return Me._showpostercol
        End Get
        Set(ByVal value As Boolean)
            Me._showpostercol = value
        End Set
    End Property

    Public Property ShowPosterHeight() As Integer
        Get
            Return Me._showposterheight
        End Get
        Set(ByVal value As Integer)
            Me._showposterheight = value
        End Set
    End Property

    Public Property ShowPosterJPG() As Boolean
        Get
            Return Me._showposterjpg
        End Get
        Set(ByVal value As Boolean)
            Me._showposterjpg = value
        End Set
    End Property

    Public Property ShowBannerJPG() As Boolean
        Get
            Return Me._showbannerjpg
        End Get
        Set(ByVal value As Boolean)
            Me._showbannerjpg = value
        End Set
    End Property

    Public Property ShowPosterQuality() As Integer
        Get
            Return Me._showposterQuality
        End Get
        Set(ByVal value As Integer)
            Me._showposterQuality = value
        End Set
    End Property

    Public Property ShowPosterTBN() As Boolean
        Get
            Return Me._showpostertbn
        End Get
        Set(ByVal value As Boolean)
            Me._showpostertbn = value
        End Set
    End Property

    Public Property ShowPosterWidth() As Integer
        Get
            Return Me._showposterwidth
        End Get
        Set(ByVal value As Integer)
            Me._showposterwidth = value
        End Set
    End Property

    Public Property ShowProperCase() As Boolean
        Get
            Return Me._showproperCase
        End Get
        Set(ByVal value As Boolean)
            Me._showproperCase = value
        End Set
    End Property

    Public Property ShowRatingRegion() As String
        Get
            Return Me._showratingregion
        End Get
        Set(ByVal value As String)
            Me._showratingregion = value
        End Set
    End Property

    Public Property ShowTBN() As Boolean
        Get
            Return Me._showtbn
        End Get
        Set(ByVal value As Boolean)
            Me._showtbn = value
        End Set
    End Property

    Public Property SingleScrapeImages() As Boolean
        Get
            Return Me._singlescrapeimages
        End Get
        Set(ByVal value As Boolean)
            Me._singlescrapeimages = value
        End Set
    End Property

    Public Property SingleScrapeTrailer() As Boolean
        Get
            Return Me._singlescrapetrailer
        End Get
        Set(ByVal value As Boolean)
            Me._singlescrapetrailer = value
        End Set
    End Property

    Public Property SkipLessThan() As Integer
        Get
            Return Me._skiplessthan
        End Get
        Set(ByVal value As Integer)
            Me._skiplessthan = value
        End Set
    End Property

    Public Property SkipStackSizeCheck() As Boolean
        Get
            Return Me._skipstacksizecheck
        End Get
        Set(ByVal value As Boolean)
            Me._skipstacksizecheck = value
        End Set
    End Property

    Public Property SkipLessThanEp() As Integer
        Get
            Return Me._skiplessthanep
        End Get
        Set(ByVal value As Integer)
            Me._skiplessthanep = value
        End Set
    End Property

    Public Property SortBeforeScan() As Boolean
        Get
            Return Me._sortbeforescan
        End Get
        Set(ByVal value As Boolean)
            Me._sortbeforescan = value
        End Set
    End Property

    Public Property SortPath() As String
        Get
            Return Me._sortpath
        End Get
        Set(ByVal value As String)
            Me._sortpath = value
        End Set
    End Property

    Public Property SortTokens() As List(Of String)
        Get
            Return Me._sorttokens
        End Get
        Set(ByVal value As List(Of String))
            Me._sorttokens = value
        End Set
    End Property

    Public Property SourceFromFolder() As Boolean
        Get
            Return Me._sourcefromfolder
        End Get
        Set(ByVal value As Boolean)
            Me._sourcefromfolder = value
        End Set
    End Property

    Public Property SplitterPanelState() As Integer
        Get
            Return Me._scmainstate
        End Get
        Set(ByVal value As Integer)
            Me._scmainstate = value
        End Set
    End Property

    Public Property ShowSplitterPanelState() As Integer
        Get
            Return Me._scshowstate
        End Get
        Set(ByVal value As Integer)
            Me._scshowstate = value
        End Set
    End Property

    Public Property SeasonSplitterPanelState() As Integer
        Get
            Return Me._scseasonstate
        End Get
        Set(ByVal value As Integer)
            Me._scseasonstate = value
        End Set
    End Property

    Public Property TrailerTimeout() As Integer
        Get
            Return Me._trailertimeout
        End Get
        Set(ByVal value As Integer)
            Me._trailertimeout = value
        End Set
    End Property

    Public Property TVCleanDB() As Boolean
        Get
            Return Me._tvcleandb
        End Get
        Set(ByVal value As Boolean)
            Me._tvcleandb = value
        End Set
    End Property

    Public Property TVDBLanguage() As String
        Get
            Return Me._tvdblanguage
        End Get
        Set(ByVal value As String)
            Me._tvdblanguage = If(String.IsNullOrEmpty(value), "en", value)
        End Set
    End Property

    Public Property TVDBLanguages() As List(Of Containers.TVLanguage)
        Get
            Return Me._tvdblanguages
        End Get
        Set(ByVal value As List(Of Containers.TVLanguage))
            Me._tvdblanguages = value
        End Set
    End Property

    Public Property TVDBMirror() As String
        Get
            Return Me._tvdbmirror
        End Get
        Set(ByVal value As String)
            Me._tvdbmirror = value
        End Set
    End Property

    Public Property TVEpTheme() As String
        Get
            Return Me._tveptheme
        End Get
        Set(ByVal value As String)
            Me._tveptheme = value
        End Set
    End Property

    Public Property TVFlagLang() As String
        Get
            Return Me._tvflaglang
        End Get
        Set(ByVal value As String)
            Me._tvflaglang = value
        End Set
    End Property

    Public Property TVIgnoreLastScan() As Boolean
        Get
            Return Me._tvignorelastscan
        End Get
        Set(ByVal value As Boolean)
            Me._tvignorelastscan = value
        End Set
    End Property

    Public Property TVMetadataperFileType() As List(Of MetadataPerType)
        Get
            Return Me._tvmetadatapertype
        End Get
        Set(ByVal value As List(Of MetadataPerType))
            Me._tvmetadatapertype = value
        End Set
    End Property

    Public Property TVScanOrderModify() As Boolean
        Get
            Return Me._tvscanordermodify
        End Get
        Set(ByVal value As Boolean)
            Me._tvscanordermodify = value
        End Set
    End Property

    Public Property TVShowRegexes() As List(Of TVShowRegEx)
        Get
            Return Me._tvshowregexes
        End Get
        Set(ByVal value As List(Of TVShowRegEx))
            Me._tvshowregexes = value
        End Set
    End Property

    Public Property TVShowTheme() As String
        Get
            Return Me._tvshowtheme
        End Get
        Set(ByVal value As String)
            Me._tvshowtheme = value
        End Set
    End Property

    Public Property TVUpdateTime() As Enums.TVUpdateTime
        Get
            Return Me._tvupdatetime
        End Get
        Set(ByVal value As Enums.TVUpdateTime)
            Me._tvupdatetime = value
        End Set
    End Property

    Public Property UpdaterTrailers() As Boolean
        Get
            Return Me._updatertrailers
        End Get
        Set(ByVal value As Boolean)
            Me._updatertrailers = value
        End Set
    End Property

    Public Property UpdaterTrailersNoDownload() As Boolean
        Get
            Return Me._updatertrailersnodownload
        End Get
        Set(ByVal value As Boolean)
            Me._updatertrailersnodownload = value
        End Set
    End Property

    Public Property UseCertForMPAA() As Boolean
        Get
            Return Me._usecertformpaa
        End Get
        Set(ByVal value As Boolean)
            Me._usecertformpaa = value
        End Set
    End Property

    Public Property UseETasFA() As Boolean
        Get
            Return Me._useetasfa
        End Get
        Set(ByVal value As Boolean)
            Me._useetasfa = value
        End Set
    End Property

    Public Property UseImgCache() As Boolean
        Get
            Return Me._useimgcache
        End Get
        Set(ByVal value As Boolean)
            Me._useimgcache = value
        End Set
    End Property

    Public Property UseImgCacheUpdaters() As Boolean
        Get
            Return Me._useimgcacheupdater
        End Get
        Set(ByVal value As Boolean)
            Me._useimgcacheupdater = value
        End Set
    End Property

    Public Property UseMIDuration() As Boolean
        Get
            Return Me._usemiduration
        End Get
        Set(ByVal value As Boolean)
            Me._usemiduration = value
        End Set
    End Property

    Public Property ValidExts() As List(Of String)
        Get
            Return Me._validexts
        End Get
        Set(ByVal value As List(Of String))
            Me._validexts = value
        End Set
    End Property

    Public Property Version() As String
        Get
            Return Me._version
        End Get
        Set(ByVal value As String)
            Me._version = value
        End Set
    End Property

    Public Property VideoTSParent() As Boolean
        Get
            Return Me._videotsparent
        End Get
        Set(ByVal value As Boolean)
            Me._videotsparent = value
        End Set
    End Property

    Public Property WindowLoc() As Point
        Get
            Return Me._windowloc
        End Get
        Set(ByVal value As Point)
            Me._windowloc = value
        End Set
    End Property

    Public Property WindowSize() As Size
        Get
            Return Me._windowsize
        End Get
        Set(ByVal value As Size)
            Me._windowsize = value
        End Set
    End Property

    Public Property WindowState() As FormWindowState
        Get
            Return Me._windowstate
        End Get
        Set(ByVal value As FormWindowState)
            Me._windowstate = value
        End Set
    End Property

    Public Property YAMJSetsCompatible() As Boolean
        Get
            Return Me._yamjsetscompatible
        End Get
        Set(ByVal value As Boolean)
            Me._yamjsetscompatible = value
        End Set
    End Property

    Public Property Username() As String
        Get
            Return Me._username
        End Get
        Set(ByVal value As String)
            Me._username = value
        End Set
    End Property

    Public Property Password() As String
        Get
            Return Me._password
        End Get
        Set(ByVal value As String)
            Me._password = value
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Public Function AllSeasonPosterEnabled() As Boolean
        Return Me._seasonalltbn OrElse Me._seasonalljpg OrElse Me._seasonallposterjpg
    End Function

    Public Sub Clear()
        Me._version = String.Empty
        Me._filterCustom = New List(Of String)
        Me._showfiltercustom = New List(Of String)
        Me._epfiltercustom = New List(Of String)
        Me._forcetitle = String.Empty
        Me._certificationLang = String.Empty
        Me._usecertformpaa = False
        Me._showratingregion = "usa"
        Me._askcheckboxscrape = True
        Me._scanmediainfo = True
        Me._scantvmediainfo = True
        Me._fullcast = True     'emm-r
        Me._fullcrew = False
        Me._castimagesonly = False
        Me._movieposterCol = False
        Me._moviefanartCol = False
        Me._movieinfoCol = False
        Me._movietrailerCol = False
        Me._moviesubCol = False
        Me._movieextraCol = False
        Me._cleanfolderJpg = False
        Me._cleanmovieTbn = False
        Me._cleanmovieTbnB = False
        Me._cleanfanartJpg = False
        Me._cleanmoviefanartJpg = False
        Me._cleanmovieNfo = False
        Me._cleanmovieNfoB = False
        Me._cleanposterTbn = False
        Me._cleanposterJpg = False
        Me._cleanmovieJpg = False
        Me._cleandotfanartJpg = False
        Me._cleanmovienameJpg = False
        Me._cleanextrathumbs = False
        Me._expertcleaner = False
        Me._cleanwhitelistvideo = False
        Me._cleanwhitelistexts = New List(Of String)
        Me._postersize = Enums.PosterSize.Xlrg
        Me._fanartsize = Enums.FanartSize.Lrg
        Me._showbanner = True
        Me._showbannertype = Enums.ShowBannerType.Graphical
        Me._showpostersize = Enums.PosterSize.Xlrg
        Me._allsbanner = False
        Me._allsbannertype = Enums.ShowBannerType.Graphical
        Me._allspostersize = Enums.PosterSize.Xlrg
        Me._showfanartsize = Enums.FanartSize.Lrg
        Me._epfanartsize = Enums.FanartSize.Lrg
        Me._seasonpostersize = Enums.SeasonPosterType.Poster
        Me._seasonfanartsize = Enums.FanartSize.Lrg
        Me._autoET = False
        Me._autoETsize = Enums.FanartSize.Lrg
        Me._fanartprefsizeonly = False
        Me._PosterPrefSizeOnly = False
        Me._posterQuality = 0
        Me._fanartQuality = 0
        Me._overwritePoster = True
        Me._overwriteFanart = True
        Me._showposterQuality = 0
        Me._showfanartQuality = 0
        Me._overwriteShowPoster = True
        Me._overwriteShowFanart = True
        Me._allsposterQuality = 0
        Me._overwriteallsPoster = True
        Me._epposterQuality = 0
        Me._epfanartQuality = 0
        Me._overwriteEpPoster = True
        Me._overwriteEpFanart = True
        Me._seasonposterQuality = 0
        Me._seasonfanartQuality = 0
        Me._overwriteSeasonPoster = True
        Me._overwriteSeasonFanart = True
        Me._logerrors = True
        Me._properCase = True
        Me._showproperCase = True
        Me._epproperCase = True
        Me._overwritenfo = False
        Me._overwritenfo = False
        Me._overwritenfo = False
        Me._validexts = New List(Of String)
        Me._nostackexts = New List(Of String)
        Me._movietbn = True
        Me._movienametbn = True
        Me._moviejpg = False
        Me._movienamejpg = False
        Me._movienamedashposterjpg = False
        Me._postertbn = False
        Me._posterjpg = False
        Me._folderjpg = False
        Me._fanartjpg = True
        Me._movienamefanartjpg = True
        Me._movienamedotfanartjpg = False
        Me._movienfo = True
        Me._movienamenfo = True
        Me._movienamemultionly = False
        Me._dashtrailer = True
        Me._videotsparent = False
        Me._lockplot = False
        Me._lockoutline = False
        Me._locktitle = False
        Me._locktagline = False
        Me._lockrating = False
        Me._lockstudio = False
        Me._lockrating = False
        Me._locktrailer = False
        Me._singlescrapeimages = True
        Me._marknew = False
        Me._resizefanart = False
        Me._fanartheight = 0
        Me._fanartwidth = 0
        Me._resizeposter = False
        Me._posterheight = 0
        Me._posterwidth = 0
        Me._resizeshowfanart = False
        Me._showfanartheight = 0
        Me._showfanartwidth = 0
        Me._resizeshowposter = False
        Me._showposterheight = 0
        Me._showposterwidth = 0
        Me._resizeallsposter = False
        Me._allsposterheight = 0
        Me._allsposterwidth = 0
        Me._resizeepfanart = False
        Me._epfanartheight = 0
        Me._epfanartwidth = 0
        Me._resizeepposter = False
        Me._epposterheight = 0
        Me._epposterwidth = 0
        Me._resizeseasonfanart = False
        Me._seasonfanartheight = 0
        Me._seasonfanartwidth = 0
        Me._resizeseasonposter = False
        Me._seasonposterheight = 0
        Me._seasonposterwidth = 0
        Me._autothumbs = 0
        Me._autothumbnospoilers = False
        Me._windowloc = New Point(If(Screen.PrimaryScreen.WorkingArea.Width <= 1024, 0, Convert.ToInt32((Screen.PrimaryScreen.WorkingArea.Width - 1024) / 2)), If(Screen.PrimaryScreen.WorkingArea.Height <= 768, 0, Convert.ToInt32((Screen.PrimaryScreen.WorkingArea.Height - 768) / 2)))
        Me._windowsize = New Size(1024, 768)
        Me._windowstate = FormWindowState.Normal
        Me._infopanelstate = 0
        Me._showinfopanelstate = 0
        Me._filterPanelState = False
        Me._scmainstate = 364
        Me._scshowstate = 200
        Me._scseasonstate = 200
        Me._infopanelanim = False   'emm-r
        Me._checkupdates = True
        Me._bdpath = String.Empty
        Me._autobd = False
        Me._usemiduration = False
        Me._runtimemask = "<m>"     'emm-r
        Me._genrefilter = "English"
        Me._useetasfa = False
        Me._useimgcache = False
        Me._useimgcacheupdater = False
        Me._persistimagecache = False
        Me._skiplessthan = 0
        Me._skipstacksizecheck = False
        Me._skiplessthanep = 0
        Me._downloadtrailers = False
        Me._trailerquality = Enums.TrailerQuality.HD1080p
        Me._updatertrailers = False
        Me._updatertrailersnodownload = False
        Me._singlescrapetrailer = False
        Me._trailertimeout = 2
        Me._overwritetrailer = False
        Me._deletealltrailers = False
        Me._sets = New List(Of String)
        Me._nosaveimagestonfo = False
        Me._showdims = False
        Me._nodisplayposter = False
        Me._nodisplayfanart = False
        Me._outlineforplot = False
        Me._sortpath = String.Empty
        Me._allwaysdisplaygenrestext = False
        Me._displayyear = False
        Me._sorttokens = New List(Of String)
        Me._etnative = True
        Me._etwidth = 0
        Me._etheight = 0
        Me._etpadding = False
        Me._nofilters = False
        Me._noshowfilters = False
        Me._noepfilters = False
        Me._notokens = False
        Me._levtolerance = 0
        Me._autodetectvts = True
        Me._flaglang = String.Empty
        Me._tvflaglang = String.Empty
        Me._language = "English_(en_US)"
        Me._fieldtitle = True
        Me._fieldyear = True
        Me._fieldmpaa = True
        Me._fieldcert = True
        Me._fieldrelease = True
        Me._fieldruntime = True
        Me._fieldrating = True
        Me._fieldvotes = True
        Me._fieldstudio = True
        Me._fieldgenre = True
        Me._fieldtrailer = True
        Me._fieldtagline = True
        Me._fieldoutline = True
        Me._fieldplot = True
        Me._fieldcast = True
        Me._fielddirector = True
        Me._fieldwriters = True
        Me._fieldproducers = True
        Me._fieldmusic = True
        Me._fieldcrew = True
        Me._field250 = True
        Me._fieldcountry = True
        Me._genrelimit = 0
        Me._actorlimit = 0
        Me._missingfilterposter = True
        Me._missingfilterfanart = True
        Me._missingfilternfo = True
        Me._missingfiltertrailer = True
        Me._missingfiltersubs = True
        Me._missingfilterextras = True
        Me._movietheme = "Default"
        Me._tvshowtheme = "Default"
        Me._tveptheme = "Default"
        Me._metadatapertype = New List(Of MetadataPerType)
        Me._tvmetadatapertype = New List(Of MetadataPerType)
        Me._enableifoscan = True
        Me._yamjsetscompatible = False
        Me._cleandb = True
        Me._ignorelastscan = True
        Me._tvcleandb = True
        Me._tvignorelastscan = True
        Me._tvshowregexes = New List(Of TVShowRegEx)
        Me._seasonalltbn = True
        Me._seasonalljpg = False
        Me._seasonallposterjpg = False
        Me._showfolderjpg = True
        Me._showtbn = False
        Me._showjpg = False
        Me._showpostertbn = False
        Me._showposterjpg = False
        Me._showbannerjpg = False
        Me._showfanartjpg = True
        Me._showdashfanart = False
        Me._showdotfanart = False
        Me._seasonxx = True
        Me._seasonx = False
        Me._seasonxxdashposterjpg = False
        Me._seasonpostertbn = False
        Me._seasonposterjpg = False
        Me._seasonnametbn = False
        Me._seasonnamejpg = False
        Me._seasonfolderjpg = False
        Me._seasonfanartjpg = False
        Me._seasondashfanart = False
        Me._seasonxxdashfanartjpg = False
        Me._seasondotfanart = False
        Me._episodetbn = True
        Me._episodejpg = False
        Me._episodedashthumbjpg = False
        Me._episodedashfanart = False
        Me._episodedotfanart = False
        Me._showpostercol = False
        Me._showfanartcol = False
        Me._shownfocol = False
        Me._seasonpostercol = False
        Me._seasonfanartcol = True
        Me._episodepostercol = False
        Me._episodefanartcol = True
        Me._episodenfocol = False
        Me._proxyuri = String.Empty
        Me._proxyport = -1
        Me._proxycredentials = New NetworkCredential
        Me._sourcefromfolder = False
        Me._sortbeforescan = False
        Me._tvdbmirror = "thetvdb.com"
        Me._tvdblanguage = "en"
        Me._tvdblanguages = New List(Of Containers.TVLanguage)
        Me._emberModules = New List(Of ModulesManager._XMLEmberModuleClass)
        Me._externaltvdbapikey = String.Empty
        Me._scanordermodify = False
        Me._tvscanordermodify = False
        Me._tvupdatetime = Enums.TVUpdateTime.Week
        Me._nofilterepisode = True
        Me._onlytvimagesforselectedlangauge = True
        Me._alwaysgetenglishtvimages = True
        Me._displaymissingepisodes = False
        Me._showlocktitle = False
        Me._showlockplot = False
        Me._showlockrating = False
        Me._showlockgenre = False
        Me._showlockstudio = False
        Me._eplocktitle = False
        Me._eplockplot = False
        Me._eplockrating = False
        Me._scrapershowtitle = True
        Me._scrapershowegu = True
        Me._scrapershowgenre = True
        Me._scrapershowmpaa = True
        Me._scrapershowplot = True
        Me._scrapershowpremiered = True
        Me._scrapershowrating = True
        Me._scrapershowstudio = True
        Me._scrapershowactors = True
        Me._scrapereptitle = True
        Me._scraperepseason = True
        Me._scraperepepisode = True
        Me._scraperepaired = True
        Me._scrapereprating = True
        Me._scraperepplot = True
        Me._scraperepdirector = True
        Me._scraperepcredits = True
        Me._scraperepactors = True
        Me._displayallseason = True
        Me._marknewshows = False
        Me._marknewepisodes = False
        Me._orderdefault = Enums.Ordering.Standard
        Me._onlyvalueforcert = False
        Me._username = String.Empty
        Me._password = String.Empty
    End Sub

    Public Function EpisodeFanartEnabled() As Boolean
        Return Me._episodedashfanart OrElse Me._episodedotfanart
    End Function

    Public Sub Load()
        Try
            Dim xmlSerial As New XmlSerializer(GetType(Settings))

            If File.Exists(Path.Combine(Functions.AppPath, "Settings.xml")) Then
                Dim strmReader As New StreamReader(Path.Combine(Functions.AppPath, "Settings.xml"))
                Master.eSettings = DirectCast(xmlSerial.Deserialize(strmReader), Settings)
                strmReader.Close()
            Else
                Master.eSettings = New Settings
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            Master.eSettings = New Settings
        End Try

        If Not Master.eSettings.Version = String.Format("r{0}", My.Application.Info.Version.Revision) Then
            SetDefaultsForLists(Enums.DefaultType.All, False)
        End If
    End Sub

    Public Sub Save()
        Try
            Dim xmlSerial As New XmlSerializer(GetType(Settings))
            Dim xmlWriter As New StreamWriter(Path.Combine(Functions.AppPath, "Settings.xml"))
            xmlSerial.Serialize(xmlWriter, Master.eSettings)
            xmlWriter.Close()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Function SeasonFanartEnabled() As Boolean
        Return Master.eSettings.SeasonFanartJPG OrElse Master.eSettings.SeasonDashFanart OrElse Master.eSettings.SeasonDotFanart OrElse Master.eSettings.SeasonXXDashFanartJPG
    End Function

    Public Sub SetDefaultsForLists(ByVal Type As Enums.DefaultType, ByVal Force As Boolean)
        If (Type = Enums.DefaultType.All OrElse Type = Enums.DefaultType.MovieFilters) AndAlso (Force OrElse (Master.eSettings.FilterCustom.Count <= 0 AndAlso Not Master.eSettings.NoFilters)) Then
            Master.eSettings.FilterCustom.Clear()
            Master.eSettings.FilterCustom.Add("[\W_]\(?\d{4}\)?.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]blu[\W_]?ray.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]bd[\W_]?rip.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]dvd.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]720.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]1080.*") 'not really needed because the year title will catch this one, but just in case a user doesn't want the year filter but wants to filter 1080
            Master.eSettings.FilterCustom.Add("(?i)[\W_]ac3.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]dts.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]divx.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]xvid.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]dc[\W_]?.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]dir(ector'?s?)?\s?cut.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]extended.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]hd(tv)?.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]unrated.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]uncut.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]([a-z]{3}|multi)[sd]ub.*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]\[offline\].*")
            Master.eSettings.FilterCustom.Add("(?i)[\W_]ntsc.*")
            Master.eSettings.FilterCustom.Add("[\W_]PAL[\W_]?.*")
            Master.eSettings.FilterCustom.Add("\.[->] ")
        End If

        If (Type = Enums.DefaultType.All OrElse Type = Enums.DefaultType.ShowFilters) AndAlso (Force OrElse (Master.eSettings.ShowFilterCustom.Count <= 0 AndAlso Not Master.eSettings.NoShowFilters)) Then
            Master.eSettings.ShowFilterCustom.Clear()
            Master.eSettings.ShowFilterCustom.Add("[\W_]\(?\d{4}\)?.*")
            'would there ever be season or episode info in the show folder name??
            'Master.eSettings.ShowFilterCustom.Add("(?i)([\W_]+\s?)?s[0-9]+[\W_]*e[0-9]+(\])*")
            'Master.eSettings.ShowFilterCustom.Add("(?i)([\W_]+\s?)?[0-9]+x[0-9]+(\])*")
            'Master.eSettings.ShowFilterCustom.Add("(?i)([\W_]+\s?)?s(eason)?[\W_]*[0-9]+(\])*")
            'Master.eSettings.ShowFilterCustom.Add("(?i)([\W_]+\s?)?e(pisode)?[\W_]*[0-9]+(\])*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]blu[\W_]?ray.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]bd[\W_]?rip.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]dvd.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]720.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]1080.*") 'not really needed because the year title will catch this one, but just in case a user doesn't want the year filter but wants to filter 1080
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]ac3.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]dts.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]divx.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]xvid.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]dc[\W_]?.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]dir(ector'?s?)?\s?cut.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]extended.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]hd(tv)?.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]unrated.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]uncut.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]([a-z]{3}|multi)[sd]ub.*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]\[offline\].*")
            Master.eSettings.ShowFilterCustom.Add("(?i)[\W_]ntsc.*")
            Master.eSettings.ShowFilterCustom.Add("[\W_]PAL[\W_]?.*")
            Master.eSettings.ShowFilterCustom.Add("\.[->] ")
        End If

        If (Type = Enums.DefaultType.All OrElse Type = Enums.DefaultType.EpFilters) AndAlso (Force OrElse (Master.eSettings.EpFilterCustom.Count <= 0 AndAlso Not Master.eSettings.NoEpFilters)) Then
            Master.eSettings.EpFilterCustom.Clear()
            Master.eSettings.EpFilterCustom.Add("[\W_]\(?\d{4}\)?.*")
            Master.eSettings.EpFilterCustom.Add("(?i)([\W_]+\s?)?s[0-9]+[\W_]*([-e][0-9]+)+(\])*")
            Master.eSettings.EpFilterCustom.Add("(?i)([\W_]+\s?)?[0-9]+([-x][0-9]+)+(\])*")
            Master.eSettings.EpFilterCustom.Add("(?i)([\W_]+\s?)?s(eason)?[\W_]*[0-9]+(\])*")
            Master.eSettings.EpFilterCustom.Add("(?i)([\W_]+\s?)?e(pisode)?[\W_]*[0-9]+(\])*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]blu[\W_]?ray.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]bd[\W_]?rip.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]dvd.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]720.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]1080.*") 'not really needed because the year title will catch this one, but just in case a user doesn't want the year filter but wants to filter 1080
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]ac3.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]dts.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]divx.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]xvid.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]dc[\W_]?.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]dir(ector'?s?)?\s?cut.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]extended.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]hd(tv)?.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]unrated.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]uncut.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]([a-z]{3}|multi)[sd]ub.*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]\[offline\].*")
            Master.eSettings.EpFilterCustom.Add("(?i)[\W_]ntsc.*")
            Master.eSettings.EpFilterCustom.Add("[\W_]PAL[\W_]?.*")
            Master.eSettings.EpFilterCustom.Add("\.[->] ")
        End If

        If Type = Enums.DefaultType.All AndAlso Master.eSettings.SortTokens.Count <= 0 AndAlso Not Master.eSettings.NoTokens Then
            Master.eSettings.SortTokens.Add("the[\W_]")
            Master.eSettings.SortTokens.Add("a[\W_]")
            Master.eSettings.SortTokens.Add("an[\W_]")
        End If

        If (Type = Enums.DefaultType.All OrElse Type = Enums.DefaultType.ValidExts) AndAlso (Force OrElse Master.eSettings.ValidExts.Count <= 0) Then
            Master.eSettings.ValidExts.Clear()
            Master.eSettings.ValidExts.AddRange(Strings.Split(".avi,.divx,.mkv,.iso,.mpg,.mp4,.mpeg,.wmv,.wma,.mov,.mts,.m2t,.img,.dat,.bin,.cue,.ifo,.vob,.dvb,.evo,.asf,.asx,.avs,.nsv,.ram,.ogg,.ogm,.ogv,.flv,.swf,.nut,.viv,.rar,.m2ts,.dvr-ms,.ts,.m4v,.rmvb", ","))
        End If

        If (Type = Enums.DefaultType.All OrElse Type = Enums.DefaultType.ShowRegex) AndAlso (Force OrElse Master.eSettings.TVShowRegexes.Count <= 0) Then
            Master.eSettings.TVShowRegexes.Clear()
            Master.eSettings.TVShowRegexes.Add(New TVShowRegEx With {.ID = 0, .SeasonRegex = "(s(eason[\W_]*)?(?<season>[0-9]+))([\W_]*(\.?(-|(e(pisode[\W_]*)?))[0-9]+)+)?", .SeasonFromDirectory = False, .EpisodeRegex = "(-|(e(pisode[\W_]*)?))(?<episode>[0-9]+)", .EpisodeRetrieve = EpRetrieve.FromSeasonResult})
            Master.eSettings.TVShowRegexes.Add(New TVShowRegEx With {.ID = 1, .SeasonRegex = "(([0-9]{4}-[0-9]{2}(-[0-9]{2})?)|([0-9]{2}-[0-9]{2}-[0-9]{4})|((?<season>[0-9]+)([x-][0-9]+)+))", .SeasonFromDirectory = False, .EpisodeRegex = "[x-](?<episode>[0-9]+)", .EpisodeRetrieve = EpRetrieve.FromSeasonResult})
            Master.eSettings.TVShowRegexes.Add(New TVShowRegEx With {.ID = 2, .SeasonRegex = "(([0-9]{4}-[0-9]{2}(-[0-9]{2})?)|([0-9]{2}-[0-9]{2}-[0-9]{4})|((?<season>[0-9]+)(-?[0-9]{2,})+(?![0-9])))", .SeasonFromDirectory = False, .EpisodeRegex = "(\([0-9]{4}\))|((([0-9]+|-)(?<episode>[0-9]{2,})))", .EpisodeRetrieve = EpRetrieve.FromSeasonResult})
            Master.eSettings.TVShowRegexes.Add(New TVShowRegEx With {.ID = 3, .SeasonRegex = "^(?<season>specials?)$", .SeasonFromDirectory = True, .EpisodeRegex = "[^a-zA-Z]e(pisode[\W_]*)?(?<episode>[0-9]+)", .EpisodeRetrieve = EpRetrieve.FromFilename})
            Master.eSettings.TVShowRegexes.Add(New TVShowRegEx With {.ID = 4, .SeasonRegex = "^(s(eason)?)?[\W_]*(?<season>[0-9]+)$", .SeasonFromDirectory = True, .EpisodeRegex = "[^a-zA-Z]e(pisode[\W_]*)?(?<episode>[0-9]+)", .EpisodeRetrieve = EpRetrieve.FromFilename})
            Master.eSettings.TVShowRegexes.Add(New TVShowRegEx With {.ID = 5, .SeasonRegex = "[^\w]s(eason)?[\W_]*(?<season>[0-9]+)", .SeasonFromDirectory = True, .EpisodeRegex = "[^a-zA-Z]e(pisode[\W_]*)?(?<episode>[0-9]+)", .EpisodeRetrieve = EpRetrieve.FromFilename})
        End If
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Public Class MetadataPerType

#Region "Fields"

        Private _filetype As String
        Private _metadata As MediaInfo.Fileinfo

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property FileType() As String
            Get
                Return Me._filetype
            End Get
            Set(ByVal value As String)
                Me._filetype = value
            End Set
        End Property

        Public Property MetaData() As MediaInfo.Fileinfo
            Get
                Return Me._metadata
            End Get
            Set(ByVal value As MediaInfo.Fileinfo)
                Me._metadata = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._filetype = String.Empty
            Me._metadata = New MediaInfo.Fileinfo
        End Sub

#End Region 'Methods

    End Class

    Public Class TVShowRegEx

#Region "Fields"

        Private _episoderegex As String
        Private _episoderetrieve As EpRetrieve
        Private _id As Integer
        Private _seasonfromdirectory As Boolean
        Private _seasonregex As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property EpisodeRegex() As String
            Get
                Return Me._episoderegex
            End Get
            Set(ByVal value As String)
                Me._episoderegex = value
            End Set
        End Property

        Public Property EpisodeRetrieve() As EpRetrieve
            Get
                Return Me._episoderetrieve
            End Get
            Set(ByVal value As EpRetrieve)
                Me._episoderetrieve = value
            End Set
        End Property

        Public Property ID() As Integer
            Get
                Return _id
            End Get
            Set(ByVal value As Integer)
                _id = value
            End Set
        End Property

        Public Property SeasonFromDirectory() As Boolean
            Get
                Return Me._seasonfromdirectory
            End Get
            Set(ByVal value As Boolean)
                Me._seasonfromdirectory = value
            End Set
        End Property

        Public Property SeasonRegex() As String
            Get
                Return Me._seasonregex
            End Get
            Set(ByVal value As String)
                Me._seasonregex = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._id = -1
            Me._seasonregex = String.Empty
            Me._seasonfromdirectory = True
            Me._episoderegex = String.Empty
            Me._episoderetrieve = EpRetrieve.FromSeasonResult
        End Sub

#End Region 'Methods

    End Class

#End Region 'Nested Types

End Class