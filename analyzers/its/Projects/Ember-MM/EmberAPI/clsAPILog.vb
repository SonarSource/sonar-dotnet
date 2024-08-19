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

Imports EmberAPI
Imports System
Imports System.IO

Public Class MediaLog
    Enum LogMediaType
        Movie = 1
        TVShow = 2
        TVSeason = 3
        TVEpisode = 4
    End Enum
    Enum LogEntryType
        Info = 1
        Warning = 2
        Critical = 3
    End Enum

    Public Saved As Boolean = False
    ' Bellow 2 vars only needed if we want back reference
    Public MediaType As LogMediaType
    Public MediaID As Long 'DB ID of Movie, Show,Season or Episode

    Public ID As Long
    Public LastDateAdd As Double
    Public LogItems As New List(Of MediaLogItem)
    Private lastID As Long = 0

    Public Class MediaLogItem
        Public Saved As Boolean = False
        Public ID As Long
        Public ItemType As LogEntryType
        Public Message As String
        Public Details As String
        Public DateAdd As Double
        Sub New()

        End Sub
    End Class
    Sub New()
        LastDateAdd = Functions.ConvertToUnixTimestamp(Now)
    End Sub
    Sub Load()
        Select Case MediaType
            Case LogMediaType.Movie
                'LoadMovieLogFromDB(Me)
            Case LogMediaType.TVShow
                'LoadTVShowLogFromDB(Me)
            Case LogMediaType.TVSeason
                'LoadTVSeasonLogFromDB(Me)
            Case LogMediaType.TVEpisode
                'LoadTVEpisodeLogFromDB(Me)
        End Select
    End Sub
    Sub AddEntry(ByVal _type As LogEntryType, ByVal _message As String, ByVal _details As String)
        lastID += 1
        LogItems.Add(New MediaLogItem With {.ID = lastID, .ItemType = _type, .Message = _message, .Details = _details, .DateAdd = Functions.ConvertToUnixTimestamp(Now)})
    End Sub
End Class
