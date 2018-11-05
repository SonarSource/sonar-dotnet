Imports System.IO

Imports EmberAPI

Public Class EmberExtrasMovieScraperModule
    Implements Interfaces.EmberMovieScraperModule

#Region "Fields"

    Private _Name As String = "Ember Extras Scraper"

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

        End Get
    End Property

    Public ReadOnly Property IsScraper() As Boolean Implements Interfaces.EmberMovieScraperModule.IsScraper
        Get

        End Get
    End Property

    Public ReadOnly Property ModuleName() As String Implements Interfaces.EmberMovieScraperModule.ModuleName
        Get
            Return _Name
        End Get
    End Property

    Public ReadOnly Property ModuleVersion() As String Implements Interfaces.EmberMovieScraperModule.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

    Public Property PostScraperEnabled() As Boolean Implements Interfaces.EmberMovieScraperModule.PostScraperEnabled
        Get

        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public Property ScraperEnabled() As Boolean Implements Interfaces.EmberMovieScraperModule.ScraperEnabled
        Get

        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

#End Region 'Properties

#Region "Methods"
    Function QueryPostScraperCapabilities(ByVal cap As Enums.PostScraperCapabilities) As Boolean Implements Interfaces.EmberMovieScraperModule.QueryPostScraperCapabilities

    End Function
    Public Function DownloadTrailer(ByRef DBMovie As Structures.DBMovie, ByRef sURL As String) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.DownloadTrailer
    End Function

    Public Function GetMovieStudio(ByRef DBMovie As Structures.DBMovie, ByRef sStudio As System.Collections.Generic.List(Of String)) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.GetMovieStudio
    End Function

    Public Sub Init(ByVal sAssemblyName As String) Implements Interfaces.EmberMovieScraperModule.Init
    End Sub

    Public Function InjectSetupPostScraper() As Containers.SettingsPanel Implements Interfaces.EmberMovieScraperModule.InjectSetupPostScraper
        Return (Nothing)
    End Function

    Public Function InjectSetupScraper() As Containers.SettingsPanel Implements Interfaces.EmberMovieScraperModule.InjectSetupScraper
        Return (Nothing)
    End Function

    Public Function PostScraper(ByRef DBMovie As Structures.DBMovie, ByVal ScrapeType As Enums.ScrapeType) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.PostScraper
    End Function

    Public Sub SaveSetupPostScraper(ByVal DoDispose As Boolean) Implements Interfaces.EmberMovieScraperModule.SaveSetupPostScraper
    End Sub

    Public Sub SaveSetupScraper(ByVal DoDispose As Boolean) Implements Interfaces.EmberMovieScraperModule.SaveSetupScraper
    End Sub

    Public Function Scraper(ByRef DBMovie As Structures.DBMovie, ByRef ScrapeType As Enums.ScrapeType, ByRef Options As Structures.ScrapeOptions) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.Scraper
    End Function

    Public Function SelectImageOfType(ByRef DBMovie As Structures.DBMovie, ByVal _DLType As Enums.ImageType, ByRef pResults As Containers.ImgResult, Optional ByVal _isEdit As Boolean = False, Optional ByVal preload As Boolean = False) As Interfaces.ModuleResult Implements Interfaces.EmberMovieScraperModule.SelectImageOfType
    End Function
    Public Sub PostScraperOrderChanged() Implements EmberAPI.Interfaces.EmberMovieScraperModule.PostScraperOrderChanged

    End Sub

    Public Sub ScraperOrderChanged() Implements EmberAPI.Interfaces.EmberMovieScraperModule.ScraperOrderChanged

    End Sub
#End Region 'Methods


End Class