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
    Namespace Utilities

        Public Class UrlInfo

#Region "Fields"

            Private _cache As String
            Private _function As String
            Private _isgzip As Boolean
            Private _post As Boolean
            Private _referrer As String
            Private _season As Integer
            Private _type As UrlTypes
            Private _url As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                Me.Clear()
            End Sub

            Public Sub New(ByVal xUrlInfo As XElement)
                Me.Clear()
                Deserialize(xUrlInfo)
            End Sub

#End Region 'Constructors

#Region "Enumerations"

            Public Enum UrlTypes
                general = 1
                season = 2
            End Enum

#End Region 'Enumerations

#Region "Properties"

            Public Property Cache() As String
                Get
                    Return Me._cache
                End Get
                Set(ByVal value As String)
                    Me._cache = value
                End Set
            End Property

            Public ReadOnly Property IsCustomFunctionCall() As Boolean
                Get
                    If Not String.IsNullOrEmpty(Me._function) Then
                        Return True
                    Else
                        Return False
                    End If
                End Get
            End Property

            Public Property IsGZip() As Boolean
                Get
                    Return Me._isgzip
                End Get
                Set(ByVal value As Boolean)
                    Me._isgzip = value
                End Set
            End Property

            Public ReadOnly Property IsRelativePath() As Boolean
                Get
                    If Me._url.Contains("://") Then
                        Return False
                    Else
                        Return True
                    End If
                End Get
            End Property

            Public Property Post() As Boolean
                Get
                    Return Me._post
                End Get
                Set(ByVal value As Boolean)
                    Me._post = value
                End Set
            End Property

            Public ReadOnly Property PostData() As String
                Get
                    If Me._post Then
                        Return Me._url.Substring(Me._url.LastIndexOf("?") + 1)
                    Else
                        Return Nothing
                    End If
                End Get
            End Property

            Public Property Referrer() As String
                Get
                    Return Me._referrer
                End Get
                Set(ByVal value As String)
                    Me._referrer = value
                End Set
            End Property

            Public Property Season() As Integer
                Get
                    Return Me._season
                End Get
                Set(ByVal value As Integer)
                    Me._season = value
                End Set
            End Property

            Public Property Type() As UrlTypes
                Get
                    Return Me._type
                End Get
                Set(ByVal value As UrlTypes)
                    Me._type = value
                End Set
            End Property

            Public Property Url() As String
                Get
                    Return Me._url
                End Get
                Set(ByVal value As String)
                    Me._url = value
                End Set
            End Property

            Public Property [Function]() As String
                Get
                    Return Me._function
                End Get
                Set(ByVal value As String)
                    Me._function = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Shared Function BlankSerialize(ByVal type As UrlTypes) As XElement
                Dim tmp As XElement = Nothing
                Select Case type
                    Case UrlTypes.general
                        tmp = New XElement("url")
                        tmp.Add(New XAttribute("gzip", "yes"))
                        tmp.Add(New XAttribute("post", "yes"))
                        tmp.Add(New XAttribute("spoof", "http://www.foo.com"))
                        Exit Select
                    Case UrlTypes.season
                        tmp = New XElement("url")
                        tmp.Add(New XAttribute("gzip", "yes"))
                        tmp.Add(New XAttribute("post", "yes"))
                        tmp.Add(New XAttribute("spoof", "http://www.foo.com"))
                        tmp.Add(New XAttribute("type", "season"))
                        tmp.Add(New XAttribute("season", 0))
                        Exit Select
                End Select
                Return tmp
            End Function

            Public Shared Function blankSerializeCustomFunction() As XElement
                Dim tmp As New XElement("url")
                tmp.Add(New XAttribute("gzip", "yes"))
                tmp.Add(New XAttribute("post", "yes"))
                tmp.Add(New XAttribute("spoof", "http://www.foo.com"))
                tmp.Add(New XAttribute("function", "CustomFunctionName"))
                Return tmp
            End Function

            Public Shared Function UrlDecode(ByVal strHTML As String) As String
                Return System.Web.HttpUtility.UrlDecode(strHTML)
            End Function

            Public Shared Function UrlEncode(ByVal strHTML As String) As String
                Return System.Web.HttpUtility.UrlEncode(strHTML)
            End Function

            Public Sub Clear()
                Me._post = False
                Me._cache = String.Empty
                Me._url = String.Empty
                Me._referrer = String.Empty
                Me._isgzip = False
                Me._season = -1
                Me._type = UrlTypes.general
            End Sub

            Public Overridable Sub Deserialize(ByVal xUrl As XElement)
                Me._url = xUrl.Value

                If Not IsNothing(xUrl.Attribute("spoof")) Then
                    Me._referrer = xUrl.Attribute("spoof").Value
                End If

                If Not IsNothing(xUrl.Attribute("cache")) Then
                    Me._cache = xUrl.Attribute("cache").Value
                End If

                If Not IsNothing(xUrl.Attribute("post")) Then
                    If xUrl.Attribute("post").Value = "yes" Then
                        Me._post = True
                    Else
                        Me._post = False
                    End If
                End If

                If Not IsNothing(xUrl.Attribute("gzip")) Then
                    If xUrl.Attribute("gzip").Value = "yes" Then
                        Me._isgzip = True
                    End If
                End If

                If Not IsNothing(xUrl.Attribute("function")) Then
                    Me._function = xUrl.Attribute("function").Value
                End If
            End Sub

            Public Overridable Function Serialize(ByVal elementName As String) As XElement
                Dim tmp As New XElement(elementName, Me._url)
                If String.IsNullOrEmpty(Me._referrer) <> True Then
                    tmp.Add(New XAttribute("spoof", Me._referrer))
                End If

                If String.IsNullOrEmpty(Me._cache) <> True Then
                    tmp.Add(New XAttribute("cache", Me._cache))
                End If

                If Me._post Then
                    tmp.Add(New XAttribute("post", "yes"))
                End If

                If Not String.IsNullOrEmpty(Me._function) Then
                    tmp.Add(New XAttribute("function", Me._function))
                End If

                Return tmp
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
