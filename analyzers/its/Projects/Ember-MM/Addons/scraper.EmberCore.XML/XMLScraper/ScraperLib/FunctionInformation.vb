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

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Imports EmberScraperModule.XMLScraper.MediaTags

Namespace XMLScraper
    Namespace ScraperLib

        Public Structure BufferContents

#Region "Fields"

            Private _Buffer As Integer
            Private _Contents As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal intBuffer As Integer, ByVal strContents As String)
                Contents = strContents
                Buffer = intBuffer
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Buffer() As Integer
                Get
                    Return _Buffer
                End Get
                Set(ByVal value As Integer)
                    _Buffer = value
                End Set
            End Property

            Public Property Contents() As String
                Get
                    Return _Contents
                End Get
                Set(ByVal value As String)
                    _Contents = value
                End Set
            End Property

#End Region 'Properties

        End Structure

        Public Class FunctionInformation

#Region "Fields"

            Private _FunctionType As FunctionType
            Private _Tag As MediaTag

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal typeFunction As FunctionType, ByVal typeMedia As MediaType)
                [Set](typeMedia, typeFunction)
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property FunctionType() As FunctionType
                Get
                    Return _FunctionType
                End Get
                Private Set(ByVal value As FunctionType)
                    _FunctionType = value
                End Set
            End Property

            Private Property Tag() As MediaTag
                Get
                    Return _Tag
                End Get
                Set(ByVal value As MediaTag)
                    _Tag = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub [Set](ByVal typeMedia As MediaType, ByVal typeFunction As FunctionType)
                Me.FunctionType = typeFunction
                Me.Tag = MediaTag.TagFromResultsType(typeMedia)
            End Sub

#End Region 'Methods

        End Class

    End Namespace
End Namespace
