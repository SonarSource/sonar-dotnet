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
Imports System.Xml.Linq

Imports EmberScraperModule.XMLScraper.ScraperLib
Imports EmberScraperModule.XMLScraper.ScraperXML
Imports EmberScraperModule.XMLScraper.Utilities

Namespace XMLScraper
    Namespace MediaTags

        Public Class AlbumTag
            Inherits MediaTag

#Region "Fields"

            Private _tracks As List(Of AlbumTrack)

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                Me.Clear()
            End Sub

            Public Sub New(ByVal element As XElement)
                Me.Clear()
                Deserialize(element)
            End Sub

            Public Sub New(ByVal albumTitle As String, ByVal artistName As String)
                Title = albumTitle
                Artist = artistName
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Artist() As String
                Get
                    Return MyBase.UserProperties("artist")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("artist") = value
                End Set
            End Property

            Public Property Genres() As List(Of String)
                Get
                    Return StringLists("genre")
                End Get
                Private Set(ByVal value As List(Of String))
                    StringLists("genre") = value
                End Set
            End Property

            Public Property Label() As String
                Get
                    Return MyBase.UserProperties("label")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("label") = value
                End Set
            End Property

            Public Property Moods() As List(Of [String])
                Get
                    Return StringLists("mood")
                End Get
                Private Set(ByVal value As List(Of [String]))
                    StringLists("mood") = value
                End Set
            End Property

            Public Property ReleaseDate() As String
                Get
                    Return MyBase.UserProperties("releasedate")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("releasedate") = value
                End Set
            End Property

            Public Property Review() As String
                Get
                    Return MyBase.UserProperties("review")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("review") = value
                End Set
            End Property

            Public Property Styles() As List(Of [String])
                Get
                    Return StringLists("style")
                End Get
                Private Set(ByVal value As List(Of [String]))
                    StringLists("style") = value
                End Set
            End Property

            Public Overloads Overrides ReadOnly Property TagType() As MediaType
                Get
                    Return MediaType.album
                End Get
            End Property

            Public Property Themes() As List(Of [String])
                Get
                    Return StringLists("theme")
                End Get
                Private Set(ByVal value As List(Of [String]))
                    StringLists("theme") = value
                End Set
            End Property

            Public Property Thumbs() As List(Of Thumbnail)
                Get
                    Return MyBase._Thumbs
                End Get
                Private Set(ByVal value As List(Of Thumbnail))
                    MyBase._Thumbs = value
                End Set
            End Property

            Public Property Tracks() As List(Of AlbumTrack)
                Get
                    Return Me._tracks
                End Get
                Private Set(ByVal value As List(Of AlbumTrack))
                    Me._tracks = value
                End Set
            End Property

            Public Property Type() As String
                Get
                    Return MyBase.UserProperties("type")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("type") = value
                End Set
            End Property

            Public Property Year() As Integer
                Get
                    Return Convert.ToInt32(MyBase.UserProperties("year"))
                End Get
                Set(ByVal value As Integer)
                    MyBase.UserProperties("year") = value.ToString("0000")
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Overloads Overrides Function BlankSerialize(ByVal elementName As String) As XElement
                Dim tmp As New XElement(elementName)
                tmp.Add(New XElement("title"))
                tmp.Add(New XElement("artist"))
                tmp.Add(New XElement("type"))
                tmp.Add(New XElement("review"))
                tmp.Add(New XElement("year"))
                tmp.Add(New XElement("releasedate"))
                tmp.Add(New XElement("style"))
                tmp.Add(New XElement("style"))
                tmp.Add(New XElement("theme"))
                tmp.Add(New XElement("theme"))
                tmp.Add(New XElement("mood"))
                tmp.Add(New XElement("mood"))
                tmp.Add(New XElement("genre"))
                tmp.Add(New XElement("genre"))
                tmp.Add(Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general))
                tmp.Add(Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general))

                Return tmp
            End Function

            Public Overloads Overrides Sub Clear()
                Title = ""
                Artist = ""
                Label = ""
                Type = ""
                ReleaseDate = ""
                Review = ""
                Year = 0
                Thumbs = New List(Of Thumbnail)()
                Genres = New List(Of [String])()
                Styles = New List(Of [String])()
                Moods = New List(Of [String])()
                Themes = New List(Of [String])()
                Tracks = New List(Of AlbumTrack)()
                Me._People = Nothing
                Me._Fanart = Nothing
            End Sub

            Public Function CompareAndUpdate(ByVal otherAlbum As AlbumTag) As Boolean
                Dim changed As Boolean = False
                If [String].Compare(Title, otherAlbum.Title, StringComparison.OrdinalIgnoreCase) = 0 Then
                    If String.IsNullOrEmpty(Label) AndAlso Not String.IsNullOrEmpty(otherAlbum.Label) Then
                        Label = otherAlbum.Label
                        changed = True
                    End If

                    If otherAlbum.Year > 0 Then
                        Year = otherAlbum.Year
                        changed = True

                    End If
                End If
                Return changed
            End Function

            Public Overloads Overrides Sub Deserialize(ByVal element As XElement)
                Title = element.GetStringElement("title", Title)
                Artist = element.GetStringElement("artist", Artist)
                Label = element.GetStringElement("label", Label)
                Type = element.GetStringElement("type", Type)
                ReleaseDate = element.GetStringElement("releaseddate", ReleaseDate)
                Year = element.GetIntElement("year", Year)
                Review = element.GetStringElement("review", Review)

                element.UpdateThumbList("thumb", Thumbs)

                element.UpdateStringList("genre", Genres)
                element.UpdateStringList("style", Styles)
                element.UpdateStringList("mood", Moods)
                element.UpdateStringList("theme", Themes)

                For Each item As XElement In element.Elements("track")
                    Dim newtrack As New AlbumTrack(item)
                    If Not Tracks.Contains(newtrack) Then
                        Tracks.Add(newtrack)
                    End If
                Next
            End Sub

            Public Overloads Sub Deserialize(ByVal xInfo As XDocument)
                Deserialize(xInfo.Root)
            End Sub

            Public Overloads Overrides Sub Deserialize(ByVal xmlFilePath As String)
                Dim tmpDocument As XDocument = XDocument.Load(xmlFilePath)
                Deserialize(tmpDocument)
            End Sub

            Public Function Functions() As Dictionary(Of FunctionType, FunctionInformation)
                Dim tmp As New Dictionary(Of FunctionType, FunctionInformation)()
                tmp.Add(FunctionType.NfoUrl, New FunctionInformation(FunctionType.NfoUrl, MediaType.album))
                tmp.Add(FunctionType.CreateSearchUrl, New FunctionInformation(FunctionType.CreateSearchUrl, MediaType.album))
                tmp.Add(FunctionType.GetSearchResults, New FunctionInformation(FunctionType.GetSearchResults, MediaType.album))
                tmp.Add(FunctionType.GetDetails, New FunctionInformation(FunctionType.GetDetails, MediaType.album))
                tmp.Add(FunctionType.CustomFunction, New FunctionInformation(FunctionType.CustomFunction, MediaType.album))
                Return tmp
            End Function

            Public Overloads Function Serialize(ByVal elementName As String) As XElement
                Dim tmp As New XElement(elementName)

                tmp.AddStringElement("title", Title)
                tmp.AddStringElement("artist", Artist)
                tmp.AddStringElement("label", Label)
                tmp.AddStringElement("type", Type)
                tmp.AddStringElement("releasedate", ReleaseDate)
                tmp.AddIntElement("year", Year, 1)
                tmp.AddStringElement("review", Review)
                tmp.AddThumbList("thumb", Thumbs)
                tmp.AddStringList("genre", Genres)
                tmp.AddStringList("style", Styles)
                tmp.AddStringList("mood", Moods)
                tmp.AddStringList("theme", Themes)

                For Each item As AlbumTrack In Tracks
                    tmp.Add(item.Serialize("track"))
                Next

                Return tmp
            End Function

            Public Overloads Function Serialize(ByVal rootElementName As String, ByVal xmlVersion As String, ByVal xmlEncoding As String, ByVal standalone As String) As XDocument
                Dim tmp As New XDocument(New XDeclaration(xmlVersion, xmlEncoding, standalone), New Object() {Serialize(rootElementName)})
                Return tmp
            End Function

            Public Overloads Overrides Function ToString() As String
                Return Serialize("album").ToString()
            End Function

#End Region 'Methods

        End Class

        Public Class AlbumTrack
            Implements IComparable(Of AlbumTrack)

#Region "Fields"

            Private _duration As String
            Private _position As Integer
            Private _title As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                Me.Clear()
            End Sub

            Public Sub New(ByVal element As XElement)
                Me.Clear()
                Deserialize(element)
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Duration() As String
                Get
                    Return _duration
                End Get
                Set(ByVal value As String)
                    _duration = value
                End Set
            End Property

            Public Property Position() As Integer
                Get
                    Return _position
                End Get
                Set(ByVal value As Integer)
                    _position = value
                End Set
            End Property

            Public Property Title() As String
                Get
                    Return _title
                End Get
                Set(ByVal value As String)
                    _title = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub Clear()
                _position = 0
                _title = String.Empty
                _duration = String.Empty
            End Sub

            Public Function CompareTo(ByVal other As AlbumTrack) As Integer Implements IComparable(Of AlbumTrack).CompareTo
                If IsNothing(Me) Then
                    If IsNothing(other) Then
                        Return 0
                    Else
                        Return 1
                    End If
                Else
                    If IsNothing(other) Then
                        Return -1
                    Else
                        Return Me.Position.CompareTo(other.Position)
                    End If
                End If
            End Function

            Public Sub Deserialize(ByVal xInfo As XDocument)
                Deserialize(xInfo.Root)
            End Sub

            Public Sub Deserialize(ByVal element As XElement)
                _position = element.GetIntElement("position", _position)
                _title = element.GetStringElement("title", _title)
                _duration = element.GetStringElement("duration", _duration)
            End Sub

            Public Sub Deserialize(ByVal xmlFilePath As String)
                Dim tmpDocument As XDocument = XDocument.Load(xmlFilePath)
                Deserialize(tmpDocument)
            End Sub

            Public Function Serialize(ByVal rootElementName As String, ByVal xmlVersion As String, ByVal xmlEncoding As String, ByVal standalone As String) As XDocument
                Dim tmp As New XDocument(New XDeclaration(xmlVersion, xmlEncoding, standalone), New Object() {Serialize(rootElementName)})
                Return tmp
            End Function

            Public Function Serialize(ByVal elementName As String) As XElement
                Dim tmp As New XElement("track")
                tmp.AddIntElement("position", _position, 1)
                tmp.AddStringElement("title", _title)
                tmp.AddStringElement("duration", _duration)
                Return tmp
            End Function

            Public Overloads Overrides Function ToString() As String
                Return Serialize("track").ToString()
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
