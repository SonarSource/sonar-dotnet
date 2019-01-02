namespace Tests.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using RestSharp;
    using RestSharp.Serializers;

    class Program
    {
        // RSPEC-4825: https://jira.sonarsource.com/browse/RSPEC-4825
        public void Rspec(string address, Uri uriAddress, HttpRequestMessage request, HttpContent content)
        {
            System.Net.Http.HttpClient httpClient = new HttpClient();
            // All the following are Questionable

            // client.GetAsync()
            httpClient.GetAsync(address);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this http request is sent safely.}}
            httpClient.GetAsync(address, HttpCompletionOption.ResponseContentRead);             // Noncompliant
            httpClient.GetAsync(uriAddress);                                                    // Noncompliant
            httpClient.GetAsync(uriAddress, HttpCompletionOption.ResponseContentRead);          // Noncompliant
            httpClient.GetAsync(address, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            //client.GetByteArrayAsync(...);
            httpClient.GetByteArrayAsync(address);      // Noncompliant
            httpClient.GetByteArrayAsync(uriAddress);   // Noncompliant

            //client.GetStreamAsync(...);
            httpClient.GetStreamAsync(address);         // Noncompliant
            httpClient.GetStreamAsync(uriAddress);      // Noncompliant

            //client.GetStringAsync(...);
            httpClient.GetStringAsync(address);         // Noncompliant
            httpClient.GetStringAsync(uriAddress);      // Noncompliant

            //client.SendAsync(...);
            httpClient.SendAsync(request);                                              // Noncompliant
            httpClient.SendAsync(request, CancellationToken.None);                      // Noncompliant
            httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);    // Noncompliant
            httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);    // Noncompliant

            //client.PostAsync(...);
            httpClient.PostAsync(address, content);                             // Noncompliant
            httpClient.PostAsync(address, content, CancellationToken.None);     // Noncompliant
            httpClient.PostAsync(uriAddress, content);                          // Noncompliant
            httpClient.PostAsync(uriAddress, content, CancellationToken.None);  // Noncompliant

            //client.PutAsync(...);
            httpClient.PutAsync(address, content);                              // Noncompliant
            httpClient.PutAsync(address, content, CancellationToken.None);      // Noncompliant
            httpClient.PutAsync(uriAddress, content);                           // Noncompliant
            httpClient.PutAsync(uriAddress, content, CancellationToken.None);   // Noncompliant

            //client.DeleteAsync(...);
            httpClient.DeleteAsync(address);                                    // Noncompliant
            httpClient.DeleteAsync(address, CancellationToken.None);            // Noncompliant
            httpClient.DeleteAsync(uriAddress);                                 // Noncompliant
            httpClient.DeleteAsync(uriAddress, CancellationToken.None);         // Noncompliant
        }

        public void RSPEC_WebClient(string address, Uri uriAddress, byte[] data,
            NameValueCollection values)
        {
            System.Net.WebClient webclient = new System.Net.WebClient();

            // All of the following are Questionable although there may be false positives if the URI scheme is "ftp" or "file"
            //webclient.Download * (...); // Any method starting with "Download"
            webclient.DownloadData(address);                            // Noncompliant
            webclient.DownloadDataAsync(uriAddress, new object());      // Noncompliant
            webclient.DownloadDataTaskAsync(uriAddress);                // Noncompliant
            webclient.DownloadFile(address, "filename");                // Noncompliant
            webclient.DownloadFileAsync(uriAddress, "filename");        // Noncompliant
            webclient.DownloadFileTaskAsync(address, "filename");       // Noncompliant
            webclient.DownloadString(uriAddress);                       // Noncompliant
            webclient.DownloadStringAsync(uriAddress, new object());    // Noncompliant
            webclient.DownloadStringTaskAsync(address);                 // Noncompliant

            // Should not raise for events
            webclient.DownloadDataCompleted += Webclient_DownloadDataCompleted;
            webclient.DownloadFileCompleted += Webclient_DownloadFileCompleted;
            webclient.DownloadProgressChanged -= Webclient_DownloadProgressChanged;
            webclient.DownloadStringCompleted -= Webclient_DownloadStringCompleted;


            //webclient.Open * (...); // Any method starting with "Open"
            webclient.OpenRead(address);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this http request is sent safely.}}
            webclient.OpenReadAsync(uriAddress, new object());              // Noncompliant
            webclient.OpenReadTaskAsync(address);                           // Noncompliant
            webclient.OpenWrite(address);                                   // Noncompliant
            webclient.OpenWriteAsync(uriAddress, "STOR", new object());     // Noncompliant
            webclient.OpenWriteTaskAsync(address, "POST");                  // Noncompliant

            webclient.OpenReadCompleted += Webclient_OpenReadCompleted;
            webclient.OpenWriteCompleted += Webclient_OpenWriteCompleted;

            //webclient.Upload * (...); // Any method starting with "Upload"
            webclient.UploadData(address, data);                        // Noncompliant
            webclient.UploadDataAsync(uriAddress, "STOR", data);        // Noncompliant
            webclient.UploadDataTaskAsync(address, "POST", data);       // Noncompliant
            webclient.UploadFile(address, "filename");                  // Noncompliant
            webclient.UploadFileAsync(uriAddress, "filename");          // Noncompliant
            webclient.UploadFileTaskAsync(uriAddress, "POST", "filename");  // Noncompliant
            webclient.UploadString(uriAddress, "data");                 // Noncompliant
            webclient.UploadStringAsync(uriAddress, "data");            // Noncompliant
            webclient.UploadStringTaskAsync(uriAddress, "data");        // Noncompliant
            webclient.UploadValues(address, values);                    // Noncompliant
            webclient.UploadValuesAsync(uriAddress, values);            // Noncompliant
            webclient.UploadValuesTaskAsync(address, "POST", values);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            // Should not raise for events
            webclient.UploadDataCompleted += Webclient_UploadDataCompleted;
            webclient.UploadFileCompleted += Webclient_UploadFileCompleted;
            webclient.UploadProgressChanged -= Webclient_UploadProgressChanged;
            webclient.UploadStringCompleted -= Webclient_UploadStringCompleted;
            webclient.UploadValuesCompleted -= Webclient_UploadValuesCompleted;
        }

        public void RSPEC_WebRequest(string address, Uri uriAddress)
        {
            // All of the following are Questionable although there may be false positives if the URI scheme is "ftp" or "file"
            //System.Net.WebRequest.Create(...);
            System.Net.WebRequest.Create(address);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure that this http request is sent safely.}}
            System.Net.WebRequest.Create(uriAddress);   // Noncompliant

            //System.Net.WebRequest.CreateDefault(...);
            System.Net.WebRequest.CreateDefault(uriAddress);    // Noncompliant

            // The following is always Questionable
            //System.Net.WebRequest.CreateHttp(...);
            System.Net.WebRequest.CreateHttp(address);      // Noncompliant
            System.Net.WebRequest.CreateHttp(uriAddress);   // Noncompliant
        }

        public void RSPEC_RestSharp(string address, Uri uriAddress)
        {
            // === RestSharp ===
            // Questionable, as well as any other instantiation of the RestSharp.IRestRequest interface.
            IRestRequest restRequest = new RestSharp.RestRequest();         // Noncompliant
            restRequest = new RestSharp.RestRequest(RestSharp.Method.PUT);  // Noncompliant
            restRequest = new RestSharp.RestRequest(address, RestSharp.Method.GET, RestSharp.DataFormat.Json);  // Noncompliant
            restRequest = new RestSharp.RestRequest(uriAddress, RestSharp.Method.GET, RestSharp.DataFormat.Json);
//                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            new DerivedRestRequest();
//          ^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure that this http request is sent safely.}}
            restRequest = new DerivedRestRequest(address);  // Noncompliant
            new RestRequestImpl();                          // Noncompliant
            restRequest = new RestRequestImpl(address);
//                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }

        #region Event handlers used in tests

        private void Webclient_UploadValuesCompleted(object sender, System.Net.UploadValuesCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_UploadStringCompleted(object sender, System.Net.UploadStringCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_UploadProgressChanged(object sender, System.Net.UploadProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_UploadFileCompleted(object sender, System.Net.UploadFileCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_UploadDataCompleted(object sender, System.Net.UploadDataCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_OpenWriteCompleted(object sender, System.Net.OpenWriteCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_OpenReadCompleted(object sender, System.Net.OpenReadCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_DownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Webclient_DownloadDataCompleted(object sender, System.Net.DownloadDataCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion // of event handlers

        #region Custom IRestRequst implementations

        private class DerivedRestRequest : RestSharp.RestRequest
        {
            public DerivedRestRequest() : base() { }
            public DerivedRestRequest(string resource) : base(resource) { }
        }

        private class RestRequestImpl : RestSharp.IRestRequest
        {
            public RestRequestImpl() : base() { }
            public RestRequestImpl(string resource) { }


            public bool AlwaysMultipartFormData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public ISerializer JsonSerializer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public IXmlSerializer XmlSerializer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Action<Stream, IHttpResponse> AdvancedResponseWriter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Action<Stream> ResponseWriter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public List<Parameter> Parameters => throw new NotImplementedException();

            public List<FileParameter> Files => throw new NotImplementedException();

            public Method Method { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string Resource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public DataFormat RequestFormat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string RootElement { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string DateFormat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string XmlNamespace { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public ICredentials Credentials { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public int ReadWriteTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public int Attempts => throw new NotImplementedException();

            public bool UseDefaultCredentials { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IList<DecompressionMethods> AllowedDecompressionMethods => throw new NotImplementedException();

            public Action<IRestResponse> OnBeforeDeserialization { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IRestRequest AddBody(object obj, string xmlNamespace)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddBody(object obj)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddCookie(string name, string value)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddDecompressionMethod(DecompressionMethods decompressionMethod)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddFile(string name, string path, string contentType = null)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddFile(string name, byte[] bytes, string fileName, string contentType = null)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddFile(string name, Action<Stream> writer, string fileName, long contentLength, string contentType = null)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddFileBytes(string name, byte[] bytes, string filename, string contentType = "application/x-gzip")
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddHeader(string name, string value)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddJsonBody(object obj)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddObject(object obj, params string[] includedProperties)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddObject(object obj)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddOrUpdateParameter(Parameter p)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddOrUpdateParameter(string name, object value)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddOrUpdateParameter(string name, object value, ParameterType type)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddOrUpdateParameter(string name, object value, string contentType, ParameterType type)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddParameter(Parameter p)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddParameter(string name, object value)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddParameter(string name, object value, ParameterType type)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddParameter(string name, object value, string contentType, ParameterType type)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddQueryParameter(string name, string value)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddQueryParameter(string name, string value, bool encode)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddUrlSegment(string name, string value)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddXmlBody(object obj)
            {
                throw new NotImplementedException();
            }

            public IRestRequest AddXmlBody(object obj, string xmlNamespace)
            {
                throw new NotImplementedException();
            }

            public void IncreaseNumAttempts()
            {
                throw new NotImplementedException();
            }
        }

        #endregion // of custom IRestRequest implementations
    }
}
