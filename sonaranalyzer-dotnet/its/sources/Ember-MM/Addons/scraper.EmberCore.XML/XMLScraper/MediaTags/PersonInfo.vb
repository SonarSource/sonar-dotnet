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
Imports System.Text
Imports System.Xml.Linq

Imports EmberScraperModule.XMLScraper.ScraperLib
Imports EmberScraperModule.XMLScraper.Utilities

Namespace XMLScraper
    Namespace MediaTags

        Public Class PersonTag
            Inherits MediaTag
            Implements IComparable(Of PersonTag)

#Region "Fields"

            Private m_YearsActive As String

#End Region 'Fields

#Region "Constructors"

            Public Sub New()
                Clear()
            End Sub

            Public Sub New(ByVal personName As String, ByVal personThumb As String, ByVal personRole As String, ByVal personBio As String)
                Me.New()
                Name = personName
                Thumb = New Thumbnail()
                Thumb.Thumb = personThumb
                Biography = personBio
            End Sub

            Public Sub New(ByVal element As XElement)
                Me.New()
                Deserialize(element)
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
                    MyBase.UserProperties("biography") = Value
                End Set
            End Property

            Public Property Born() As String
                Get
                    Return MyBase.UserProperties("born")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("born") = Value
                End Set
            End Property

            Public Property Died() As String
                Get
                    Return MyBase.UserProperties("died")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("died") = Value
                End Set
            End Property

            Public Property Fanart() As Fanart
                Get
                    Return MyBase._Fanart
                End Get
                Set(ByVal value As Fanart)
                    MyBase._Fanart = Value
                End Set
            End Property

            Public Property Name() As String
                Get
                    Return MyBase.UserProperties("title")
                End Get
                Set(ByVal value As String)
                    MyBase.UserProperties("title") = Value
                End Set
            End Property

            Public Property Role() As String
                Get
                    Return UserProperties("role")
                End Get
                Set(ByVal value As String)
                    UserProperties("role") = Value
                End Set
            End Property

            Public Overloads Overrides ReadOnly Property TagType() As MediaType
                Get
                    Return MediaType.person
                End Get
            End Property

            Public Property Thumb() As Thumbnail
                Get
                    If Thumbs.Count > 0 Then
                        Return Thumbs(0)
                    Else
                        Return Nothing
                    End If
                End Get
                Set(ByVal value As Thumbnail)
                    Dim tmpThumb As Thumbnail = Thumbs.Find(Function(n) n.Thumb = Value.Thumb)
                    If IsNothing(tmpThumb) Then
                        Thumbs.Insert(0, Value)
                    Else

                        tmpThumb.CompareAndUpdateThumb(Value)

                        Thumbs.Remove(tmpThumb)
                        Thumbs.Insert(0, tmpThumb)
                    End If
                End Set
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
                    Return m_YearsActive
                End Get
                Set(ByVal value As String)
                    m_YearsActive = Value
                End Set
            End Property

#End Region 'Properties

#Region "Methods"

            Public Overloads Overrides Function BlankSerialize(ByVal elementName As String) As XElement
                Throw New NotImplementedException()
            End Function

            Public Overloads Overrides Sub Clear()
                Name = ""
                Aliases = New List(Of String)()
                Role = ""
                Biography = ""
                Born = ""
                Died = ""
                Thumbs = New List(Of Thumbnail)()
                Fanart = New Fanart()
            End Sub

            Public Function CompareAndUpdate(ByVal otherPerson As PersonTag) As Boolean
                Dim sTemp As String = String.Empty
                Dim changed As Boolean = False
                If [String].Compare(Name, otherPerson.Name, StringComparison.OrdinalIgnoreCase) = 0 Then
                    If Not String.IsNullOrEmpty(otherPerson.Role) Then
                        Role = otherPerson.Role
                        changed = True
                    End If

                    If [String].IsNullOrEmpty(Biography) AndAlso Not [String].IsNullOrEmpty(otherPerson.Biography) Then
                        Biography = otherPerson.Biography
                        changed = True
                    End If

                    For Each item As Thumbnail In otherPerson.Thumbs
                        sTemp = item.Thumb
                        Dim tmp As Thumbnail = Thumbs.Find(Function(n) n.Thumb = sTemp)

                        If Not IsNothing(tmp) Then
                            tmp.CompareAndUpdateThumb(item)
                        End If
                    Next
                End If
                Return changed
            End Function

            Public Function CompareTo(ByVal other As PersonTag) As Integer Implements IComparable(Of PersonTag).CompareTo
                If IsNothing(Me) Then
                    If IsNothing(other) Then
                        Return 0
                    Else
                        Return -1
                    End If
                Else
                    If IsNothing(other) Then
                        Return -1
                    Else
                        Return Me.Name.CompareTo(other.Name)
                    End If
                End If
            End Function

            Public Overloads Sub Deserialize(ByVal xInfo As XDocument)
                Deserialize(xInfo.Root)
            End Sub

            Public Overloads Overrides Sub Deserialize(ByVal element As XElement)
                Name = element.GetStringElement("name", Name)
                element.UpdateStringList("alias", Aliases)
                Role = element.GetStringElement("role", Role)
                Born = element.GetStringElement("born", Born)
                Died = element.GetStringElement("died", Died)
                Biography = element.GetStringElement("biography", Biography)
                element.UpdateThumbList("thumb", Thumbs)
            End Sub

            Public Overloads Overrides Sub Deserialize(ByVal xmlFilePath As String)
                Dim tmpDocument As XDocument = XDocument.Load(xmlFilePath)
                Deserialize(tmpDocument)
            End Sub

            Public Overloads Overrides Function Equals(ByVal obj As Object) As Boolean
                If Not IsNothing(obj) OrElse Me.[GetType]() IsNot obj.[GetType]() Then
                    Return False
                End If
                Return Me.GetHashCode() = obj.GetHashCode()
            End Function

            Public Overloads Function Equals(ByVal other As PersonTag) As Boolean
                Return Me.GetHashCode() = other.GetHashCode()
            End Function

            Public Overloads Overrides Function GetHashCode() As Integer
                Dim hashString As String = Name.ToLowerInvariant()
                Return hashString.GetHashCode()
            End Function

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
                tmp.AddStringElement("biography", Biography)
                tmp.AddThumbList("thumb", Thumbs)
                tmp.AddStringElement("role", Role)

                Return tmp
            End Function

#End Region 'Methods

        End Class

        Public Module PersonSerializeHelpers

#Region "Methods"

            <System.Runtime.CompilerServices.Extension()> _
            Friend Sub AddPersonList(ByVal element As XElement, ByVal name As String, ByVal list As List(Of PersonTag))
                For Each person As PersonTag In list
                    element.Add(person.Serialize(name))
                Next
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Friend Sub UpdatePersonList(ByVal element As XElement, ByVal name As String, ByVal personList As List(Of PersonTag))
                For Each item As XElement In element.Elements(name)
                    Dim newPerson As New PersonTag(item)

                    Dim found As PersonTag = personList.Find(Function(n) n Is newPerson)

                    If IsNothing(found) Then
                        personList.Add(newPerson)
                    Else
                        found.CompareAndUpdate(newPerson)

                    End If
                Next
            End Sub

#End Region 'Methods

        End Module

    End Namespace
End Namespace
