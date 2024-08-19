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
Imports EmberAPI

Namespace XMLScraper
    Namespace MediaTags

        Public Class TVEpisodeTag
            Inherits MediaTag

#Region "Constructors"

            Public Sub New()
                MyBase.New()
                Clear()
            End Sub

            Public Sub New(ByVal element As XElement)
                Me.New()
                Deserialize(element)
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Actors() As List(Of PersonTag)
                Get
                    Return _People
                End Get
                Set(ByVal value As List(Of PersonTag))
                    _People = value
                End Set
            End Property

            Public Property Aired() As String
                Get
                    Return UserProperties("aired")
                End Get
                Set(ByVal value As String)
                    UserProperties("aired") = value
                End Set
            End Property

            Public Property Credits() As List(Of String)
                Get
                    Return StringLists("credits")
                End Get
                Set(ByVal value As List(Of String))
                    StringLists("credits") = value
                End Set
            End Property

            Public Property Directors() As List(Of String)
                Get
                    Return StringLists("director")
                End Get
                Set(ByVal value As List(Of String))
                    StringLists("director") = value
                End Set
            End Property

            Public Property Episode() As Integer
                Get
                    Return Convert.ToInt32(UserProperties("episode"))
                End Get
                Set(ByVal value As Integer)
                    UserProperties("episode") = value.ToString()
                End Set
            End Property

            Public Property Plot() As String
                Get
                    Return UserProperties("plot")
                End Get
                Set(ByVal value As String)
                    UserProperties("plot") = value
                End Set
            End Property

            Public Property Rating() As Single
                Get
                    Return NumUtils.ConvertToSingle(UserProperties("rating"))
                End Get
                Set(ByVal value As Single)
                    UserProperties("rating") = value.ToString("#0.0")
                End Set
            End Property

            Public Property Runtime() As String
                Get
                    Return UserProperties("runtime")
                End Get
                Set(ByVal value As String)
                    UserProperties("runtime") = value
                End Set
            End Property

            Public Property Season() As Integer
                Get
                    Return Convert.ToInt32(UserProperties("season"))
                End Get
                Set(ByVal value As Integer)
                    UserProperties("season") = value.ToString()
                End Set
            End Property

            Public Overloads Overrides ReadOnly Property TagType() As MediaType
                Get
                    Return MediaType.tvepisode
                End Get
            End Property

            Public Property Thumbs() As List(Of Thumbnail)
                Get
                    Return _Thumbs
                End Get
                Set(ByVal value As List(Of Thumbnail))
                    _Thumbs = value
                End Set
            End Property

            Public Property Votes() As Integer
                Get
                    Return Convert.ToInt32(UserProperties("votes"))
                End Get
                Set(ByVal value As Integer)
                    UserProperties("votes") = value.ToString()
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Overloads Overrides Function BlankSerialize(ByVal elementName As String) As XElement
                Throw New NotImplementedException()
            End Function

            Public Overloads Overrides Sub Clear()
                Title = ""
                Plot = ""
                Season = -1
                Episode = -1
                Aired = ""
                Runtime = ""
                Votes = 0
                Rating = 0.0

                Thumbs = New List(Of Thumbnail)()
                Directors = New List(Of String)()
                Credits = New List(Of String)()
                Actors = New List(Of PersonTag)()
            End Sub

            Public Overloads Sub Deserialize(ByVal xInfo As XDocument)
                Deserialize(xInfo.Root)
            End Sub

            Public Overloads Overrides Sub Deserialize(ByVal element As XElement)
                Title = element.GetStringElement("title", Title)
                Plot = element.GetStringElement("plot", Plot)
                Aired = element.GetStringElement("aired", Aired)
                If Not IsNothing(element.Element("season")) Then
                    Season = Convert.ToInt32(element.Element("season").Value)
                End If

                If Not IsNothing(element.Element("epnum")) Then
                    Episode = Convert.ToInt32(element.Element("epnum").Value)
                ElseIf Not IsNothing(element.Element("episode")) Then
                    Episode = Convert.ToInt32(element.Element("episode").Value)
                End If

                If Not IsNothing(element.Element("runtime")) Then
                    If Not String.IsNullOrEmpty(element.Element("runtime").Value) Then
                        Runtime = element.Element("runtime").Value
                    End If
                End If

                element.UpdateThumbList("thumb", Thumbs)

                If Not IsNothing(element.Element("rating")) Then
                    If Not String.IsNullOrEmpty(element.Element("rating").Value) Then
                        Rating = NumUtils.ConvertToSingle(element.Element("rating").Value)
                    End If
                End If

                element.UpdateStringList("credits", Credits)
                element.UpdateStringList("director", Directors)
                element.UpdatePersonList("actor", Actors)
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
                Dim element As New XElement("episode")
                element.AddStringElement("title", Title)

                element.Add(New XElement("season", Convert.ToString(Season)))
                element.Add(New XElement("episode", Convert.ToString(Episode)))

                element.AddStringElement("plot", Plot)
                element.AddStringElement("runtime", Runtime)
                element.AddStringElement("aired", Aired)

                If Rating > 0 Then
                    element.Add(New XElement("rating", Convert.ToString(Rating)))
                End If

                If Votes > 0 Then
                    element.Add(New XElement("votes", Convert.ToString(Votes)))
                End If

                element.AddThumbList("thumb", Thumbs)

                element.UpdateStringList("director", Directors)
                element.UpdateStringList("credits", Credits)
                element.AddPersonList("actor", Actors)

                Return element
            End Function

            Public Overloads Overrides Function ToString() As String
                Return Serialize("tvepisode").ToString()
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
