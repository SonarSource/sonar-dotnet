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

        Public MustInherit Class ScraperFunctionContainer

#Region "Fields"

            Private m_Name As String
            Private m_ScraperFunctions As List(Of ScraperFunction)

#End Region 'Fields

#Region "Constructors"

            Protected Friend Sub New()
                ScraperFunctions = New List(Of ScraperFunction)()
            End Sub

#End Region 'Constructors

#Region "Properties"

            Public ReadOnly Property FirstFunction() As ScraperFunction
                Get
                    If ScraperFunctions.Count > 0 Then
                        Return ScraperFunctions(0)
                    Else
                        Return Nothing
                    End If
                End Get
            End Property

            Public ReadOnly Property LastFunction() As ScraperFunction
                Get
                    If ScraperFunctions.Count > 0 Then
                        Return ScraperFunctions(ScraperFunctions.Count - 1)
                    Else
                        Return Nothing
                    End If
                End Get
            End Property

            Public Property Name() As String
                Get
                    Return m_Name
                End Get
                Set(ByVal value As String)
                    m_Name = value
                End Set
            End Property

            Public Overridable ReadOnly Property NeededSettings() As List(Of SettingsUsed)
                Get
                    Return GetNeededSettings()
                End Get
            End Property

            Public Property ScraperFunctions() As List(Of ScraperFunction)
                Get
                    Return m_ScraperFunctions
                End Get
                Set(ByVal value As List(Of ScraperFunction))
                    m_ScraperFunctions = value
                End Set
            End Property

            Public ReadOnly Property XML() As String
                Get
                    Return CreateXML()
                End Get
            End Property

#End Region 'Properties

#Region "Methods"

            Public Sub AddFunction(ByVal FunctionName As String, ByVal FunctionDest As Integer)
                ScraperFunctions.Add(New ScraperFunction(Me))
                ScraperFunctions(ScraperFunctions.Count - 1).Name = FunctionName
                ScraperFunctions(ScraperFunctions.Count - 1).Dest = FunctionDest
            End Sub

            Public Sub AddFunction()
                ScraperFunctions.Add(New ScraperFunction(Me))
            End Sub

            Public Function CheckForDulicateFunction(ByVal FunctionName As String) As List(Of Integer)
                Dim DuplicatesIndex As New List(Of Integer)()
                For i As Integer = 0 To ScraperFunctions.Count - 1
                    If ScraperFunctions(i).Name = FunctionName Then
                        ScraperFunctions.RemoveAt(i)
                    End If
                Next
                Return DuplicatesIndex
            End Function

            Public Function ContainsFunction(ByVal FunctionName As String) As Boolean
                Dim Exists As Boolean = False
                For i As Integer = 0 To ScraperFunctions.Count - 1
                    If ScraperFunctions(i).Name = FunctionName Then
                        Exists = True
                    End If

                    If Exists Then
                        Exit For
                    End If
                Next
                Return Exists
            End Function

            Public Overridable Sub Deserialize(ByVal xContainerInfo As XDocument)
            End Sub

            Public Overridable Function Evaluate() As List(Of ScraperEvaluation)
                Dim tmp As New List(Of ScraperEvaluation)()
                Dim pathName As String = Me.[GetType]().Name

                For Each item As ScraperFunction In ScraperFunctions
                    tmp.AddRange(item.GetEvaluation(pathName))
                Next

                tmp.Sort()
                Return tmp
            End Function

            Public Overridable Function GetNeededSettings() As List(Of SettingsUsed)
                Dim sTemp As String = String.Empty
                Dim tmpMissing As New List(Of SettingsUsed)()
                For Each funcItem As ScraperFunction In ScraperFunctions
                    Dim MissingTemp As List(Of SettingsUsed) = funcItem.CollectSettings()

                    For Each msItem As SettingsUsed In MissingTemp
                        sTemp = msItem.ID

                        Dim msTemp As SettingsUsed = tmpMissing.Find(Function(n) n.ID = sTemp)

                        If Not IsNothing(msTemp) Then

                            If msTemp.Type <> msItem.Type Then
                                msTemp.Type = MissingSettingType.[boolean]
                            End If

                            If Not msTemp.ImplementingFunctions.Contains(funcItem.Name) Then
                                msTemp.ImplementingFunctions.Add(funcItem.Name)
                            End If
                        Else
                            tmpMissing.Add(msItem)
                        End If
                    Next
                Next
                Return tmpMissing
            End Function

            Public Function HasFunction(ByVal FunctionName As String) As Boolean
                For i As Integer = 0 To ScraperFunctions.Count - 1
                    If ScraperFunctions(i).Name = FunctionName Then
                        Return True
                    End If
                Next
                Return False
            End Function

            Public Sub InsertFunction(ByVal Index As Integer, ByVal newFunction As ScraperFunction)
                If ContainsFunction(newFunction.Name) Then
                    RemoveFunction(newFunction.Name)
                End If
                ScraperFunctions.Insert(Index, newFunction)
            End Sub

            Public Sub RemoveFunction(ByVal Index As Integer)
                ScraperFunctions.RemoveAt(Index)
            End Sub

            Public Sub RemoveFunction(ByVal FunctionName As String)
                For i As Integer = 0 To ScraperFunctions.Count - 1
                    If ScraperFunctions(i).Name = FunctionName Then
                        ScraperFunctions.RemoveAt(i)
                    End If
                Next
            End Sub

            Protected Friend Overridable Function CreateXML() As String
                Dim strReturn As String = ""
                For Each funcItem As ScraperFunction In ScraperFunctions
                    strReturn += funcItem.XML
                Next

                Return strReturn
            End Function

            Protected Overridable Function Serialize(ByVal Encoding As String) As XDocument
                Dim tmp As New XDocument()

                tmp.Declaration = New XDeclaration("1.0", Encoding, "")

                Return tmp
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
