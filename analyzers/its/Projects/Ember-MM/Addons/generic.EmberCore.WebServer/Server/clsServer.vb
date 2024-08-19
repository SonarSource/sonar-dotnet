Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Collections
Imports System.ComponentModel
Imports System.Data
Imports System.IO
Imports System.Text
Imports System.Configuration
Imports System.Environment
Imports System.Collections.Specialized
Imports System.Xml.Serialization
Imports EmberAPI


Public Class clsServer
    Private sMyWebServerRoot As String
    Private mySocket As Socket
    Private _DefaultPage As String
    Private Shared EmberPages As New ProccessPages

    Public Sub New(ByVal s As Socket, ByVal Location As String, ByVal DefaultPage As String)
        mySocket = s
        If Microsoft.VisualBasic.Right(Location, 1) = Path.DirectorySeparatorChar Then
            Location = Mid(Location, 1, Len(Location) - 1)
        End If
        sMyWebServerRoot = String.Concat(Location, Path.DirectorySeparatorChar)
        _DefaultPage = DefaultPage
    End Sub
    Public Function GetMimeType(ByVal filename As String) As String
        Try
            Dim ext As String = Path.GetExtension(filename).ToLower
            Return WebServerModule.MimeTypes.MimeTypes.FirstOrDefault(Function(y) y.Ext = ext).MineType
        Catch ex As Exception
        End Try
        Return ""
    End Function
    Public Sub HandleConnection()
        Dim iStartPos As Integer = 0
        Dim sRequest As String = String.Empty
        Dim sDirName As String = String.Empty
        Dim sRequestedFile As String = String.Empty
        Dim sErrorMessage As String = String.Empty
        Dim sPhysicalFilePath As String = String.Empty
        Dim bReceive As Byte()
        ReDim bReceive(1024)
        Try
            Dim i As Integer = mySocket.Receive(bReceive, bReceive.Length, 0)
        Catch ex As Exception
            Return
        End Try
        Dim sbuffer As String = Encoding.ASCII.GetString(bReceive)
        Dim qscoll As NameValueCollection = Nothing
        Dim resizeImage As Boolean = False
        iStartPos = sbuffer.IndexOf("HTTP", 1)
        If iStartPos < 0 Then Return
        Dim sHttpVersion As String = sbuffer.Substring(iStartPos, 8)

        If Not sbuffer.ToUpper.StartsWith("GET") Then
            mySocket.Close()
            Return
        End If
        sRequest = Web.HttpUtility.UrlDecode(sbuffer.Substring(0, iStartPos - 1))
        sRequest.Replace("\\", "/")
        Dim sMimeType As String = String.Empty
        'If ((sRequest.IndexOf(".") < 1) AndAlso (Not sRequest.EndsWith("/"))) Then
        'sRequest = sRequest & "/"
        'End If
		iStartPos = sRequest.LastIndexOf("/") + 1
		If (iStartPos > 0) Then
			sRequestedFile = sRequest.Substring(iStartPos)
		Else
			Master.eLog.WriteToErrorLog("Error: iStartPos = 0", sbuffer & " - " & sRequest, "Error")
			mySocket.Close()
			Return
		End If

		If sRequest.IndexOf("?") >= 0 Then
			qscoll = Web.HttpUtility.ParseQueryString(sRequest.Substring(sRequest.IndexOf("?") + 1))
			sRequestedFile = sRequest.Substring(iStartPos, sRequest.IndexOf("?") - iStartPos)
		End If
        If sRequest.Contains("<$RELOAD_DB>") Then
            sRequest = sRequest.Replace("<$RELOAD_DB>", String.Empty)
            ProccessPages.ReloadDatabase = True
        End If
        If sRequest.Contains("<$THUMB>") Then
            resizeImage = True
            sRequest = sRequest.Replace("<$THUMB>", String.Empty)
        End If
        If sRequest.Contains("<$MSOURCE>") Then
            sDirName = EmberPages.GetSourceRealPath(sRequest)
            sDirName = sDirName.Substring(0, sDirName.Length - sRequestedFile.Length - 1)
            sPhysicalFilePath = String.Concat(sDirName, Path.DirectorySeparatorChar, sRequestedFile)
        ElseIf sRequest.Contains("<$ESOURCE>") Then
            sDirName = sRequest.Substring(sRequest.IndexOf("<$ESOURCE>")).Replace("/", Path.DirectorySeparatorChar).Replace("<$ESOURCE>", Functions.AppPath)
            sDirName = sDirName.Substring(0, sDirName.Length - sRequestedFile.Length - 1)
            sPhysicalFilePath = Path.Combine(sDirName, sRequestedFile)
        Else
            sDirName = sRequest.Substring(sRequest.IndexOf("/") + 1, sRequest.LastIndexOf("/") - sRequest.IndexOf("/"))
            If (sRequestedFile.Length = 0) Then
                sRequestedFile = _DefaultPage
                sPhysicalFilePath = String.Concat(sMyWebServerRoot, sDirName.Replace("/", Path.DirectorySeparatorChar), sRequestedFile)
                If Not File.Exists(sPhysicalFilePath) AndAlso (sDirName = "" OrElse sDirName = "/") Then
                    sErrorMessage = "<H2>Welcome to the Ember Media Manager WebServer<BR>"
                    sErrorMessage = sErrorMessage & "<BR>No default page was found."
                    SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found")
                    SendToBrowser(sErrorMessage)
                    mySocket.Close()
                    Return
                Else
                    sPhysicalFilePath = String.Concat(sMyWebServerRoot, sDirName, sRequestedFile)
                End If
            Else
                sPhysicalFilePath = String.Concat(sMyWebServerRoot, sDirName, sRequestedFile)
            End If
        End If
        sMimeType = GetMimeType(sRequestedFile)
        If String.IsNullOrEmpty(sMimeType) Then
            ' unknown type
            mySocket.Close()
            Return
        End If
        If Not File.Exists(sPhysicalFilePath) Then
            sErrorMessage = "<H2>404 Error! File Does Not Exist...</H2>"
            SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found")
            SendToBrowser(sErrorMessage)
        Else
            If Path.GetExtension(sPhysicalFilePath) = ".htme" Then
                Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(EmberPages.MovieBuildHTML(sPhysicalFilePath, qscoll))
                SendHeader(sHttpVersion, sMimeType, bytes.Length, " 200 OK")
                SendToBrowser(bytes)
            ElseIf resizeImage Then
                Dim _image As System.Drawing.Image = internalResizeImage(sPhysicalFilePath)
                Dim imgStream As MemoryStream = New MemoryStream()
                _image.Save(imgStream, System.Drawing.Imaging.ImageFormat.Jpeg)
                imgStream.Close()
                Dim byteArray As Byte() = imgStream.ToArray()
                imgStream.Dispose()
                sMimeType = GetMimeType(".jpg")
                SendHeader(sHttpVersion, sMimeType, byteArray.Length, " 200 OK")

                SendToBrowser(byteArray)
            Else
                Dim byteArray() As Byte = Nothing
                Dim _FileStream As New System.IO.FileStream(sPhysicalFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read)
                Dim _BinaryReader As New System.IO.BinaryReader(_FileStream)
                Dim _TotalBytes As Long = New System.IO.FileInfo(sPhysicalFilePath).Length
                byteArray = _BinaryReader.ReadBytes(CInt(Fix(_TotalBytes)))
                _FileStream.Close()
                _FileStream.Dispose()
                _BinaryReader.Close()
                SendHeader(sHttpVersion, sMimeType, byteArray.Length, " 200 OK")

                SendToBrowser(byteArray)
            End If

        End If
        mySocket.Close()
    End Sub
    Function internalResizeImage(ByVal sPath As String) As System.Drawing.Image
        Dim _image As System.Drawing.Image = Nothing
        Dim ms As MemoryStream = New MemoryStream()
        If Not String.IsNullOrEmpty(sPath) AndAlso File.Exists(sPath) Then
            Try
                Using fsImage As New FileStream(sPath, FileMode.Open, FileAccess.Read)
                    ms.SetLength(fsImage.Length)
                    fsImage.Read(ms.GetBuffer(), 0, Convert.ToInt32(fsImage.Length))
                    ms.Flush()
                    _image = New Bitmap(ms)
                End Using
                ImageUtils.ResizeImage(_image, 100, 140)
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error: " & sPath)
            End Try
        End If
        Return _image
    End Function



    Private Sub SendHeader(ByVal sHttpVersion As String, ByVal sMIMEHeader As String, ByVal iTotBytes As Integer, ByVal sStatusCode As String)
        Dim sBuffer As String = ""
        If (sMIMEHeader.Length = 0) Then sMIMEHeader = "text/html" ' // Default Mime Type is text/html
        sBuffer = sBuffer & sHttpVersion & sStatusCode & vbNewLine
        sBuffer = sBuffer & "Server: Ember Media Manager WebServer" & vbNewLine
        sBuffer = sBuffer & "Content-Type: " & sMIMEHeader & vbNewLine
        sBuffer = sBuffer & "Accept-Ranges: bytes" & vbNewLine
        sBuffer = sBuffer & "Content-Length: " & iTotBytes & vbNewLine & vbNewLine

        Dim bSendData As Byte() = Encoding.ASCII.GetBytes(sBuffer)
        SendToBrowser(bSendData)
    End Sub

    Private Sub SendToBrowser(ByVal sData As String)
        SendToBrowser(Encoding.ASCII.GetBytes(sData))
    End Sub

    Private Sub SendToBrowser(ByVal bSendData As Byte())
        If (mySocket.Connected) Then
            Try
                Dim numbytes As Integer
                numbytes = mySocket.Send(bSendData, bSendData.Length, 0)
                If numbytes = -1 Then
                Else
                End If
            Catch ex As Exception
            End Try
        Else
        End If
    End Sub
End Class
