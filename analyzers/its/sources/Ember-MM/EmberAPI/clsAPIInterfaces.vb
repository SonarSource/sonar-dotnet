Imports System.Drawing

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

Public Class Interfaces

#Region "Nested Interfaces"

    ' Interfaces for external Modules
    Public Interface EmberExternalModule

#Region "Events"

        Event GenericEvent(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))

        Event ModuleSettingsChanged()

        Event ModuleSetupChanged(ByVal Name As String, ByVal State As Boolean, ByVal diffOrder As Integer)

#End Region 'Events

#Region "Properties"

        Property Enabled() As Boolean

        ReadOnly Property ModuleName() As String

        ReadOnly Property ModuleType() As List(Of Enums.ModuleEventType)

        ReadOnly Property ModuleVersion() As String

#End Region 'Properties

#Region "Methods"

		Sub Init(ByVal sAssemblyName As String, ByVal sExecutable As String)

        Function InjectSetup() As Containers.SettingsPanel

        Function RunGeneric(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object), ByRef _refparam As Object) As ModuleResult

        Sub SaveSetup(ByVal DoDispose As Boolean)

#End Region 'Methods

    End Interface

    Public Interface EmberMovieScraperModule

#Region "Events"

        Event ModuleSettingsChanged()

        Event MovieScraperEvent(ByVal eType As Enums.MovieScraperEventType, ByVal Parameter As Object)

        Event PostScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

        Sub ScraperOrderChanged()

        Sub PostScraperOrderChanged()

#End Region 'Events

#Region "Properties"

        ReadOnly Property IsPostScraper() As Boolean

        ReadOnly Property IsScraper() As Boolean

        ReadOnly Property ModuleName() As String

        ReadOnly Property ModuleVersion() As String

        Property PostScraperEnabled() As Boolean

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function DownloadTrailer(ByRef DBMovie As Structures.DBMovie, ByRef sURL As String) As ModuleResult

        Function GetMovieStudio(ByRef DBMovie As Structures.DBMovie, ByRef sStudio As List(Of String)) As ModuleResult

        Sub Init(ByVal sAssemblyName As String)

        Function InjectSetupPostScraper() As Containers.SettingsPanel

        Function QueryPostScraperCapabilities(ByVal cap As Enums.PostScraperCapabilities) As Boolean

        Function InjectSetupScraper() As Containers.SettingsPanel

        Function PostScraper(ByRef DBMovie As Structures.DBMovie, ByVal ScrapeType As Enums.ScrapeType) As ModuleResult

        Sub SaveSetupPostScraper(ByVal DoDispose As Boolean)

        Sub SaveSetupScraper(ByVal DoDispose As Boolean)

        'Movie is byref because some scrapper may run to update only some fields (defined in Scraper Setup)
        'Options is byref to allow field blocking in scraper chain
        Function Scraper(ByRef DBMovie As Structures.DBMovie, ByRef ScrapeType As Enums.ScrapeType, ByRef Options As Structures.ScrapeOptions) As ModuleResult

        Function SelectImageOfType(ByRef DBMovie As Structures.DBMovie, ByVal _DLType As Enums.ImageType, ByRef pResults As Containers.ImgResult, Optional ByVal _isEdit As Boolean = False, Optional ByVal preload As Boolean = False) As ModuleResult

#End Region 'Methods

    End Interface

    Public Interface EmberTVScraperModule

#Region "Events"

        Event ModuleSettingsChanged()

        Event SetupPostScraperChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

        Event SetupScraperChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

        Event TVScraperEvent(ByVal eType As Enums.TVScraperEventType, ByVal iProgress As Integer, ByVal Parameter As Object)

#End Region 'Events

#Region "Properties"

        ReadOnly Property IsBusy() As Boolean

        ReadOnly Property IsPostScraper() As Boolean

        ReadOnly Property IsScraper() As Boolean

        ReadOnly Property ModuleName() As String

        ReadOnly Property ModuleVersion() As String

        Property PostScraperEnabled() As Boolean

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Sub CancelAsync()

        Function ChangeEpisode(ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Lang As String, ByRef epDet As MediaContainers.EpisodeDetails) As ModuleResult

        Function GetLangs(ByVal sMirror As String, ByRef Langs As List(Of Containers.TVLanguage)) As ModuleResult

        Function GetSingleEpisode(ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Season As Integer, ByVal Episode As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions, ByRef epDetails As MediaContainers.EpisodeDetails) As ModuleResult

        Function GetSingleImage(ByVal Title As String, ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Type As Enums.TVImageType, ByVal Season As Integer, ByVal Episode As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal CurrentImage As Image, ByRef Image As Image) As ModuleResult

        Sub Init(ByVal sAssemblyName As String)

        Function InjectSetupPostScraper() As Containers.SettingsPanel

        Function InjectSetupScraper() As Containers.SettingsPanel

        Function PostScraper(ByRef DBTV As Structures.DBTV, ByVal ScrapeType As Enums.ScrapeType) As ModuleResult

        Function SaveImages() As ModuleResult

        Sub SaveSetupPostScraper(ByVal DoDispose As Boolean)

        Sub SaveSetupScraper(ByVal DoDispose As Boolean)

        Function ScrapeEpisode(ByVal ShowID As Integer, ByVal ShowTitle As String, ByVal TVDBID As String, ByVal iEpisode As Integer, ByVal iSeason As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions) As ModuleResult

        Function Scraper(ByVal ShowID As Integer, ByVal ShowTitle As String, ByVal TVDBID As String, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions, ByVal ScrapeType As Enums.ScrapeType, ByVal WithCurrent As Boolean) As ModuleResult

        Function ScrapeSeason(ByVal ShowID As Integer, ByVal ShowTitle As String, ByVal TVDBID As String, ByVal iSeason As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions) As ModuleResult

#End Region 'Methods

    End Interface

#End Region 'Nested Interfaces

#Region "Nested Types"

    Public Structure ModuleResult

#Region "Fields"

        Public breakChain As Boolean
        Public Cancelled As Boolean
        Public BoolProperty As Boolean

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class