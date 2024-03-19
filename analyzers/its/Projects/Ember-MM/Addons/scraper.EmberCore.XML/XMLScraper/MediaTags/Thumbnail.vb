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

Namespace XMLScraper
    Namespace MediaTags

        Public Class Thumbnail
            Inherits Utilities.UrlInfo
            Implements IEquatable(Of Thumbnail)

#Region "Fields"

            Private m_Colors As String
            Private m_Dimensions As String
            Private m_Preview As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                MyBase.New()
                Type = UrlTypes.general
                Dimensions = ""
                Colors = ""
                Preview = ""
                Season = -1
                Thumb = ""
                Referrer = ""
            End Sub

            Public Sub New(ByVal element As XElement)
                Me.New()
                Deserialize(element)
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public Property Colors() As String
                Get
                    Return m_Colors
                End Get
                Set(ByVal value As String)
                    m_Colors = value
                End Set
            End Property

            Public Property Dimensions() As String
                Get
                    Return m_Dimensions
                End Get
                Set(ByVal value As String)
                    m_Dimensions = value
                End Set
            End Property

            Public ReadOnly Property IsLocal() As Boolean
                Get
                    If Thumb.Contains("://") OrElse Thumb.Contains(UrlEncode("://")) OrElse Thumb.Contains("/") OrElse Thumb.Contains(UrlEncode("/")) Then
                        Return False
                    End If
                    Return True
                End Get
            End Property

            Public Property Preview() As String
                Get
                    Return m_Preview
                End Get
                Set(ByVal value As String)
                    m_Preview = value
                End Set
            End Property

            Public Property Thumb() As String
                Get
                    Return Url
                End Get
                Set(ByVal value As String)
                    Url = value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Function CompareAndUpdateThumb(ByVal otherThumb As Thumbnail) As Boolean
                Dim changed As Boolean = False
                If Referrer <> otherThumb.Referrer Then
                    Referrer = otherThumb.Referrer
                    changed = True
                End If

                If Me.Preview <> otherThumb.Preview Then
                    Preview = otherThumb.Preview
                    changed = True
                End If

                If Me.Season <> otherThumb.Season Then
                    Me.Season = otherThumb.Season
                    changed = True
                End If

                If Me.Type <> otherThumb.Type Then
                    Me.Type = otherThumb.Type
                    changed = True
                End If

                If Me.Colors <> otherThumb.Colors Then
                    Me.Colors = otherThumb.Colors
                    changed = True
                End If

                If Me.Dimensions <> otherThumb.Dimensions Then
                    Me.Dimensions = otherThumb.Dimensions
                    changed = True
                End If

                Return changed
            End Function

            Public Overloads Overrides Sub Deserialize(ByVal element As XElement)
                If Not String.IsNullOrEmpty(element.Value) Then
                    Thumb = element.Value
                    If Not IsNothing(element.Attribute("type")) Then
                        If element.Attribute("type").Value = "season" Then
                            Type = UrlTypes.season
                        Else
                            Type = UrlTypes.general
                        End If
                    End If
                    If Not IsNothing(element.Attribute("colors")) Then
                        Colors = element.Attribute("colors").Value
                    End If

                    If Not IsNothing(element.Attribute("dim")) Then
                        Dimensions = element.Attribute("dim").Value
                    End If

                    If Not IsNothing(element.Attribute("preview")) Then
                        Preview = element.Attribute("preview").Value
                    End If

                    If Not IsNothing(element.Attribute("season")) Then
                        Try
                            Season = Convert.ToInt32(element.Attribute("season").Value)
                        Catch
                            Season = -1
                        End Try
                    End If

                    If Not IsNothing(element.Attribute("spoof")) Then
                        Referrer = element.Attribute("spoof").Value
                    End If
                End If
            End Sub

            Public Overloads Overrides Function Equals(ByVal obj As Object) As Boolean
                If Not IsNothing(obj) OrElse Me.[GetType]() IsNot obj.[GetType]() Then
                    Return False
                End If
                Return Me.GetHashCode() = obj.GetHashCode()
            End Function

            Public Overloads Overrides Function GetHashCode() As Integer
                Dim hashString As String = Colors & "|" & Dimensions & "|" & Preview & "|" & Season & "|" & Thumb & "|" & Convert.ToString(Type)
                Return hashString.GetHashCode()
            End Function

            Public Overloads Function Serialize() As XElement
                Return Serialize("thumb")
            End Function

            Public Overloads Overrides Function Serialize(ByVal elementname As String) As XElement
                Dim tmp As New XElement(elementname)

                tmp.Value = Thumb

                If Type = UrlTypes.season Then
                    tmp.Add(New XAttribute("type", [Enum].GetName(GetType(UrlTypes), Type)))
                End If

                If Not String.IsNullOrEmpty(Dimensions) Then
                    tmp.Add(New XAttribute("dim", Dimensions))
                End If

                If Not String.IsNullOrEmpty(Colors) Then
                    tmp.Add(New XAttribute("colors", Colors))
                End If

                If Not String.IsNullOrEmpty(Preview) Then
                    tmp.Add(New XAttribute("preview", Preview))
                End If
                If Season > -1 Then
                    tmp.Add(New XAttribute("season", Season))
                End If

                If Not String.IsNullOrEmpty(Referrer) Then
                    tmp.Add(New XAttribute("spoof", Referrer))
                End If
                Return tmp
            End Function

            Public Overloads Overrides Function ToString() As String
                Return Serialize("thumb").ToString()
            End Function

            Private Function IEquatable_Equals(ByVal other As Thumbnail) As Boolean Implements IEquatable(Of Thumbnail).Equals
                Return Me.GetHashCode() = other.GetHashCode()
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
