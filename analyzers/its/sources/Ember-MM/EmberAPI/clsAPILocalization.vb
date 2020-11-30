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

Imports System.IO
Imports System.IO.IsolatedStorage
Imports System.Xml
Imports System.Xml.Linq
Imports System.Xml.Serialization
Imports System.Text

Public Class Localization

#Region "Fields"

    Private Shared htArrayStrings As New List(Of Locs)
    Private Shared htHelpStrings As New Hashtable
    Private Shared htStrings As New Hashtable
    Private Shared _ISOLanguages As New List(Of _ISOLanguage)

    Private _all As String
    Private _disabled As String
    Private _none As String

#End Region 'Fields

#Region "Constructors"

    Public Sub New()
        Me.Clear()
        Dim lPath As String = String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, "Languages.xml")
        If File.Exists(lPath) Then
            Dim LanguageXML As XDocument = XDocument.Load(lPath)
            For Each e In From xGen In LanguageXML...<Language> Select xGen
                Dim c3 As String = If(e.<Alpha3>.Value Is Nothing, String.Empty, e.<Alpha3>.Value.ToString)
                Dim c2 As String = If(e.<Alpha2>.Value Is Nothing, String.Empty, e.<Alpha2>.Value.ToString)
                _ISOLanguages.Add(New _ISOLanguage With {.Alpha2Code = c2, .Alpha3Code = c3, .Language = e.<Name>.Value.ToString})
            Next
        Else
            MsgBox(String.Concat("Cannot find Language.xml.", vbNewLine, vbNewLine, "Expected path:", vbNewLine, lPath), MsgBoxStyle.Critical, "File Not Found")
        End If
    End Sub

#End Region 'Constructors

#Region "Properties"

    Public Property All() As String
        Get
            Return _all
        End Get
        Set(ByVal value As String)
            _all = value
        End Set
    End Property

    Public Property Disabled() As String
        Get
            Return _disabled
        End Get
        Set(ByVal value As String)
            _disabled = value
        End Set
    End Property

    Public Property None() As String
        Get
            Return _none
        End Get
        Set(ByVal value As String)
            _none = value
        End Set
    End Property

#End Region 'Properties

#Region "Methods"
    ' ************************************************************************************************
    ' This are functions for country/Language codes under ISO639 Alpha-2 and Alpha-3(ie: Used by DVD/GoogleAPI)
    Shared Function ISOGetLangByCode2(ByVal code As String) As String
        Return (From x As _ISOLanguage In _ISOLanguages Where (x.Alpha2Code = code))(0).Language
    End Function

    Shared Function ISOGetLangByCode3(ByVal code As String) As String
        Return (From x As _ISOLanguage In _ISOLanguages Where (x.Alpha3Code = code))(0).Language
    End Function

    Public Shared Function ISOLangGetCode2ByLang(ByVal lang As String) As String
        Return (From x As _ISOLanguage In _ISOLanguages Where (x.Language = lang))(0).Alpha2Code
    End Function

    Public Shared Function ISOLangGetCode3ByLang(ByVal lang As String) As String
        Return (From x As _ISOLanguage In _ISOLanguages Where (x.Language = lang))(0).Alpha3Code
    End Function

    Public Shared Function ISOLangGetLanguagesList() As ArrayList
        Dim r As New ArrayList
        For Each x As _ISOLanguage In _ISOLanguages
            r.Add(x.Language)
        Next
        Return r
    End Function

    Public Shared Function ISOLangGetLanguagesListAlpha2() As ArrayList
        Dim r As New ArrayList
        For Each x As _ISOLanguage In _ISOLanguages.Where(Function(y) Not String.IsNullOrEmpty(y.Alpha2Code))
            r.Add(x.Language)
        Next
        Return r
    End Function
    Public Shared Function ISOLangGetLanguagesListAlpha3() As ArrayList
        Dim r As New ArrayList
        For Each x As _ISOLanguage In _ISOLanguages.Where(Function(y) Not String.IsNullOrEmpty(y.Alpha3Code))
            r.Add(x.Language)
        Next
        Return r
    End Function

    Public Sub Clear()
        _all = "All"
        _none = "[none]"
        _disabled = "[Disabled]"
    End Sub

    Public Function GetHelpString(ByVal ctrlName As String) As String
        If htHelpStrings.ContainsKey(ctrlName) Then
            Return htHelpStrings.Item(ctrlName).ToString
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetString(ByVal ID As Integer, ByVal strDefault As String, Optional ByVal forceFromMain As Boolean = False) As String
        Dim Assembly As String = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetCallingAssembly().Location)
        If Assembly = "Ember Media Manager" OrElse Assembly = "EmberAPI" OrElse forceFromMain Then
            Assembly = "*EmberAPP"
        End If
        htStrings = htArrayStrings.FirstOrDefault(Function(x) x.AssenblyName = Assembly).htStrings
        If IsNothing(htStrings) Then
            Return strDefault
        End If
        If htStrings.ContainsKey(ID) Then
            Return htStrings.Item(ID).ToString
        Else
            Return strDefault
        End If
    End Function

	Public Sub LoadAllLanguage(ByVal language As String, Optional ByVal force As Boolean = False)
		If force Then
			htHelpStrings = New Hashtable
			htHelpStrings.Clear()
			htArrayStrings.Clear()
		End If
		For Each s As String In ModulesManager.VersionList.Select(Function(m) m.AssemblyFileName).Distinct
			LoadLanguage(language, s.Replace(".dll", String.Empty))
		Next
	End Sub

    Public Sub LoadHelpStrings(ByVal hPath As String)
        Try
            If File.Exists(hPath) Then
                Dim tReader As New StreamReader(hPath, Encoding.UTF8, False)
                Dim LangXML As XDocument = XDocument.Load(tReader)
                Dim xLanguage = From xLang In LangXML...<strings>...<string> Select xLang.@control, xLang.Value
                If xLanguage.Count > 0 Then
                    For i As Integer = 0 To xLanguage.Count - 1
                        htHelpStrings.Add(xLanguage(i).control, xLanguage(i).Value)
                    Next
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub LoadLanguage(ByVal Language As String, Optional ByVal rAssembly As String = "", Optional ByVal force As Boolean = False)
        Dim _old_all As String = _all
        Dim Assembly As String
        Dim lPath As String = String.Empty
        Dim lhPath As String = String.Empty
        Me.Clear()
        Try
            If Not String.IsNullOrEmpty(Language) Then
                If rAssembly = "" Then
                    Assembly = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetCallingAssembly().Location)
                Else
                    Assembly = rAssembly
                End If


                If Assembly = "Ember Media Manager" OrElse Assembly = "*EmberAPI" OrElse Assembly = "*EmberAPP" Then
                    Assembly = "*EmberAPP"
                    lPath = String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, Language, ".xml")
                    lhPath = String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, Language, "-Help.xml")
                Else
                    lPath = String.Concat(Functions.AppPath, "Modules", Path.DirectorySeparatorChar, "Langs", Path.DirectorySeparatorChar, Assembly, ".", Language, ".xml")
                    lhPath = String.Concat(Functions.AppPath, "Modules", Path.DirectorySeparatorChar, "Langs", Path.DirectorySeparatorChar, Assembly, ".", Language, "-Help.xml")
                    If Not File.Exists(lPath) Then 'Failback disabled, possible not need anymore
                        'lPath = String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, Language, ".xml")
                        File.WriteAllText(lPath, "<?xml version=""1.0"" encoding=""utf-8""?>" & vbCrLf & _
                            "<strings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">" & vbCrLf & _
                            "</strings>")
                    End If
                End If
                If Not force AndAlso Not htArrayStrings.FirstOrDefault(Function(h) h.AssenblyName = Assembly).AssenblyName Is Nothing Then Return

                LoadHelpStrings(lhPath)
                htStrings = New Hashtable
                htStrings.Clear()
                If File.Exists(lPath) Then
                    Dim tReader As New StreamReader(lPath, Encoding.UTF8)
                    Dim LangXML As XDocument = XDocument.Load(tReader)
                    Dim xLanguage = From xLang In LangXML...<strings>...<string> Select xLang.@id, xLang.Value
                    If xLanguage.Count > 0 Then
                        For i As Integer = 0 To xLanguage.Count - 1
                            htStrings.Add(Convert.ToInt32(xLanguage(i).id), xLanguage(i).Value)
                        Next
                        htArrayStrings.Remove(htArrayStrings.FirstOrDefault(Function(h) h.AssenblyName = Assembly))
                        htArrayStrings.Add(New Locs With {.AssenblyName = Assembly, .htStrings = htStrings, .FileName = lPath})
                        _all = String.Format("[{0}]", GetString(569, Master.eLang.All))
                        _none = GetString(570, Master.eLang.None)
                        _disabled = GetString(571, Master.eLang.Disabled)
                    Else
                        Dim tLocs As Locs = htArrayStrings.FirstOrDefault(Function(h) h.AssenblyName = Assembly)
                        If Not IsNothing(tLocs) Then
                            tLocs.htStrings = htStrings
                        Else
                            htArrayStrings.Add(New Locs With {.AssenblyName = Assembly, .htStrings = htStrings, .FileName = lPath})
                        End If
                    End If
                Else
                    MsgBox(String.Concat(String.Format("Cannot find {0}.xml.", Language), vbNewLine, vbNewLine, "Expected path:", vbNewLine, lPath), MsgBoxStyle.Critical, "File Not Found")
                End If
            End If

            ' Need to change Globaly Langs_all
            Master.eSettings.GenreFilter = Master.eSettings.GenreFilter.Replace(_old_all, _all)

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub


#End Region 'Methods

#Region "Nested Types"

    ' ************************************************************************************************
    Structure Locs

#Region "Fields"

        Dim AssenblyName As String
        Dim FileName As String
        Dim htStrings As Hashtable

#End Region 'Fields

    End Structure

    Structure _ISOLanguage

#Region "Fields"

        Public Alpha2Code As String
        Public Alpha3Code As String
        Public Language As String

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class