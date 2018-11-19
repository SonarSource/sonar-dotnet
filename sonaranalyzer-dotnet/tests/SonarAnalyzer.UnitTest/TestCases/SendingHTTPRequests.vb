Imports System
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports RestSharp
Imports RestSharp.Serializers

Namespace Tests.Diagnostics

    Class Program

        ' RSPEC-4825: https://jira.sonarsource.com/browse/RSPEC-4825
        Public Sub Rspec(address As String, uriAddress As Uri, request As HttpRequestMessage, content As HttpContent)

            Dim httpClient = New HttpClient()
            ' All the following are Questionable

            ' client.GetAsync()
            httpClient.GetAsync(address)
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this http request is sent safely.}}
            httpClient.GetAsync(address, HttpCompletionOption.ResponseContentRead)             ' Noncompliant
            httpClient.GetAsync(uriAddress)                                                    ' Noncompliant
            httpClient.GetAsync(uriAddress, HttpCompletionOption.ResponseContentRead)          ' Noncompliant
            httpClient.GetAsync(address, HttpCompletionOption.ResponseContentRead, CancellationToken.None)
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            'client.GetByteArrayAsync(...)
            httpClient.GetByteArrayAsync(address)      ' Noncompliant
            httpClient.GetByteArrayAsync(uriAddress)   ' Noncompliant

            'client.GetStreamAsync(...)
            httpClient.GetStreamAsync(address)         ' Noncompliant
            httpClient.GetStreamAsync(uriAddress)      ' Noncompliant

            'client.GetStringAsync(...)
            httpClient.GetStringAsync(address)         ' Noncompliant
            httpClient.GetStringAsync(uriAddress)      ' Noncompliant

            'client.SendAsync(...)
            httpClient.SendAsync(request)                                              ' Noncompliant
            httpClient.SendAsync(request, CancellationToken.None)                      ' Noncompliant
            httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)    ' Noncompliant
            httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None)    ' Noncompliant

            'client.PostAsync(...)
            httpClient.PostAsync(address, content)                             ' Noncompliant
            httpClient.PostAsync(address, content, CancellationToken.None)     ' Noncompliant
            httpClient.PostAsync(uriAddress, content)                          ' Noncompliant
            httpClient.PostAsync(uriAddress, content, CancellationToken.None)  ' Noncompliant

            'client.PutAsync(...)
            httpClient.PutAsync(address, content)                              ' Noncompliant
            httpClient.PutAsync(address, content, CancellationToken.None)      ' Noncompliant
            httpClient.PutAsync(uriAddress, content)                           ' Noncompliant
            httpClient.PutAsync(uriAddress, content, CancellationToken.None)   ' Noncompliant

            'client.DeleteAsync(...)
            httpClient.DeleteAsync(address)                                    ' Noncompliant
            httpClient.DeleteAsync(address, CancellationToken.None)            ' Noncompliant
            httpClient.DeleteAsync(uriAddress)                                 ' Noncompliant
            httpClient.DeleteAsync(uriAddress, CancellationToken.None)         ' Noncompliant
        End Sub

        Public Sub RSPEC_WebClient(address As String, uriAddress As Uri, data As Byte(),
            values As NameValueCollection)
            Dim WebClient As System.Net.WebClient = New System.Net.WebClient()

            ' All of the following are Questionable although there may be false positives if the URI scheme Is "ftp" Or "file"
            'webclient.Download * (...) ' Any method starting with "Download"
            WebClient.DownloadData(address)                            ' Noncompliant
            WebClient.DownloadDataAsync(uriAddress, New Object())      ' Noncompliant
            WebClient.DownloadDataTaskAsync(uriAddress)                ' Noncompliant
            WebClient.DownloadFile(address, "filename")                ' Noncompliant
            WebClient.DownloadFileAsync(uriAddress, "filename")        ' Noncompliant
            WebClient.DownloadFileTaskAsync(address, "filename")       ' Noncompliant
            WebClient.DownloadString(uriAddress)                       ' Noncompliant
            WebClient.DownloadStringAsync(uriAddress, New Object())    ' Noncompliant
            WebClient.DownloadStringTaskAsync(address)                 ' Noncompliant

            ' Should Not raise for events
            AddHandler WebClient.DownloadDataCompleted, AddressOf Webclient_DownloadDataCompleted
            AddHandler WebClient.DownloadFileCompleted, AddressOf Webclient_DownloadFileCompleted
            RemoveHandler WebClient.DownloadProgressChanged, AddressOf Webclient_DownloadProgressChanged
            RemoveHandler WebClient.DownloadStringCompleted, AddressOf Webclient_DownloadStringCompleted


            'webclient.Open * (...) ' Any method starting with "Open"
            WebClient.OpenRead(address)
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this http request is sent safely.}}
            WebClient.OpenReadAsync(uriAddress, New Object())              ' Noncompliant
            WebClient.OpenReadTaskAsync(address)                           ' Noncompliant
            WebClient.OpenWrite(address)                                   ' Noncompliant
            WebClient.OpenWriteAsync(uriAddress, "STOR", New Object())     ' Noncompliant
            WebClient.OpenWriteTaskAsync(address, "POST")                  ' Noncompliant

            AddHandler WebClient.OpenReadCompleted, AddressOf Webclient_OpenReadCompleted
            AddHandler WebClient.OpenWriteCompleted, AddressOf Webclient_OpenWriteCompleted

            'webclient.Upload * (...) ' Any method starting with "Upload"
            WebClient.UploadData(address, data)                        ' Noncompliant
            WebClient.UploadDataAsync(uriAddress, "STOR", data)        ' Noncompliant
            WebClient.UploadDataTaskAsync(address, "POST", data)       ' Noncompliant
            WebClient.UploadFile(address, "filename")                  ' Noncompliant
            WebClient.UploadFileAsync(uriAddress, "filename")          ' Noncompliant
            WebClient.UploadFileTaskAsync(uriAddress, "POST", "filename")  ' Noncompliant
            WebClient.UploadString(uriAddress, "data")                 ' Noncompliant
            WebClient.UploadStringAsync(uriAddress, "data")            ' Noncompliant
            WebClient.UploadStringTaskAsync(uriAddress, "data")        ' Noncompliant
            WebClient.UploadValues(address, values)                    ' Noncompliant
            WebClient.UploadValuesAsync(uriAddress, values)            ' Noncompliant
            WebClient.UploadValuesTaskAsync(address, "POST", values)
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            ' Should not raise for events
            AddHandler WebClient.UploadDataCompleted, AddressOf Webclient_UploadDataCompleted
            AddHandler WebClient.UploadFileCompleted, AddressOf Webclient_UploadFileCompleted
            RemoveHandler WebClient.UploadProgressChanged, AddressOf Webclient_UploadProgressChanged
            RemoveHandler WebClient.UploadStringCompleted, AddressOf Webclient_UploadStringCompleted
            RemoveHandler WebClient.UploadValuesCompleted, AddressOf Webclient_UploadValuesCompleted
        End Sub

        Public Sub RSPEC_WebRequest(address As String, uriAddress As Uri)
            ' All of the following are Questionable although there may be false positives if the URI scheme Is "ftp" Or "file"
            'System.Net.WebRequest.Create(...)
            System.Net.WebRequest.Create(address)
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure that this http request is sent safely.}}
            WebRequest.Create(uriAddress)              ' Noncompliant

            'System.Net.WebRequest.CreateDefault(...)
            System.Net.WebRequest.CreateDefault(uriAddress)    ' Noncompliant

            ' The following Is always Questionable
            'System.Net.WebRequest.CreateHttp(...)
            System.Net.WebRequest.CreateHttp(address)      ' Noncompliant
            WebRequest.CreateHttp(uriAddress)              ' Noncompliant
        End Sub

        Public Sub RSPEC_RestSharp(address As String, uriAddress As Uri)
            '=== RestSharp ===
            ' Questionable, as well as any other instantiation of the RestSharp.IRestRequest interface.
            Dim restReq As IRestRequest = New RestSharp.RestRequest()   ' Noncompliant
            restReq = New RestSharp.RestRequest(RestSharp.Method.PUT)   ' Noncompliant
            restReq = New RestSharp.RestRequest(address, RestSharp.Method.GET, RestSharp.DataFormat.Json) ' Noncompliant
            restReq = New RestSharp.RestRequest(uriAddress, RestSharp.Method.GET, RestSharp.DataFormat.Json)
'                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            restReq = New DerivedRestRequest()          ' Noncompliant
            restReq = New DerivedRestRequest(address)   ' Noncompliant
            restReq = New RestRequestImpl()             ' Noncompliant
            restReq = New RestRequestImpl(address)
'                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        End Sub

#Region "Event handlers used in tests"

        Private Sub Webclient_UploadValuesCompleted(sender As Object, e As System.Net.UploadValuesCompletedEventArgs)
            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_UploadStringCompleted(sender As Object, e As System.Net.UploadStringCompletedEventArgs)
            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_UploadProgressChanged(sender As Object, e As System.Net.UploadProgressChangedEventArgs)

            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_UploadFileCompleted(sender As Object, e As System.Net.UploadFileCompletedEventArgs)

            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_UploadDataCompleted(sender As Object, e As System.Net.UploadDataCompletedEventArgs)

            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_OpenWriteCompleted(sender As Object, e As System.Net.OpenWriteCompletedEventArgs)

            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_OpenReadCompleted(sender As Object, e As System.Net.OpenReadCompletedEventArgs)

            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_DownloadStringCompleted(sender As Object, e As System.Net.DownloadStringCompletedEventArgs)

            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_DownloadProgressChanged(sender As Object, e As System.Net.DownloadProgressChangedEventArgs)

            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_DownloadFileCompleted(sender As Object, e As System.ComponentModel.AsyncCompletedEventArgs)

            Throw New NotImplementedException()
        End Sub

        Private Sub Webclient_DownloadDataCompleted(sender As Object, e As System.Net.DownloadDataCompletedEventArgs)

            Throw New NotImplementedException()
        End Sub

#End Region ' of Event handlers

#Region "Custom IRestRequst implementations"

        Private Class DerivedRestRequest
            Inherits RestSharp.RestRequest

            Public Sub New()
            End Sub

            Public Sub New(resource As String)
                MyBase.New(resource)
            End Sub
        End Class

        Private Class RestRequestImpl
            Implements RestSharp.IRestRequest

            Public Sub New()
            End Sub

            Public Sub New(resource As String)
            End Sub

            Public Property AlwaysMultipartFormData As Boolean Implements IRestRequest.AlwaysMultipartFormData
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As Boolean)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property JsonSerializer As ISerializer Implements IRestRequest.JsonSerializer
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As ISerializer)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property XmlSerializer As IXmlSerializer Implements IRestRequest.XmlSerializer
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As IXmlSerializer)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property AdvancedResponseWriter As Action(Of Stream, IHttpResponse) Implements IRestRequest.AdvancedResponseWriter
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As Action(Of Stream, IHttpResponse))
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property ResponseWriter As Action(Of Stream) Implements IRestRequest.ResponseWriter
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As Action(Of Stream))
                    Throw New NotImplementedException()
                End Set
            End Property

            Public ReadOnly Property Parameters As List(Of Parameter) Implements IRestRequest.Parameters
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public ReadOnly Property Files As List(Of FileParameter) Implements IRestRequest.Files
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public Property Method As Method Implements IRestRequest.Method
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As Method)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property Resource As String Implements IRestRequest.Resource
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As String)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property RequestFormat As DataFormat Implements IRestRequest.RequestFormat
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As DataFormat)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property RootElement As String Implements IRestRequest.RootElement
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As String)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property DateFormat As String Implements IRestRequest.DateFormat
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As String)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property XmlNamespace As String Implements IRestRequest.XmlNamespace
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As String)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property Credentials As ICredentials Implements IRestRequest.Credentials
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As ICredentials)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property Timeout As Integer Implements IRestRequest.Timeout
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As Integer)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property ReadWriteTimeout As Integer Implements IRestRequest.ReadWriteTimeout
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As Integer)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public ReadOnly Property Attempts As Integer Implements IRestRequest.Attempts
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public Property UseDefaultCredentials As Boolean Implements IRestRequest.UseDefaultCredentials
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As Boolean)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public ReadOnly Property AllowedDecompressionMethods As IList(Of DecompressionMethods) Implements IRestRequest.AllowedDecompressionMethods
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public Property OnBeforeDeserialization As Action(Of IRestResponse) Implements IRestRequest.OnBeforeDeserialization
                Get
                    Throw New NotImplementedException()
                End Get
                Set(value As Action(Of IRestResponse))
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Sub IncreaseNumAttempts() Implements IRestRequest.IncreaseNumAttempts
                Throw New NotImplementedException()
            End Sub

            Public Function AddFile(name As String, path As String, Optional contentType As String = Nothing) As IRestRequest Implements IRestRequest.AddFile
                Throw New NotImplementedException()
            End Function

            Public Function AddFile(name As String, bytes() As Byte, fileName As String, Optional contentType As String = Nothing) As IRestRequest Implements IRestRequest.AddFile
                Throw New NotImplementedException()
            End Function

            Public Function AddFile(name As String, writer As Action(Of Stream), fileName As String, contentLength As Long, Optional contentType As String = Nothing) As IRestRequest Implements IRestRequest.AddFile
                Throw New NotImplementedException()
            End Function

            Public Function AddFileBytes(name As String, bytes() As Byte, filename As String, Optional contentType As String = "application/x-gzip") As IRestRequest Implements IRestRequest.AddFileBytes
                Throw New NotImplementedException()
            End Function

            Public Function AddBody(obj As Object, xmlNamespace As String) As IRestRequest Implements IRestRequest.AddBody
                Throw New NotImplementedException()
            End Function

            Public Function AddBody(obj As Object) As IRestRequest Implements IRestRequest.AddBody
                Throw New NotImplementedException()
            End Function

            Public Function AddJsonBody(obj As Object) As IRestRequest Implements IRestRequest.AddJsonBody
                Throw New NotImplementedException()
            End Function

            Public Function AddXmlBody(obj As Object) As IRestRequest Implements IRestRequest.AddXmlBody
                Throw New NotImplementedException()
            End Function

            Public Function AddXmlBody(obj As Object, xmlNamespace As String) As IRestRequest Implements IRestRequest.AddXmlBody
                Throw New NotImplementedException()
            End Function

            Public Function AddObject(obj As Object, ParamArray includedProperties() As String) As IRestRequest Implements IRestRequest.AddObject
                Throw New NotImplementedException()
            End Function

            Public Function AddObject(obj As Object) As IRestRequest Implements IRestRequest.AddObject
                Throw New NotImplementedException()
            End Function

            Public Function AddParameter(p As Parameter) As IRestRequest Implements IRestRequest.AddParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddParameter(name As String, value As Object) As IRestRequest Implements IRestRequest.AddParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddParameter(name As String, value As Object, type As ParameterType) As IRestRequest Implements IRestRequest.AddParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddParameter(name As String, value As Object, contentType As String, type As ParameterType) As IRestRequest Implements IRestRequest.AddParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddOrUpdateParameter(p As Parameter) As IRestRequest Implements IRestRequest.AddOrUpdateParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddOrUpdateParameter(name As String, value As Object) As IRestRequest Implements IRestRequest.AddOrUpdateParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddOrUpdateParameter(name As String, value As Object, type As ParameterType) As IRestRequest Implements IRestRequest.AddOrUpdateParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddOrUpdateParameter(name As String, value As Object, contentType As String, type As ParameterType) As IRestRequest Implements IRestRequest.AddOrUpdateParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddHeader(name As String, value As String) As IRestRequest Implements IRestRequest.AddHeader
                Throw New NotImplementedException()
            End Function

            Public Function AddCookie(name As String, value As String) As IRestRequest Implements IRestRequest.AddCookie
                Throw New NotImplementedException()
            End Function

            Public Function AddUrlSegment(name As String, value As String) As IRestRequest Implements IRestRequest.AddUrlSegment
                Throw New NotImplementedException()
            End Function

            Public Function AddQueryParameter(name As String, value As String) As IRestRequest Implements IRestRequest.AddQueryParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddQueryParameter(name As String, value As String, encode As Boolean) As IRestRequest Implements IRestRequest.AddQueryParameter
                Throw New NotImplementedException()
            End Function

            Public Function AddDecompressionMethod(decompressionMethod As DecompressionMethods) As IRestRequest Implements IRestRequest.AddDecompressionMethod
                Throw New NotImplementedException()
            End Function
        End Class

#End Region ' of custom IRestRequest implementations
    End Class
End Namespace
