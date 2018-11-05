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

Public Class Master

    #Region "Fields"

    Public Shared CanScanDiscImage As Boolean
    Public Shared DB As New Database
    Public Shared currMovie As New Structures.DBMovie
    Public Shared currShow As New Structures.DBTV
    Public Shared DefaultOptions As New Structures.ScrapeOptions
    Public Shared DefaultTVOptions As New Structures.TVScrapeOptions
    'Public Shared eAdvancedSettings As New AdvancedSettings
    Public Shared eLang As New Localization
    Public Shared eLog As New ErrorLogger
    Public Shared eSettings As New Settings
    Public Shared GlobalScrapeMod As New Structures.ScrapeModifier    
    Public Shared isWindows As Boolean = Functions.CheckIfWindows
    'Public Shared MediaJobLog As New MediaLog
    Public Shared SourcesList As New List(Of String)
    Public Shared TempPath As String = Path.Combine(Functions.AppPath, "Temp")
    Public Shared tmpMovie As New MediaContainers.Movie
    Public Shared MovieSources As New List(Of Structures.MovieSource)
    Public Shared TVSources As New List(Of Structures.TVSource)
    Public Shared MajorVersion As Single = 1.3
    #End Region 'Fields

End Class