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

Imports System.Xml.Linq

Namespace XMLScraper
    Namespace ScraperXML

        Public NotInheritable Class IncludeInfo

#Region "Fields"

            Private _document As XElement
            Private _name As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal doc As XDocument, ByVal xmlFilePath As String)
                Me.Clear()
                Me._document = doc.Root
                Me._name = String.Concat("common/", xmlFilePath.Substring(xmlFilePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1))
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Document() As XElement
                Get
                    Return Me._document
                End Get
                Set(ByVal value As XElement)
                    Me._document = value
                End Set
            End Property

            Public Property Name() As String
                Get
                    Return Me._name
                End Get
                Set(ByVal value As String)
                    Me._name = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub Clear()
                Me._name = String.Empty
                Me._document = Nothing
            End Sub

#End Region 'Methods

        End Class

    End Namespace
End Namespace
