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
Imports System.IO.Compression
Imports System.Text
Imports System.Net
Imports System.Drawing

Public Class HTTP

#Region "Fields"

    Private dThread As New Threading.Thread(AddressOf DownloadImage)
    Private wrRequest As HttpWebRequest
    Private _cancel As Boolean
    Private _image As Image
    Private _responseuri As String
    Private _URL As String = String.Empty
#End Region 'Fields

#Region "Constructors"

    Public Sub New()
        Me.Clear()
    End Sub

    Protected Overrides Sub Finalize()
        Me.wrRequest = Nothing
        MyBase.Finalize()
    End Sub

#End Region 'Constructors

#Region "Events"

    Public Event ProgressUpdated(ByVal iPercent As Integer)

#End Region 'Events

#Region "Properties"
    Public ReadOnly Property Image() As Image
        Get
            Return Me._image
        End Get
    End Property

    Public Property ResponseUri() As String
        Get
            Return Me._responseuri
        End Get
        Set(ByVal value As String)
            Me._responseuri = value
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Public Sub Cancel()
        Me._cancel = True
        If Not IsNothing(Me.wrRequest) Then Me.wrRequest.Abort()
    End Sub

    Public Sub Clear()
        Me._responseuri = String.Empty
        Me._image = Nothing
        Me._cancel = False
    End Sub
    Public Function DownloadData(ByVal URL As String) As String
        Dim sResponse As String = String.Empty
        Dim cEncoding As System.Text.Encoding

        Me.Clear()

        Try
            Me.wrRequest = DirectCast(WebRequest.Create(URL), HttpWebRequest)
            Me.wrRequest.Timeout = 20000
            Me.wrRequest.Headers.Add("Accept-Encoding", "gzip,deflate")
            Me.wrRequest.KeepAlive = False

            If Not String.IsNullOrEmpty(Master.eSettings.ProxyURI) AndAlso Master.eSettings.ProxyPort >= 0 Then
                Dim wProxy As New WebProxy(Master.eSettings.ProxyURI, Master.eSettings.ProxyPort)
                wProxy.BypassProxyOnLocal = True
                If Not String.IsNullOrEmpty(Master.eSettings.ProxyCreds.UserName) Then
                    wProxy.Credentials = Master.eSettings.ProxyCreds
                Else
                    wProxy.Credentials = CredentialCache.DefaultCredentials
                End If
                Me.wrRequest.Proxy = wProxy
            End If
            Using wrResponse As HttpWebResponse = DirectCast(Me.wrRequest.GetResponse(), HttpWebResponse)
                Select Case True
                    'for our purposes I think it's safe to assume that all xmls we will be dealing with will be UTF-8 encoded
                    Case wrResponse.ContentType.ToLower.Contains("/xml") OrElse wrResponse.ContentType.ToLower.Contains("charset=utf-8")
                        cEncoding = System.Text.Encoding.UTF8
                    Case Else
                        cEncoding = System.Text.Encoding.GetEncoding(28591)
                End Select
                Using Ms As Stream = wrResponse.GetResponseStream
                    If wrResponse.ContentEncoding.ToLower = "gzip" Then
                        sResponse = New StreamReader(New GZipStream(Ms, CompressionMode.Decompress), cEncoding, True).ReadToEnd
                    ElseIf wrResponse.ContentEncoding.ToLower = "deflate" Then
                        sResponse = New StreamReader(New DeflateStream(Ms, CompressionMode.Decompress), cEncoding, True).ReadToEnd
                    Else
                        sResponse = New StreamReader(Ms, cEncoding, True).ReadToEnd
                    End If
                End Using
                Me._responseuri = wrResponse.ResponseUri.ToString
            End Using
        Catch ex As Exception
        End Try

        Return sResponse
    End Function
    Private Function MakePostFieldText(ByVal Boundary As String, ByVal name As String, ByVal value As String) As String
        Return String.Concat(Boundary, vbCrLf, String.Format("Content-Disposition:form-data;name=""{0}""", name), vbCrLf, vbCrLf, value, vbCrLf)
    End Function
    Private Function MakePostFieldFile(ByVal Boundary As String, ByVal name As String) As String
        Return String.Concat(Boundary, vbCrLf, String.Format("Content-Disposition:form-data;name=""file"";filename=""{0}""", name), vbCrLf, "Content-Type: application/octet-stream", vbCrLf, vbCrLf)
    End Function

    Public Function PostDownloadData(ByVal URL As String, ByVal postDataList As List(Of String())) As String
        Dim sResponse As String = String.Empty
        Dim cEncoding As System.Text.Encoding
        Dim Idboundary As String = Convert.ToInt64(Functions.ConvertToUnixTimestamp(Now)).ToString
        Dim Boundary As String = String.Format("--{0}", Idboundary)
        Dim postDataBytes As New List(Of Byte())
        Me.Clear()
        System.Net.ServicePointManager.Expect100Continue = False
        Try
            For Each s() As String In postDataList
                If s.Count = 2 Then postDataBytes.Add(System.Text.Encoding.UTF8.GetBytes(String.Concat(MakePostFieldText(Boundary, s(0), s(1)))))
                If s.Count = 3 Then
                    Select Case s(2)
                        Case "file"  'array in list is {filename,filepath,"file"}
                            postDataBytes.Add(System.Text.Encoding.UTF8.GetBytes(String.Concat(MakePostFieldFile(Boundary, s(0)))))
                            postDataBytes.Add(File.ReadAllBytes(s(1)))
                            postDataBytes.Add(System.Text.Encoding.UTF8.GetBytes(String.Concat(vbCrLf, Boundary, vbCrLf)))
                    End Select
                End If
            Next
            postDataBytes.Add(System.Text.Encoding.UTF8.GetBytes(String.Concat(Boundary, vbCrLf)))

            Me.wrRequest = DirectCast(WebRequest.Create(URL), HttpWebRequest)
            Me.wrRequest.Timeout = 20000
            Me.wrRequest.Headers.Add("Accept-Encoding", "gzip,deflate")
            If Not String.IsNullOrEmpty(Master.eSettings.ProxyURI) AndAlso Master.eSettings.ProxyPort >= 0 Then
                Dim wProxy As New WebProxy(Master.eSettings.ProxyURI, Master.eSettings.ProxyPort)
                wProxy.BypassProxyOnLocal = True
                If Not String.IsNullOrEmpty(Master.eSettings.ProxyCreds.UserName) Then
                    wProxy.Credentials = Master.eSettings.ProxyCreds
                Else
                    wProxy.Credentials = CredentialCache.DefaultCredentials
                End If
                Me.wrRequest.Proxy = wProxy
            End If
            Me.wrRequest.Method = "POST"
            Me.wrRequest.ContentType = String.Concat("multipart/form-data;boundary=", Idboundary)
            Dim size As Integer = 0
            For i As Integer = 0 To postDataBytes.Count - 1
                size += postDataBytes(i).Length
            Next
            Me.wrRequest.ContentLength = size
            Dim newStream As Stream = Me.wrRequest.GetRequestStream()
            For i As Integer = 0 To postDataBytes.Count - 1
                newStream.Write(postDataBytes(i), 0, postDataBytes(i).Length)
            Next
            newStream.Close()

            Using wrResponse As HttpWebResponse = DirectCast(Me.wrRequest.GetResponse(), HttpWebResponse)
                Select Case True
                    Case wrResponse.ContentType.ToLower.Contains("/xml") OrElse wrResponse.ContentType.ToLower.Contains("charset=utf-8")
                        cEncoding = System.Text.Encoding.UTF8
                    Case Else
                        cEncoding = System.Text.Encoding.GetEncoding(28591)
                End Select
                Using Ms As Stream = wrResponse.GetResponseStream
                    If wrResponse.ContentEncoding.ToLower = "gzip" Then
                        sResponse = New StreamReader(New GZipStream(Ms, CompressionMode.Decompress), cEncoding, True).ReadToEnd
                    ElseIf wrResponse.ContentEncoding.ToLower = "deflate" Then
                        sResponse = New StreamReader(New DeflateStream(Ms, CompressionMode.Decompress), cEncoding, True).ReadToEnd
                    Else
                        sResponse = New StreamReader(Ms, cEncoding, True).ReadToEnd
                    End If
                End Using
                Me._responseuri = wrResponse.ResponseUri.ToString
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
        End Try

        Return sResponse
    End Function

    Public Function DownloadFile(ByVal URL As String, ByVal LocalFile As String, ByVal ReportUpdate As Boolean, ByVal Type As String) As String
        Dim outFile As String = String.Empty
        Dim urlExt As String = String.Empty

        Me._cancel = False
        Try
            Me.wrRequest = DirectCast(WebRequest.Create(URL), HttpWebRequest)
            Me.wrRequest.Timeout = 20000

            If Not String.IsNullOrEmpty(Master.eSettings.ProxyURI) AndAlso Master.eSettings.ProxyPort >= 0 Then
                Dim wProxy As New WebProxy(Master.eSettings.ProxyURI, Master.eSettings.ProxyPort)
                wProxy.BypassProxyOnLocal = True
                If Not String.IsNullOrEmpty(Master.eSettings.ProxyCreds.UserName) Then
                    wProxy.Credentials = Master.eSettings.ProxyCreds
                Else
                    wProxy.Credentials = CredentialCache.DefaultCredentials
                End If
                Me.wrRequest.Proxy = wProxy
            End If

            Try
                urlExt = Path.GetExtension(URL)
            Catch
            End Try

            Using wrResponse As HttpWebResponse = DirectCast(Me.wrRequest.GetResponse(), HttpWebResponse)
                Select Case True
                    Case Type = "trailer" AndAlso Master.eSettings.ValidExts.Contains(urlExt)
                        outFile = Path.Combine(Directory.GetParent(LocalFile).FullName, String.Concat(Path.GetFileNameWithoutExtension(LocalFile), If(Master.eSettings.DashTrailer, "-trailer", "[trailer]"), Path.GetExtension(URL)))
                    Case Type = "trailer" AndAlso Master.eSettings.ValidExts.Contains(Path.GetExtension(wrResponse.ResponseUri.AbsolutePath))
                        outFile = Path.Combine(Directory.GetParent(LocalFile).FullName, String.Concat(Path.GetFileNameWithoutExtension(LocalFile), If(Master.eSettings.DashTrailer, "-trailer", "[trailer]"), Path.GetExtension(wrResponse.ResponseUri.AbsolutePath)))
                    Case Type = "trailer" AndAlso wrResponse.ContentType.Contains("mp4")
                        outFile = Path.Combine(Directory.GetParent(LocalFile).FullName, String.Concat(Path.GetFileNameWithoutExtension(LocalFile), If(Master.eSettings.DashTrailer, "-trailer.mp4", "[trailer].mp4")))
                    Case Type = "trailer" AndAlso wrResponse.ContentType.Contains("flv")
                        outFile = Path.Combine(Directory.GetParent(LocalFile).FullName, String.Concat(Path.GetFileNameWithoutExtension(LocalFile), If(Master.eSettings.DashTrailer, "-trailer.flv", "[trailer].flv")))
                    Case Type = "trailer" AndAlso wrResponse.ContentType.Contains("webm")
                        outFile = Path.Combine(Directory.GetParent(LocalFile).FullName, String.Concat(Path.GetFileNameWithoutExtension(LocalFile), If(Master.eSettings.DashTrailer, "-trailer.webm", "[trailer].webm")))
                    Case Type = "other"
                        outFile = LocalFile
                End Select

                If Not String.IsNullOrEmpty(outFile) AndAlso Not wrResponse.ContentLength = 0 Then

                    If File.Exists(outFile) Then File.Delete(outFile)

                    Using Ms As Stream = wrResponse.GetResponseStream
                        Using mStream As New FileStream(outFile, FileMode.Create, FileAccess.Write)
                            Dim StreamBuffer(4096) As Byte
                            Dim BlockSize As Integer
                            Dim iProgress As Integer
                            Dim iCurrent As Integer
                            Do
                                BlockSize = Ms.Read(StreamBuffer, 0, 4096)
                                iCurrent += BlockSize
                                If BlockSize > 0 Then
                                    mStream.Write(StreamBuffer, 0, BlockSize)
                                    If ReportUpdate Then
                                        iProgress = Convert.ToInt32((iCurrent / wrResponse.ContentLength) * 100)
                                        RaiseEvent ProgressUpdated(iProgress)
                                    End If
                                End If
                            Loop While BlockSize > 0 AndAlso Not Me._cancel
                            StreamBuffer = Nothing
                        End Using
                    End Using
                End If

            End Using
        Catch ex As Exception
        End Try

        Return outFile
    End Function

    Public Sub DownloadImage()
        Try
            If StringUtils.isValidURL(Me._URL) Then
                Me.wrRequest = DirectCast(HttpWebRequest.Create(Me._URL), HttpWebRequest)
                Me.wrRequest.Timeout = 20000

                If Me._cancel Then Return

                If Not String.IsNullOrEmpty(Master.eSettings.ProxyURI) AndAlso Master.eSettings.ProxyPort >= 0 Then
                    Dim wProxy As New WebProxy(Master.eSettings.ProxyURI, Master.eSettings.ProxyPort)
                    wProxy.BypassProxyOnLocal = True
                    If Not String.IsNullOrEmpty(Master.eSettings.ProxyCreds.UserName) AndAlso _
                    Not String.IsNullOrEmpty(Master.eSettings.ProxyCreds.Password) Then
                        wProxy.Credentials = Master.eSettings.ProxyCreds
                    Else
                        wProxy.Credentials = CredentialCache.DefaultCredentials
                    End If
                    Me.wrRequest.Proxy = wProxy
                End If

                If Me._cancel Then Return

                Using wrResponse As WebResponse = Me.wrRequest.GetResponse()
                    If Me._cancel Then Return
                    Dim temp As String = wrResponse.ContentType.ToString
                    If wrResponse.ContentType.ToLower.Contains("image") Then
                        If Me._cancel Then Return                                                
                        Me._image = Image.FromStream(wrResponse.GetResponseStream)
                    End If
                End Using
            End If
        Catch
        End Try
    End Sub

    Public Function DownloadZip(ByVal URL As String) As Byte()
        Me.wrRequest = DirectCast(WebRequest.Create(URL), HttpWebRequest)

        Try
            Me.wrRequest.Timeout = 20000

            If Not String.IsNullOrEmpty(Master.eSettings.ProxyURI) AndAlso Master.eSettings.ProxyPort >= 0 Then
                Dim wProxy As New WebProxy(Master.eSettings.ProxyURI, Master.eSettings.ProxyPort)
                wProxy.BypassProxyOnLocal = True
                If Not String.IsNullOrEmpty(Master.eSettings.ProxyCreds.UserName) Then
                    wProxy.Credentials = Master.eSettings.ProxyCreds
                Else
                    wProxy.Credentials = CredentialCache.DefaultCredentials
                End If
                Me.wrRequest.Proxy = wProxy
            End If

            Using wrResponse As HttpWebResponse = DirectCast(Me.wrRequest.GetResponse(), HttpWebResponse)
                Return Functions.ReadStreamToEnd(wrResponse.GetResponseStream)
            End Using
        Catch
        End Try

        Return Nothing
    End Function

    Public Function IsDownloading() As Boolean
        Return Me.dThread.IsAlive
    End Function

    Public Function IsValidURL(ByVal URL As String) As Boolean
        Dim wrResponse As WebResponse
        Try
            Me.wrRequest = DirectCast(WebRequest.Create(URL), HttpWebRequest)

            If Not String.IsNullOrEmpty(Master.eSettings.ProxyURI) AndAlso Master.eSettings.ProxyPort >= 0 Then
                Dim wProxy As New WebProxy(Master.eSettings.ProxyURI, Master.eSettings.ProxyPort)
                wProxy.BypassProxyOnLocal = True
                If Not String.IsNullOrEmpty(Master.eSettings.ProxyCreds.UserName) Then
                    wProxy.Credentials = Master.eSettings.ProxyCreds
                Else
                    wProxy.Credentials = CredentialCache.DefaultCredentials
                End If
                Me.wrRequest.Proxy = wProxy
            End If

            Dim noCachePolicy As System.Net.Cache.HttpRequestCachePolicy = New System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore)
            Me.wrRequest.CachePolicy = noCachePolicy
            Me.wrRequest.Timeout = Master.eSettings.TrailerTimeout * 1000
            wrResponse = Me.wrRequest.GetResponse()
        Catch ex As Exception
            Return False
        End Try
        wrResponse.Close()
        wrResponse = Nothing
        Return True
    End Function

    Public Sub StartDownloadImage(ByVal sURL As String)
        Me.Clear()
        Me._URL = sURL
        Me.dThread = New Threading.Thread(AddressOf DownloadImage)
        Me.dThread.IsBackground = True
        Me.dThread.Start()
    End Sub

#End Region 'Methods
End Class