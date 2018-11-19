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
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

Namespace XMLScraper
    Namespace ScraperLib

        Public MustInherit Class ScraperNode

#Region "Fields"

            Protected Friend indent As String

            Protected Const IndentationString As String = vbTab

            Private m_Comment As String
            Private m_IsInclude As Boolean
            Private m_Name As String
            Private m_Parent As ScraperNode

#End Region 'Fields

#Region "Constructors"

            Protected Friend Sub New(ByVal ndParent As ScraperNode)
                Parent = ndParent

                If IsNothing(Parent) Then
                    indent = IndentationString
                Else
                    Me.indent = Parent.indent & IndentationString
                    Me.IsInclude = Parent.IsInclude
                End If
            End Sub

#End Region 'Constructors

#Region "Enumerations"

            Protected Friend Enum XMLDisplayOption
                RegExpOnly
                StandAlone
                Complete
                [Function]
            End Enum

#End Region 'Enumerations

#Region "Properties"

            Public Property Comment() As String
                Get
                    Return m_Comment
                End Get
                Set(ByVal value As String)
                    m_Comment = value
                End Set
            End Property

            Public ReadOnly Property Index() As Integer
                Get
                    Return GetIndex()
                End Get
            End Property

            Public Property IsInclude() As Boolean
                Get
                    Return m_IsInclude
                End Get
                Set(ByVal value As Boolean)
                    m_IsInclude = value
                End Set
            End Property

            Public Property Name() As String
                Get
                    Return m_Name
                End Get
                Set(ByVal value As String)
                    m_Name = value
                End Set
            End Property

            Public Property Parent() As ScraperNode
                Get
                    Return m_Parent
                End Get
                Private Set(ByVal value As ScraperNode)
                    m_Parent = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Overridable Function CollectSettings() As List(Of SettingsUsed)
                Return New List(Of SettingsUsed)()
            End Function

            Public Overridable Sub Deserialize(ByVal xNodeInfo As XElement)
                Me.Name = xNodeInfo.Name.ToString()
            End Sub

            Public MustOverride Function GetEvaluation(ByVal parentPath As String) As List(Of ScraperEvaluation)

            Protected Friend MustOverride Function GetIndex() As Integer

#End Region 'Methods

        End Class

    End Namespace
End Namespace
