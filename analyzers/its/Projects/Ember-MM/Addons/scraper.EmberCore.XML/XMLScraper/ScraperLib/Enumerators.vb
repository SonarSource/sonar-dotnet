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

Namespace XMLScraper
    Namespace ScraperLib

#Region "Enumerations"

        Public Enum FunctionType
            NfoUrl = 0
            CreateSearchUrl = 1
            GetSearchResults = 2
            GetDetails = 3
            GetList = 4
            GetListDetails = 5
            CustomFunction = 6
        End Enum

        Public Enum IncludeContent
            music = 0
            video = 1
        End Enum

        Public Enum MediaType
            album = 0
            artist = 1
            movie = 2
            musicvideo = 3
            person = 4
            tvshow = 5
            tvepisode = 6
        End Enum

        Public Enum MissingSettingType
            [boolean]
            variable
        End Enum

        Public Enum ScraperContent
            albums = 0
            movies = 1
            musicvideos = 1
            tvshows = 2
        End Enum

        Public Enum ScraperFunctionType
            NfoUrl = 0
            CreateSearchUrl = 1
            GetSearchResults = 2
            GetDetails = 3
            GetList = 4
            CustomFunction = 5
        End Enum

#End Region 'Enumerations

    End Namespace
End Namespace
