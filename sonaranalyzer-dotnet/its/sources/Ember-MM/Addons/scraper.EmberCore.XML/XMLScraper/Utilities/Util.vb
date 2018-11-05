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
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

Imports ICSharpCode.SharpZipLib.Zip

Imports EmberScraperModule.XMLScraper.ScraperLib
Imports EmberScraperModule.XMLScraper.ScraperXML

Namespace XMLScraper
    Namespace Utilities

        Public NotInheritable Class FileIO

#Region "Methods"

            Public Shared Function LoadStringFromFile(ByVal filePath As String) As String
                Dim strReturn As String = String.Empty
                If File.Exists(filePath) Then
                    Using temp As New FileStream(filePath, FileMode.Open)
                        Using sr As New StreamReader(temp)
                            strReturn = sr.ReadToEnd()
                        End Using
                    End Using
                End If
                Return strReturn
            End Function

            Public Shared Function VerifyCacheFolder(ByVal cachePath As String) As Boolean
                If Not String.IsNullOrEmpty(cachePath) Then
                    Dim CacheFolder As New DirectoryInfo(cachePath)
                    If Not CacheFolder.Exists Then
                        CacheFolder.Create()
                    End If
                    Return True
                Else
                    Return False
                End If
            End Function

#End Region 'Methods

        End Class

        Public Class HttpRetrieve

#Region "Methods"

            Public Shared Function GetPage(ByVal url As UrlInfo, ByVal cacheFolder As String) As String
                Dim strReturn As String = String.Empty

                If Not String.IsNullOrEmpty(url.Cache) Then
                    If Not String.IsNullOrEmpty(cacheFolder) Then
                        Dim filename As String = cacheFolder & url.Cache
                        If File.Exists(filename) Then
                            Using temp As New FileStream(filename, FileMode.Open)
                                Using sr As New StreamReader(temp)
                                    strReturn = sr.ReadToEnd()
                                End Using
                            End Using
                            Return strReturn
                        End If
                    End If
                End If

                Dim objRequest As HttpWebRequest

                objRequest = DirectCast(WebRequest.Create(url.Url), HttpWebRequest)
                objRequest.UseDefaultCredentials = True
                objRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2)"
                If Not String.IsNullOrEmpty(url.Referrer) Then
                    objRequest.Referer = url.Referrer
                End If

                If url.Post Then
                    Dim encoding__1 As New UTF8Encoding()
                    Dim postBytes As [Byte]() = UTF8Encoding.UTF8.GetBytes(url.PostData)
                    objRequest.ContentLength = postBytes.Length
                    objRequest.Method = "POST"
                    objRequest.ContentType = "application/octet-stream"
                    Using postStream As Stream = DirectCast(objRequest.GetRequestStream(), Stream)
                        postStream.Write(postBytes, 0, postBytes.Length)
                    End Using
                End If

                Using objResponse As HttpWebResponse = DirectCast(objRequest.GetResponse(), HttpWebResponse)
                    If objResponse.ContentType = "application/zip" Then
                        Using Wc As New WebClient()
                            Using mStream As New MemoryStream(Wc.DownloadData(url.Url))
                                Dim zip As New ZipFile(mStream)

                                For Each zEntry As ZipEntry In zip
                                    Using sr As New StreamReader(zip.GetInputStream(zEntry))
                                        strReturn += sr.ReadToEnd()
                                    End Using
                                Next

                            End Using
                        End Using
                    Else
                        Dim pageencdoding As Encoding = Nothing
                        If Not String.IsNullOrEmpty(objResponse.ContentEncoding) Then
                            pageencdoding = Encoding.GetEncoding(objResponse.ContentEncoding)
                        ElseIf Not String.IsNullOrEmpty(objResponse.CharacterSet) Then
                            pageencdoding = Encoding.GetEncoding(objResponse.CharacterSet)
                        End If
                        If Not pageencdoding Is Nothing Then
                            Using sr As StreamReader = New System.IO.StreamReader(objResponse.GetResponseStream(), pageencdoding)
                                strReturn = sr.ReadToEnd()
                            End Using
                        Else
                            Using sr As StreamReader = New System.IO.StreamReader(objResponse.GetResponseStream())
                                strReturn = sr.ReadToEnd()
                            End Using
                        End If
                    End If
                End Using

                If Not String.IsNullOrEmpty(cacheFolder) Then
                    If Not String.IsNullOrEmpty(url.Cache) Then
                        Using temp As New FileStream(cacheFolder & url.Cache, FileMode.Create)
                            Using sw As New StreamWriter(temp)
                                sw.Write(strReturn)
                            End Using
                        End Using
                    End If
                End If
                Return strReturn
            End Function

#End Region 'Methods

        End Class

        Public NotInheritable Class XmlUtilities

#Region "Methods"

            Public Shared Function AddRootElementAndParse(ByVal xml As String, ByVal tag As String) As XElement
                Dim tString As New StringBuilder
                Dim ElementSnatcher As New Regex("<([^\s]+)(.*?)>([^<]+)</\1>")

                tString.Append(String.Concat("<", tag, ">"))

                For i As Integer = 0 To ElementSnatcher.Matches(xml).Count - 1
                    tString.Append("<")
                    tString.Append(ElementSnatcher.Matches(xml)(i).Groups(1).Value)
                    tString.Append(ElementSnatcher.Matches(xml)(i).Groups(2).Value)
                    tString.Append(">")
                    tString.Append(ElementSnatcher.Matches(xml)(i).Groups(3).Value)
                    tString.Append("</")
                    tString.Append(ElementSnatcher.Matches(xml)(i).Groups(1).Value)
                    tString.Append(">")
                Next

                tString.Append(String.Concat("</", tag, ">"))
                Return TryParse(tString.ToString)
            End Function

            Public Shared Function FixParse(ByVal value As String) As String
                value = value.Replace("&", "&amp;")
                value = value.Replace("&amp;amp;", "&amp;")
                value = value.Replace("&amp;amp;amp;", "&amp;&amp;")
                value = value.Replace("&amp;#", "&#")
                value = value.Replace("&amp;quot;", "&quot;")
                value = value.Replace("&amp;lt;", "&lt;")
                value = value.Replace("&amp;gt;", "&gt;")
                value = value.Replace("&amp;apos;", "&apos;")

                Return value
            End Function

            Public Shared Function HasChildren(ByVal xmlElement As XContainer) As Boolean
                If xmlElement.Elements.Count > 0 Then
                    Return True
                Else
                    Return False
                End If
            End Function

            Public Shared Function HasElements(ByVal xml As String) As Boolean
                Dim ElementSnatcher As New Regex("<([^\s]+)(.*?)>([^<]+)</\1>")

                If ElementSnatcher.Matches(xml).Count > 0 Then
                    Return True
                Else
                    Return False
                End If
            End Function

            Public Shared Function HasUTF8Declaration(ByVal xml As String) As Boolean
                Dim test As String = xml
                test = test.ToLower()
                Return test.Contains("encoding=""utf-8""")
            End Function

            Public Shared Function TryFixParse(ByVal xml As String) As XElement
                Try
                    If Not [String].IsNullOrEmpty(xml) Then
                        Return XElement.Parse(FixParse(xml))
                    End If
                Catch
                End Try
                Return Nothing
            End Function

            Public Shared Function TryLoadDocument(ByVal filePath As String) As XDocument
                Try
                    Return XDocument.Load(filePath)
                Catch
                End Try
                Return Nothing
            End Function

            Public Shared Function TryParse(ByVal xml As String) As XElement
                Try
                    If Not [String].IsNullOrEmpty(xml) Then
                        Return XElement.Parse(xml)
                    End If
                Catch
                End Try
                Return Nothing
            End Function

            Friend Shared Function IsCommonFile(ByVal doc As XDocument) As Boolean
                If IsNothing(doc) OrElse Not doc.Root.Name.ToString = "scraperfunctions" Then
                    Return False
                End If
                Return True
            End Function

            Friend Shared Function IsScraper(ByVal doc As XDocument) As Boolean
                If IsNothing(doc) OrElse Not doc.Root.Name.ToString = "scraper" Then
                    Return False
                End If
                Return True
            End Function

            Friend Shared Function ReplaceEntities(ByVal toChange As String) As String
                toChange = toChange.Replace("&", "&amp;")
                toChange = toChange.Replace("<", "&lt;")
                toChange = toChange.Replace(">", "&gt;")
                toChange = toChange.Replace("'", "&apos;")
                toChange = toChange.Replace("""", "&quot;")
                toChange = toChange.Replace(vbLf, "\n")
                Return toChange
            End Function

#End Region 'Methods

        End Class

    End Namespace
End Namespace
