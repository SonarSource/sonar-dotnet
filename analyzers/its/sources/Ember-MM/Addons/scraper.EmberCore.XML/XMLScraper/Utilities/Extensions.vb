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

Imports EmberScraperModule.XMLScraper.MediaTags

Namespace XMLScraper
    Namespace Utilities

        Public Module Extensions

#Region "Methods"

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddBooleanAttribute(ByVal element As XElement, ByVal name As String, ByVal value As Boolean, ByVal ignorevalue As Boolean)
                If value <> ignorevalue Then
                    element.Add(New XAttribute(name, value.ToString.ToLower))
                End If
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddBooleanAttribute(ByVal element As XElement, ByVal name As String, ByVal value As String, ByVal ignorevalue As Boolean)
                Try
                    Dim tmp As Boolean = Convert.ToBoolean(value)
                    If tmp <> ignorevalue Then
                        element.Add(New XAttribute(name, value.ToString.ToLower))
                    End If

                Catch
                End Try
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddBooleanAttribute(ByVal element As XElement, ByVal name As String, ByVal value As Boolean)
                element.Add(New XAttribute(name, value.ToString.ToLower))
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddBooleanAttribute(ByVal element As XElement, ByVal name As String, ByVal value As String)
                Try
                    Dim tmp As Boolean = Convert.ToBoolean(value)
                    element.Add(New XAttribute(name, tmp.ToString.ToLower))
                Catch
                    element.Add(New XAttribute(name, "false"))
                End Try
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddBooleanElement(ByVal element As XElement, ByVal name As String, ByVal value As Boolean, ByVal ignorevalue As Boolean)
                If value <> ignorevalue Then
                    element.Add(New XElement(name, value.ToString.ToLower))
                End If
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddBooleanElement(ByVal element As XElement, ByVal name As String, ByVal value As String, ByVal ignorevalue As Boolean, ByVal defaultvalue As Boolean)
                Try
                    Dim tmp As Boolean = Convert.ToBoolean(value.ToLower)
                    If Not tmp = ignorevalue Then
                        element.Add(New XElement(name, value.ToString.ToLower))
                    End If
                Catch
                    If defaultvalue Then
                        If ignorevalue Then
                            element.Add(New XElement(name, "false"))
                        End If
                    Else
                        If Not ignorevalue Then
                            element.Add(New XElement(name, "true"))
                        End If
                    End If
                End Try
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddBooleanElement(ByVal element As XElement, ByVal name As String, ByVal value As Boolean)
                element.Add(New XElement(name, value.ToString.ToLower))
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddBooleanElement(ByVal element As XElement, ByVal name As String, ByVal value As String, ByVal defaultvalue As Boolean)
                Try
                    Dim tmp As Boolean = Convert.ToBoolean(value)
                    element.Add(New XElement(name, tmp.ToString.ToLower))
                Catch
                    element.Add(New XElement(name, defaultvalue.ToString))
                End Try
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddDoubleAttribute(ByVal element As XElement, ByVal name As String, ByVal value As Double, ByVal leastvalue As Double)
                If value >= leastvalue Then
                    element.Add(New XAttribute(name, value))
                End If
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddDoubleElement(ByVal element As XElement, ByVal name As String, ByVal value As Double, ByVal leastvalue As Double)
                If value >= leastvalue Then
                    element.Add(New XElement(name, value))
                End If
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddIntAttribute(ByVal element As XElement, ByVal name As String, ByVal value As Integer, ByVal leastvalue As Integer)
                If value >= leastvalue Then
                    element.Add(New XAttribute(name, value))
                End If
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddIntElement(ByVal element As XElement, ByVal name As String, ByVal value As Integer, ByVal leastvalue As Integer)
                If value >= leastvalue Then
                    element.Add(New XElement(name, value))
                End If
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddPersonList(ByVal element As XElement, ByVal name As String, ByVal list As List(Of PersonTag))
                For Each person As PersonTag In list
                    element.Add(person.Serialize("name"))
                Next
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddStringAttribute(ByVal element As XElement, ByVal name As String, ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    element.Add(New XAttribute(name, value))
                End If
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddStringElement(ByVal element As XElement, ByVal name As String, ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    element.Add(New XElement(name, value))
                End If
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddStringList(ByVal element As XElement, ByVal name As String, ByVal list As List(Of String))
                For Each item As String In list
                    element.Add(New XElement(name, item))
                Next
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub AddThumbList(ByVal element As XElement, ByVal name As String, ByVal thumbs As List(Of Thumbnail))
                For Each item As Thumbnail In thumbs
                    element.Add(New XElement(item.Serialize(name)))
                Next
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub Clean(ByVal stringlist As List(Of String), ByVal Delimiter As String)
                Dim tmp As New List(Of String)
                For Each item As String In stringlist
                    For Each strItem As String In item.Split(New String() {Delimiter}, StringSplitOptions.RemoveEmptyEntries)
                        tmp.Add(strItem.Trim)
                    Next
                Next
                stringlist.Clear()
                stringlist.AddRange(tmp)
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Function GetBooleanAttribute(ByVal element As XElement, ByVal name As String, ByVal defaultValue As Boolean) As Boolean
                Dim stringAttribute As XAttribute = element.Attribute(name)
                If Not IsNothing(stringAttribute) Then
                    Try
                        Return Convert.ToBoolean(stringAttribute.Value.Trim.ToLower)
                    Catch
                        Return defaultValue
                    End Try
                End If
                Return defaultValue
            End Function

            <System.Runtime.CompilerServices.Extension()> _
            Public Function GetBooleanElement(ByVal element As XElement, ByVal name As String, ByVal defaultValue As Boolean) As Boolean
                Dim boolElement As XElement = element.Element(name)
                If Not IsNothing(boolElement) Then
                    Try
                        Return Convert.ToBoolean(boolElement.Value.Trim.ToLower)
                    Catch
                        Return defaultValue
                    End Try
                End If
                Return defaultValue
            End Function

            <System.Runtime.CompilerServices.Extension()> _
            Public Function GetDoubleAttribute(ByVal element As XElement, ByVal name As String, ByVal defaultValue As Double) As Double
                Dim doubleAttribute As XAttribute = element.Attribute(name)
                If Not IsNothing(doubleAttribute) Then

                    Try
                        Return Convert.ToDouble(doubleAttribute.Value.Trim)
                    Catch
                        Return defaultValue

                    End Try
                End If
                Return defaultValue
            End Function

            <System.Runtime.CompilerServices.Extension()> _
            Public Function GetDoubleElement(ByVal element As XElement, ByVal name As String, ByVal defaultValue As Double) As Double
                Dim doubleElement As XElement = element.Element(name)
                If Not IsNothing(doubleElement) Then
                    If Not String.IsNullOrEmpty(doubleElement.Value) Then
                        Try
                            Return Convert.ToDouble(doubleElement.Value.Trim)
                        Catch
                            Return defaultValue
                        End Try
                    End If
                End If
                Return defaultValue
            End Function

            <System.Runtime.CompilerServices.Extension()> _
            Public Function GetIntAttribute(ByVal element As XElement, ByVal name As String, ByVal defaultValue As Integer) As Integer
                Dim intAttribute As XAttribute = element.Attribute(name)
                If Not IsNothing(intAttribute) Then
                    Try
                        Return Convert.ToInt32(intAttribute.Value.Trim)
                    Catch
                        Return defaultValue
                    End Try
                End If
                Return defaultValue
            End Function

            <System.Runtime.CompilerServices.Extension()> _
            Public Function GetIntElement(ByVal element As XElement, ByVal name As String, ByVal defaultValue As Integer) As Integer
                Dim intElement As XElement = element.Element(name)
                If Not IsNothing(intElement) Then
                    Try
                        Return Convert.ToInt32(intElement.Value.Replace(",", String.Empty).Trim)
                    Catch
                        Return defaultValue
                    End Try
                End If
                Return defaultValue
            End Function

            <System.Runtime.CompilerServices.Extension()> _
            Public Function GetStringAttribute(ByVal element As XElement, ByVal name As String, ByVal defaultValue As String) As String
                Dim attribute As XAttribute = element.Attribute(name)
                If Not IsNothing(attribute) Then
                    If Not String.IsNullOrEmpty(attribute.Value) Then
                        Return attribute.Value.Trim
                    End If
                End If
                Return defaultValue
            End Function

            <System.Runtime.CompilerServices.Extension()> _
            Public Function GetStringElement(ByVal element As XElement, ByVal name As String, ByVal defaultValue As String) As String
                Dim stringElement As XElement = element.Element(name)
                If Not IsNothing(stringElement) Then
                    If Not String.IsNullOrEmpty(stringElement.Value) Then
                        Return stringElement.Value.Trim
                    End If
                End If
                Return defaultValue
            End Function

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub UpdatePersonList(ByVal element As XElement, ByVal name As String, ByVal personList As List(Of PersonTag))
                For Each item As XElement In element.Elements(name)
                    If Not String.IsNullOrEmpty(item.Value) Then
                        Dim newPerson As New PersonTag(item)
                        If Not personList.Contains(newPerson) Then
                            personList.Add(newPerson)
                        Else
                            Dim found As PersonTag = personList.Find(Function(n) n Is newPerson)
                            found.CompareAndUpdate(newPerson)
                        End If
                    End If
                Next
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub UpdateStringList(ByVal element As XElement, ByVal name As String, ByVal list As List(Of String))
                For Each item As XElement In element.Elements(name)
                    If Not String.IsNullOrEmpty(item.Value) Then
                        Dim newItem As String = item.Value.Trim
                        If Not list.Contains(newItem) Then
                            list.Add(newItem)
                        End If
                    End If
                Next
            End Sub

            <System.Runtime.CompilerServices.Extension()> _
            Public Sub UpdateThumbList(ByVal element As XElement, ByVal name As String, ByVal list As List(Of Thumbnail))
                For Each thumbItem As XElement In element.Elements(name)
                    Dim newthumb As New Thumbnail(thumbItem)
                    If Not list.Contains(newthumb) Then
                        list.Add(newthumb)
                    End If
                Next
            End Sub

#End Region 'Methods

        End Module

    End Namespace
End Namespace
