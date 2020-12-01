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
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Xml.Linq

Namespace XMLScraper
    Namespace ScraperLib

        Public Class IncludeManager

#Region "Fields"

            Private m_Content As IncludeContent
            Private m_IncludeList As List(Of Include)

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                IncludeList = New List(Of Include)()
            End Sub

#End Region 'Constructors

#Region "Events"

            Public Event ContentChanged As EventHandler

#End Region 'Events

#Region "Properties"

            Public Property Content() As IncludeContent
                Get
                    Return m_Content
                End Get
                Set(ByVal value As IncludeContent)
                    m_Content = Value
                End Set
            End Property

            Public ReadOnly Property CurrentContentList() As List(Of Include)
                Get
                    Return IncludeList.FindAll(Function(n) n.Content = Content)
                End Get
            End Property

            Public Property IncludeList() As List(Of Include)
                Get
                    Return m_IncludeList
                End Get
                Set(ByVal value As List(Of Include))
                    m_IncludeList = Value
                End Set
            End Property

            Public ReadOnly Property XML() As String
                Get
                    Return Nothing
                End Get
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub ChangeContent(ByVal scraperContent__1 As ScraperContent)
                Select Case scraperContent__1
                    Case ScraperContent.movies, ScraperContent.tvshows, ScraperContent.musicvideos
                        Content = IncludeContent.video
                        RaiseEvent ContentChanged(Me, New EventArgs())
                    Case ScraperContent.albums
                        Content = IncludeContent.music
                        RaiseEvent ContentChanged(Me, New EventArgs())
                End Select
            End Sub

            Public Sub Load(ByVal ScraperFolder As String)
                IncludeList = New List(Of Include)()
                Dim di As New DirectoryInfo(ScraperFolder)

                For Each item As DirectoryInfo In di.GetDirectories("common", SearchOption.AllDirectories)
                    Dim tmpContent As IncludeContent = IncludeContent.video
                    Dim xScraperFunctions As XElement
                    If item.FullName.Contains("video") Then
                        tmpContent = IncludeContent.video
                    ElseIf item.FullName.Contains("music") Then
                        tmpContent = IncludeContent.music
                    End If

                    For Each fiItem As FileInfo In item.GetFiles("*.xml")
                        xScraperFunctions = XElement.Load(fiItem.FullName)

                        If xScraperFunctions.Name.ToString() = "scraperfunctions" Then
                            Dim tmp As New Include(xScraperFunctions, "common/" & fiItem.Name, tmpContent)
                            IncludeList.Add(tmp)
                        End If
                    Next
                Next
            End Sub

#End Region 'Methods

        End Class

    End Namespace
End Namespace

