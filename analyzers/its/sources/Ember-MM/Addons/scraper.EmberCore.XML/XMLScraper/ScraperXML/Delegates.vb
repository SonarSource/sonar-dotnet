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

Imports EmberScraperModule.XMLScraper.MediaTags
Imports EmberScraperModule.XMLScraper.ScraperLib
Imports EmberScraperModule.XMLScraper.Utilities
Imports EmberAPI

Namespace XMLScraper
    Namespace ScraperXML

        Public Class DetailsRetrievedEventArgs
            Inherits EventArgs

#Region "Fields"

            Private _details As MediaTag
            Private _searchentity As ScrapeResultsEntity

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal entity As ScrapeResultsEntity, ByVal tag As MediaTag)
                Me._searchentity = entity
                Me._details = tag
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Details() As MediaTag
                Get
                    Return Me._details
                End Get
                Private Set(ByVal value As MediaTag)
                    Me._details = value
                End Set
            End Property

            Public Property SearchEntity() As ScrapeResultsEntity
                Get
                    Return Me._searchentity
                End Get
                Private Set(ByVal value As ScrapeResultsEntity)
                    Me._searchentity = value
                End Set
            End Property

#End Region 'Properties

        End Class

        Public Class LogInfoEventArgs
            Inherits EventArgs

#Region "Fields"

            Private _message As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal message__1 As String)
                Message = message__1
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Message() As String
                Get
                    Return Me._message
                End Get
                Set(ByVal value As String)
                    Me._message = value
                End Set
            End Property

#End Region 'Properties

        End Class

        Public Class SearchProgressChangedEventArgs
            Inherits EventArgs

#Region "Fields"

            Private _currentitemprocessing As Integer
            Private _progresspercentage As Integer
            Private _scrapername As String
            Private _totalitemstoprocess As Integer

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal totalToProcess As Integer, ByVal currentlyProcessing As Integer, ByVal nameOfScraper As String)
                Me._scrapername = nameOfScraper
                Me._totalitemstoprocess = totalToProcess
                Me._currentitemprocessing = currentlyProcessing
                Dim TrueProgress As Single = Me._currentitemprocessing * (100 / NumUtils.ConvertToSingle(Me._totalitemstoprocess.ToString))
                Me._progresspercentage = Convert.ToInt32(Math.Truncate(TrueProgress))
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property CurrentItemProcessing() As Integer
                Get
                    Return Me._currentitemprocessing
                End Get
                Friend Set(ByVal value As Integer)
                    Me._currentitemprocessing = value
                End Set
            End Property

            Public Property ProgressPercentage() As Integer
                Get
                    Return Me._progresspercentage
                End Get
                Friend Set(ByVal value As Integer)
                    Me._progresspercentage = value
                End Set
            End Property

            Public Property ScraperName() As String
                Get
                    Return Me._scrapername
                End Get
                Friend Set(ByVal value As String)
                    Me._scrapername = value
                End Set
            End Property

            Public Property TotalItemsToProcess() As Integer
                Get
                    Return Me._totalitemstoprocess
                End Get
                Friend Set(ByVal value As Integer)
                    Me._totalitemstoprocess = value
                End Set
            End Property

#End Region 'Properties

        End Class

        Public Class SearchResultsRetrievedEventArgs
            Inherits EventArgs

#Region "Fields"

            Private _primary As String
            Private _results As List(Of ScrapeResultsEntity)
            Private _scrapername As String
            Private _secondary As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal scraper As String, ByVal primary__1 As String, ByVal secondary__2 As String, ByVal results__3 As List(Of ScrapeResultsEntity))
                Me._scrapername = scraper
                Me._primary = primary__1
                Me._secondary = secondary__2
                Me._results = results__3
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Primary() As String
                Get
                    Return Me._primary
                End Get
                Private Set(ByVal value As String)
                    Me._primary = value
                End Set
            End Property

            Public Property Results() As List(Of ScrapeResultsEntity)
                Get
                    Return Me._results
                End Get
                Private Set(ByVal value As List(Of ScrapeResultsEntity))
                    Me._results = value
                End Set
            End Property

            Public Property ScraperName() As String
                Get
                    Return Me._scrapername
                End Get
                Private Set(ByVal value As String)
                    Me._scrapername = value
                End Set
            End Property

            Public Property Secondary() As String
                Get
                    Return Me._secondary
                End Get
                Private Set(ByVal value As String)
                    Me._secondary = value
                End Set
            End Property

#End Region 'Properties

        End Class

    End Namespace
End Namespace

