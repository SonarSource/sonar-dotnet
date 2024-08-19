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

''' <summary>
''' Native Scraper
''' </summary>
''' <remarks></remarks>
Public Class EmberNativeTVScraperModule
    Implements Interfaces.EmberTVScraperModule

#Region "Fields"

    Public Shared TVScraper As New Scraper

    Private _Name As String = "Ember Native TV Scrapers"
    Private _PostScraperEnabled As Boolean = False
    Private _ScraperEnabled As Boolean = False
    Private _setup As frmTVInfoSettingsHolder
    Private _setupPost As frmTVMediaSettingsHolder
#End Region 'Fields

#Region "Events"

    Public Event ModuleSettingsChanged() Implements Interfaces.EmberTVScraperModule.ModuleSettingsChanged

    Public Event SetupPostScraperChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.EmberTVScraperModule.SetupPostScraperChanged

    Public Event SetupScraperChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.EmberTVScraperModule.SetupScraperChanged

    Public Event TVScraperEvent(ByVal eType As Enums.TVScraperEventType, ByVal iProgress As Integer, ByVal Parameter As Object) Implements Interfaces.EmberTVScraperModule.TVScraperEvent

#End Region 'Events

#Region "Properties"

    Public ReadOnly Property IsBusy() As Boolean Implements Interfaces.EmberTVScraperModule.IsBusy
        Get
            Return TVScraper.IsBusy
        End Get
    End Property

    Public ReadOnly Property IsPostScraper() As Boolean Implements Interfaces.EmberTVScraperModule.IsPostScraper
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property IsScraper() As Boolean Implements Interfaces.EmberTVScraperModule.IsScraper
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property ModuleName() As String Implements Interfaces.EmberTVScraperModule.ModuleName
        Get
            Return _Name
        End Get
    End Property

    Public ReadOnly Property ModuleVersion() As String Implements Interfaces.EmberTVScraperModule.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

    Public Property PostScraperEnabled() As Boolean Implements Interfaces.EmberTVScraperModule.PostScraperEnabled
        Get
            Return _PostScraperEnabled
        End Get
        Set(ByVal value As Boolean)
            _PostScraperEnabled = value
        End Set
    End Property

    Public Property ScraperEnabled() As Boolean Implements Interfaces.EmberTVScraperModule.ScraperEnabled
        Get
            Return _ScraperEnabled
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled = value
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Public Sub CancelAsync() Implements Interfaces.EmberTVScraperModule.CancelAsync
        TVScraper.CancelAsync()
    End Sub

    Public Function ChangeEpisode(ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Lang As String, ByRef epDet As MediaContainers.EpisodeDetails) As Interfaces.ModuleResult Implements Interfaces.EmberTVScraperModule.ChangeEpisode
        epDet = TVScraper.ChangeEpisode(ShowID, TVDBID, Lang)
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Public Function GetLangs(ByVal sMirror As String, ByRef Langs As List(Of Containers.TVLanguage)) As Interfaces.ModuleResult Implements Interfaces.EmberTVScraperModule.GetLangs
        Langs = TVScraper.GetLangs(sMirror)
        Return New Interfaces.ModuleResult With {.breakChain = True}
    End Function

    Public Function GetSingleEpisode(ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Season As Integer, ByVal Episode As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions, ByRef epDetails As MediaContainers.EpisodeDetails) As Interfaces.ModuleResult Implements Interfaces.EmberTVScraperModule.GetSingleEpisode
        epDetails = TVScraper.GetSingleEpisode(ShowID, TVDBID, Season, Episode, Lang, Ordering, Options)
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Public Function GetSingleImage(ByVal Title As String, ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Type As Enums.TVImageType, ByVal Season As Integer, ByVal Episode As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal CurrentImage As Image, ByRef Image As Image) As Interfaces.ModuleResult Implements Interfaces.EmberTVScraperModule.GetSingleImage
        Image = TVScraper.GetSingleImage(Title, ShowID, TVDBID, Type, Season, Episode, Lang, Ordering, CurrentImage)
        Return New Interfaces.ModuleResult With {.breakChain = True}
    End Function

    Public Sub Handler_ScraperEvent(ByVal eType As Enums.TVScraperEventType, ByVal iProgress As Integer, ByVal Parameter As Object)
        RaiseEvent TVScraperEvent(eType, iProgress, Parameter)
    End Sub

    Public Sub Init(ByVal sAssemblyName As String) Implements Interfaces.EmberTVScraperModule.Init
        AddHandler TVScraper.ScraperEvent, AddressOf Handler_ScraperEvent
    End Sub

    Public Function InjectSetupPostScraper() As Containers.SettingsPanel Implements Interfaces.EmberTVScraperModule.InjectSetupPostScraper
        Dim SPanel As New Containers.SettingsPanel
        _setupPost = New frmTVMediaSettingsHolder
        _setupPost.cbEnabled.Checked = _PostScraperEnabled
        SPanel.Name = String.Concat(Me._Name, "PostScraper")
        SPanel.Text = Master.eLang.GetString(0, "Ember Native TV Scrapers")
        SPanel.Type = Master.eLang.GetString(698, "TV Shows", True)
        SPanel.ImageIndex = If(Me._ScraperEnabled, 9, 10)
        SPanel.Order = 100
        SPanel.Panel = Me._setupPost.pnlSettings
        SPanel.Parent = "pnlTVMedia"
        AddHandler _setupPost.SetupPostScraperChanged, AddressOf Handle_SetupPostScraperChanged
        AddHandler _setupPost.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Return SPanel
    End Function

    'Public Event ScraperUpdateMediaList(ByVal col As Integer, ByVal v As Boolean) Implements Interfaces.EmberTVScraperModule.ScraperUpdateMediaList
    Public Function InjectSetupScraper() As Containers.SettingsPanel Implements Interfaces.EmberTVScraperModule.InjectSetupScraper
        Dim SPanel As New Containers.SettingsPanel
        _setup = New frmTVInfoSettingsHolder
        _setup.cbEnabled.Checked = _ScraperEnabled
        SPanel.Name = String.Concat(Me._Name, "Scraper")
        SPanel.Text = Master.eLang.GetString(0, "Ember Native TV Scrapers")
        SPanel.Prefix = "NativeTV_"
        SPanel.Type = Master.eLang.GetString(698, "TV Shows", True)
        SPanel.ImageIndex = If(Me._ScraperEnabled, 9, 10)
        SPanel.Order = 100
        SPanel.Panel = Me._setup.pnlSettings
        SPanel.Parent = "pnlTVData"
        AddHandler _setup.SetupScraperChanged, AddressOf Handle_SetupScraperChanged
        AddHandler _setup.ModuleSettingsChanged, AddressOf Handle_PostModuleSettingsChanged
        Return SPanel
    End Function
    Private Sub Handle_ModuleSettingsChanged()
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub Handle_PostModuleSettingsChanged()
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub Handle_SetupPostScraperChanged(ByVal state As Boolean, ByVal difforder As Integer)
        PostScraperEnabled = state
        RaiseEvent SetupPostScraperChanged(String.Concat(Me._Name, "PostScraper"), state, difforder)
    End Sub

    Private Sub Handle_SetupScraperChanged(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled = state
        RaiseEvent SetupScraperChanged(String.Concat(Me._Name, "Scraper"), state, difforder)
    End Sub
    Public Function PostScraper(ByRef DBTV As Structures.DBTV, ByVal ScrapeType As Enums.ScrapeType) As Interfaces.ModuleResult Implements Interfaces.EmberTVScraperModule.PostScraper
    End Function

    Public Function SaveImages() As Interfaces.ModuleResult Implements Interfaces.EmberTVScraperModule.SaveImages
        TVScraper.SaveImages()
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Public Sub SaveSetupPostScraper(ByVal DoDispose As Boolean) Implements Interfaces.EmberTVScraperModule.SaveSetupPostScraper
    End Sub

    Public Sub SaveSetupScraper(ByVal DoDispose As Boolean) Implements Interfaces.EmberTVScraperModule.SaveSetupScraper
    End Sub

    Public Function ScrapeEpisode(ByVal ShowID As Integer, ByVal ShowTitle As String, ByVal TVDBID As String, ByVal iEpisode As Integer, ByVal iSeason As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions) As Interfaces.ModuleResult Implements Interfaces.EmberTVScraperModule.ScrapeEpisode
        TVScraper.ScrapeEpisode(ShowID, ShowTitle, TVDBID, iEpisode, iSeason, Lang, Ordering, Options)
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Public Function Scraper(ByVal ShowID As Integer, ByVal ShowTitle As String, ByVal TVDBID As String, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions, ByVal ScrapeType As Enums.ScrapeType, ByVal WithCurrent As Boolean) As Interfaces.ModuleResult Implements Interfaces.EmberTVScraperModule.Scraper
        TVScraper.SingleScrape(ShowID, ShowTitle, TVDBID, Lang, Ordering, Options, ScrapeType, WithCurrent)
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Public Function ScrapeSeason(ByVal ShowID As Integer, ByVal ShowTitle As String, ByVal TVDBID As String, ByVal iSeason As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions) As Interfaces.ModuleResult Implements Interfaces.EmberTVScraperModule.ScrapeSeason
        TVScraper.ScrapeSeason(ShowID, ShowTitle, TVDBID, iSeason, Lang, Ordering, Options)
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

#End Region 'Methods

End Class