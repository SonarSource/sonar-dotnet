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
Imports System.Xml.Linq

Namespace XMLScraper
    Namespace ScraperLib

        Public Class Include
            Inherits ScraperFunctionContainer

#Region "Fields"

            Private m_Content As IncludeContent

#End Region 'Fields

#Region "Constructors"

            Public Sub New(ByVal xScraperFunctions As XElement, ByVal name As String, ByVal Content As IncludeContent)
                MyBase.New()
                Me.Content = Content
                For Each item As XElement In xScraperFunctions.Elements()
                    ScraperFunctions.Add(New ScraperFunction(item, Me))
                Next
                Me.Name = name
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Content() As IncludeContent
                Get
                    Return m_Content
                End Get
                Set(ByVal value As IncludeContent)
                    m_Content = value
                End Set
            End Property

            Public ReadOnly Property FriendlyName() As String
                Get
                    Return Name.Replace("common/", "")
                End Get
            End Property

#End Region 'Properties

        End Class

    End Namespace
End Namespace
