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
Imports EmberScraperModule.XMLScraper.Utilities

Namespace XMLScraper
    Namespace MediaTags

        Public Class ArtistTag
            Inherits MediaTag

#Region "Fields"

            Private m_Discography As List(Of AlbumTag)

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                MyBase.New()
                Clear()
            End Sub

            Public Sub New(ByVal element As XElement)
                Me.New()
                Deserialize(element)
            End Sub

            Public Sub New(ByVal xmlFilePath As String)
                Me.New()
                Deserialize(xmlFilePath)
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Aliases() As List(Of String)
                Get
                    Return MyBase.StringLists("alias")
                End Get
                Set(ByVal value As List(Of String))
                    MyBase.StringLists("alias") = Value
                End Set
            End Property

            Public Property Biography() As String
                Get
                    Return MyBase.UserProperties("biography")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("biography") = value
                End Set
            End Property

            Public Property Born() As String
                Get
                    Return MyBase.UserProperties("born")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("born") = value
                End Set
            End Property

            Public Property Died() As String
                Get
                    Return MyBase.UserProperties("died")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("died") = value
                End Set
            End Property

            Public Property Disbanded() As String
                Get
                    Return MyBase.UserProperties("disbanded")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("disbanded") = value
                End Set
            End Property

            Public Property Discography() As List(Of AlbumTag)
                Get
                    Return m_Discography
                End Get
                Private Set(ByVal value As List(Of AlbumTag))
                    m_Discography = Value
                End Set
            End Property

            Public Property Fanart() As Fanart
                Get
                    Return MyBase._Fanart
                End Get
                Set(ByVal value As Fanart)
                    MyBase._Fanart = value
                End Set
            End Property

            Public Property Formed() As String
                Get
                    Return MyBase.UserProperties("formed")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("formed") = value
                End Set
            End Property

            Public Property Genres() As List(Of String)
                Get
                    Return MyBase.StringLists("genre")
                End Get
                Set(ByVal value As List(Of String))
                    MyBase.StringLists("genre") = Value
                End Set
            End Property

            Public Property Instruments() As List(Of [String])
                Get
                    Return MyBase.StringLists("instrument")
                End Get
                Set(ByVal value As List(Of [String]))
                    MyBase.StringLists("instrument") = Value
                End Set
            End Property

            Public Property Moods() As List(Of [String])
                Get
                    Return MyBase.StringLists("mood")
                End Get
                Set(ByVal value As List(Of [String]))
                    MyBase.StringLists("mood") = Value
                End Set
            End Property

            Public Property Name() As String
                Get
                    Return MyBase.UserProperties("title")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("title") = value
                End Set
            End Property

            Public Property Styles() As List(Of [String])
                Get
                    Return MyBase.StringLists("style")
                End Get
                Set(ByVal value As List(Of [String]))
                    MyBase.StringLists("style") = Value
                End Set
            End Property

            Public Overloads Overrides ReadOnly Property TagType() As MediaType
                Get
                    Return MediaType.artist
                End Get
            End Property

            Public Property Thumbs() As List(Of Thumbnail)
                Get
                    Return MyBase._Thumbs
                End Get
                Set(ByVal value As List(Of Thumbnail))
                    MyBase._Thumbs = Value
                End Set
            End Property

            Public Property YearsActive() As String
                Get
                    Return MyBase.UserProperties("yearsactive")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("yearsactive") = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Overloads Overrides Function BlankSerialize(ByVal elementName As String) As XElement
                Dim tmp As New XElement(elementName)
                tmp.Add(New XElement("name"))
                tmp.Add(New XElement("alias"))
                tmp.Add(New XElement("alias"))
                tmp.Add(New XElement("born"))
                tmp.Add(New XElement("died"))
                tmp.Add(New XElement("formed"))
                tmp.Add(New XElement("disbanded"))
                tmp.Add(New XElement("yearsactive"))
                tmp.Add(New XElement("biography"))
                tmp.Add(New XElement("genre"))
                tmp.Add(New XElement("genre"))
                tmp.Add(New XElement("mood"))
                tmp.Add(New XElement("mood"))
                tmp.Add(New XElement("style"))
                tmp.Add(New XElement("style"))
                tmp.Add(New XElement("instrument"))
                tmp.Add(Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general))
                tmp.Add(Thumbnail.BlankSerialize(UrlInfo.UrlTypes.general))

                Return tmp
            End Function

            Public Overloads Overrides Sub Clear()
                Name = ""
                Born = ""
                Died = ""
                Formed = ""
                Disbanded = ""
                YearsActive = ""
                Biography = ""
                Discography = New List(Of AlbumTag)()
                Genres = New List(Of [String])()
                Styles = New List(Of [String])()
                Moods = New List(Of [String])()
                Aliases = New List(Of String)()
                Instruments = New List(Of [String])()
                Fanart = New Fanart()
                Thumbs = New List(Of Thumbnail)()
                Me._People = Nothing
            End Sub

            Public Overloads Sub Deserialize(ByVal xInfo As XDocument)
                Deserialize(xInfo.Root)
            End Sub

            Public Overloads Overrides Sub Deserialize(ByVal element As XElement)
                Name = element.GetStringElement("name", Name)
                element.UpdateStringList("alias", Aliases)
                Biography = element.GetStringElement("biography", Biography)
                Born = element.GetStringElement("born", Born)
                Died = element.GetStringElement("died", Died)
                Formed = element.GetStringElement("formed", Formed)
                Disbanded = element.GetStringElement("disbanded", Disbanded)
                YearsActive = element.GetStringElement("yearsactive", YearsActive)
                element.UpdateStringList("genre", Genres)
                element.UpdateStringList("mood", Moods)
                element.UpdateStringList("style", Styles)
                element.UpdateStringList("instrument", Instruments)
                element.UpdateThumbList("thumb", Thumbs)

                For Each albumItem As XElement In element.Elements("album")
                    Dim newAlbum As New AlbumTag(albumItem)
                    Dim oldAlbum As AlbumTag = Discography.Find(Function(n) n.Title = newAlbum.Title)

                    If Not IsNothing(oldAlbum) Then
                        oldAlbum.CompareAndUpdate(newAlbum)
                    Else
                        Discography.Add(newAlbum)
                    End If
                Next

                If Not IsNothing(element.Element("fanart")) Then
                    Fanart = New Fanart(element.Element("fanart"))
                End If
            End Sub

            Public Overloads Overrides Sub Deserialize(ByVal xmlFilePath As String)
                Dim tmpDocument As XDocument = XDocument.Load(xmlFilePath)
                Deserialize(tmpDocument)
            End Sub

            Public Overloads Function Serialize(ByVal rootElementName As String, ByVal xmlVersion As String, ByVal xmlEncoding As String, ByVal standalone As String) As XDocument
                Dim tmp As New XDocument(New XDeclaration(xmlVersion, xmlEncoding, standalone), New Object() {Serialize(rootElementName)})
                Return tmp
            End Function

            Public Overloads Function Serialize(ByVal elementName As String) As XElement
                Dim tmp As New XElement(elementName)
                tmp.AddStringElement("name", Name)
                tmp.AddStringList("alias", Aliases)
                tmp.AddStringElement("born", Born)
                tmp.AddStringElement("died", Died)
                tmp.AddStringElement("formed", Formed)
                tmp.AddStringElement("disbanded", Disbanded)
                tmp.AddStringElement("yearsactive", YearsActive)
                tmp.AddStringElement("biography", Biography)
                tmp.AddStringList("genre", Genres)
                tmp.AddStringList("mood", Moods)
                tmp.AddStringList("style", Styles)
                tmp.AddStringList("instrument", Instruments)
                tmp.AddThumbList("thumb", Thumbs)

                For Each albumItem As AlbumTag In Discography
                    tmp.Add(New XElement(albumItem.Serialize("album")))
                Next

                If Fanart.Count > 0 Then
                    tmp.Add(New XElement(Fanart.Serialize("fanart")))
                End If

                Return tmp
            End Function

            Public Overloads Overrides Function ToString() As String
                Return Serialize("artist").ToString()
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
