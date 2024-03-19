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
Imports System.Text
Imports System.Xml.Linq

Imports EmberScraperModule.XMLScraper.ScraperLib
Imports EmberScraperModule.XMLScraper.Utilities
Imports EmberAPI

Namespace XMLScraper
    Namespace MediaTags

        Public MustInherit Class MediaTag

#Region "Fields"

            Protected StringLists As Dictionary(Of String, List(Of String))
            Protected UserProperties As Dictionary(Of String, String)

            Private m_LastScraped As DateTime
            Private m_LastScraper As String
            Private m_LastUpdated As DateTime
            Private m__Fanart As Fanart
            Private m__People As List(Of PersonTag)
            Private m__Thumbs As List(Of Thumbnail)

#End Region 'Fields

#Region "Constructors"

            Protected Sub New()
                Me.StringLists = New Dictionary(Of String, List(Of String))()
                Me.UserProperties = New Dictionary(Of String, String)()
                Me._Thumbs = New List(Of Thumbnail)()
                Me._Fanart = New Fanart()
                Me._People = New List(Of PersonTag)()
                Me.Title = ""
                Me.LastScraped = DateTime.Now
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property LastScraped() As DateTime
                Get
                    Return m_LastScraped
                End Get
                Set(ByVal value As DateTime)
                    m_LastScraped = value
                End Set
            End Property

            Public Property LastScraper() As String
                Get
                    Return m_LastScraper
                End Get
                Set(ByVal value As String)
                    m_LastScraper = value
                End Set
            End Property

            Public Property LastUpdated() As DateTime
                Get
                    Return m_LastUpdated
                End Get
                Set(ByVal value As DateTime)
                    m_LastUpdated = value
                End Set
            End Property

            Public MustOverride ReadOnly Property TagType() As MediaType

            Public Property Title() As String
                Get
                    Return UserProperties("title")
                End Get
                Set(ByVal value As String)
                    UserProperties("title") = value
                End Set
            End Property

            Protected Friend Property _Fanart() As Fanart
                Get
                    Return m__Fanart
                End Get
                Set(ByVal value As Fanart)
                    m__Fanart = value
                End Set
            End Property

            Protected Friend Property _People() As List(Of PersonTag)
                Get
                    Return m__People
                End Get
                Set(ByVal value As List(Of PersonTag))
                    m__People = value
                End Set
            End Property

            Protected Friend Property _Thumbs() As List(Of Thumbnail)
                Get
                    Return m__Thumbs
                End Get
                Set(ByVal value As List(Of Thumbnail))
                    m__Thumbs = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Shared Function TagFromResultsType(ByVal mediaType__1 As MediaType) As MediaTag
                Dim tag As MediaTag = Nothing

                Select Case mediaType__1
                    Case MediaType.movie
                        tag = New MovieTag()
                    Case MediaType.musicvideo
                        tag = New MusicVideoTag()
                    Case MediaType.tvshow
                        tag = New TvShowTag()
                    Case MediaType.tvepisode
                        tag = New TVEpisodeTag()
                    Case MediaType.album
                        tag = New AlbumTag()
                    Case MediaType.artist
                        tag = New ArtistTag()
                    Case MediaType.person
                        tag = New PersonTag()
                End Select

                Return tag
            End Function

            Public MustOverride Function BlankSerialize(ByVal elementName As String) As XElement

            Public MustOverride Sub Clear()

            Public MustOverride Sub Deserialize(ByVal xmlFilePath As String)

            Public MustOverride Sub Deserialize(ByVal xmlElement As XElement)

            Friend Function ProcessRating(ByVal ratingElement As XElement, ByVal defaultValue As Single) As Single
                If Not IsNothing(ratingElement) Then
                    If Not IsNothing(ratingElement.Attribute("max")) Then
                        Dim scale As Single = 10 / NumUtils.ConvertToSingle(ratingElement.Attribute("max").Value)
                        Return scale * NumUtils.ConvertToSingle(ratingElement.Value)
                    Else
                        Return NumUtils.ConvertToSingle(ratingElement.Value)
                    End If
                End If

                Return defaultValue
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
