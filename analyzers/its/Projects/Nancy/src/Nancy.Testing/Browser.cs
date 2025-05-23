namespace Nancy.Testing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Nancy.Bootstrapper;
    using Nancy.Configuration;
    using Nancy.Helpers;
    using Nancy.IO;

    /// <summary>
    /// Provides the capability of executing a request with Nancy, using a specific configuration provided by an <see cref="INancyBootstrapper"/> instance.
    /// </summary>
    public class Browser : IHideObjectMembers
    {
        private readonly Action<BrowserContext> defaultBrowserContext;
        private readonly INancyEngine engine;
        private readonly INancyEnvironment environment;

        private readonly IDictionary<string, string> cookies = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Browser"/> class, with the
        /// provided <see cref="ConfigurableBootstrapper"/> configuration.
        /// </summary>
        /// <param name="action">The <see cref="ConfigurableBootstrapper"/> configuration that should be used by the bootstrapper.</param>
        /// <param name="defaults">The default <see cref="BrowserContext"/> that should be used in a all requests through this browser object.</param>
        public Browser(Action<ConfigurableBootstrapper.ConfigurableBootstrapperConfigurator> action, Action<BrowserContext> defaults = null)
            : this(new ConfigurableBootstrapper(action), defaults)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Browser"/> class.
        /// </summary>
        /// <param name="bootstrapper">A <see cref="INancyBootstrapper"/> instance that determines the Nancy configuration that should be used by the browser.</param>
        /// <param name="defaults">The default <see cref="BrowserContext"/> that should be used in a all requests through this browser object.</param>
        public Browser(INancyBootstrapper bootstrapper, Action<BrowserContext> defaults = null)
        {
            bootstrapper.Initialise();
            this.engine = bootstrapper.GetEngine();
            this.environment = bootstrapper.GetEnvironment();
            this.defaultBrowserContext = defaults ?? DefaultBrowserContext;
        }

        /// <summary>
        /// Performs a DELETE request against Nancy.
        /// </summary>
        /// <param name="path">The path that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Delete(string path, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("DELETE", path, browserContext);
        }

        /// <summary>
        /// Performs a DELETE request against Nancy.
        /// </summary>
        /// <param name="url">The url that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Delete(Url url, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("DELETE", url, browserContext);
        }

        /// <summary>
        /// Performs a GET request against Nancy.
        /// </summary>
        /// <param name="path">The path that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Get(string path, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("GET", path, browserContext);
        }

        /// <summary>
        /// Performs a GET request against Nancy.
        /// </summary>
        /// <param name="url">The url that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Get(Url url, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("GET", url, browserContext);
        }

        /// <summary>
        /// Performs a HEAD request against Nancy.
        /// </summary>
        /// <param name="path">The path that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Head(string path, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("HEAD", path, browserContext);
        }

        /// <summary>
        /// Performs a HEAD request against Nancy.
        /// </summary>
        /// <param name="url">The url that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Head(Url url, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("HEAD", url, browserContext);
        }

        /// <summary>
        /// Performs a OPTIONS request against Nancy.
        /// </summary>
        /// <param name="path">The path that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Options(string path, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("OPTIONS", path, browserContext);
        }

        /// <summary>
        /// Performs a OPTIONS request against Nancy.
        /// </summary>
        /// <param name="url">The url that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Options(Url url, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("OPTIONS", url, browserContext);
        }

        /// <summary>
        /// Performs a PATCH request against Nancy.
        /// </summary>
        /// <param name="path">The path that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Patch(string path, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("PATCH", path, browserContext);
        }

        /// <summary>
        /// Performs a PATCH request against Nancy.
        /// </summary>
        /// <param name="url">The url that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Patch(Url url, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("PATCH", url, browserContext);
        }

        /// <summary>
        /// Performs a POST request against Nancy.
        /// </summary>
        /// <param name="path">The path that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Post(string path, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("POST", path, browserContext);
        }

        /// <summary>
        /// Performs a POST request against Nancy.
        /// </summary>
        /// <param name="url">The url that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Post(Url url, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("POST", url, browserContext);
        }

        /// <summary>
        /// Performs a PUT request against Nancy.
        /// </summary>
        /// <param name="path">The path that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Put(string path, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("PUT", path, browserContext);
        }

        /// <summary>
        /// Performs a PUT request against Nancy.
        /// </summary>
        /// <param name="url">The url that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> Put(Url url, Action<BrowserContext> browserContext = null)
        {
            return this.HandleRequest("PUT", url, browserContext);
        }

        /// <summary>
        /// Performs a request of the HTTP <paramref name="method"/>, on the given <paramref name="url"/>, using the
        /// provided <paramref name="browserContext"/> configuration.
        /// </summary>
        /// <param name="method">HTTP method to send the request as.</param>
        /// <param name="url">The URl of the request.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public async Task<BrowserResponse> HandleRequest(string method, Url url, Action<BrowserContext> browserContext)
        {
            var browserContextValues =
                BuildBrowserContextValues(browserContext ?? (with => { }));

            var request =
                CreateRequest(method, url, browserContextValues);

            var context = await this.engine.HandleRequest(request).ConfigureAwait(false);

            var response = new BrowserResponse(context, this, (BrowserContext)browserContextValues);

            this.CaptureCookies(response);

            return response;
        }

        /// <summary>
        /// Performs a request of the HTTP <paramref name="method"/>, on the given <paramref name="path"/>, using the
        /// provided <paramref name="browserContext"/> configuration.
        /// </summary>
        /// <param name="method">HTTP method to send the request as.</param>
        /// <param name="path">The path that is being requested.</param>
        /// <param name="browserContext">An closure for providing browser context for the request.</param>
        /// <returns>An <see cref="BrowserResponse"/> instance of the executed request.</returns>
        public Task<BrowserResponse> HandleRequest(string method, string path, Action<BrowserContext> browserContext)
        {
            var url = Uri.IsWellFormedUriString(path, UriKind.Relative) ?
                new Url { Path = path } :
                (Url)new Uri(path);

            return this.HandleRequest(method, url, browserContext);
        }

        private static void DefaultBrowserContext(BrowserContext context)
        {
            context.HttpRequest();
        }

        private void SetCookies(BrowserContext context)
        {
            if (!this.cookies.Any())
            {
                return;
            }

            var cookieString = this.cookies.Aggregate(string.Empty, (current, cookie) => current + string.Format("{0}={1};", HttpUtility.UrlEncode(cookie.Key), HttpUtility.UrlEncode(cookie.Value)));

            context.Header("Cookie", cookieString);
        }

        private void CaptureCookies(BrowserResponse response)
        {
            if (response.Cookies == null || !response.Cookies.Any())
            {
                return;
            }

            foreach (var cookie in response.Cookies)
            {
                if (string.IsNullOrEmpty(cookie.Value))
                {
                    this.cookies.Remove(cookie.Name);
                }
                else
                {
                    this.cookies[cookie.Name] = cookie.Value;
                }
            }
        }

        private static void BuildRequestBody(IBrowserContextValues contextValues)
        {
            if (contextValues.Body != null)
            {
                return;
            }

            var useFormValues = !string.IsNullOrEmpty(contextValues.FormValues);
            var bodyContents = useFormValues ? contextValues.FormValues : contextValues.BodyString;
            var bodyBytes = bodyContents != null ? Encoding.UTF8.GetBytes(bodyContents) : ArrayCache.Empty<byte>();

            if (useFormValues && !contextValues.Headers.ContainsKey("Content-Type"))
            {
                contextValues.Headers["Content-Type"] = new[] { "application/x-www-form-urlencoded" };
            }

            contextValues.Body = new MemoryStream(bodyBytes);
        }

        private IBrowserContextValues BuildBrowserContextValues(Action<BrowserContext> browserContext)
        {
            var context =
                new BrowserContext(this.environment);

            this.SetCookies(context);

            this.defaultBrowserContext.Invoke(context);
            browserContext.Invoke(context);

            var contextValues =
                (IBrowserContextValues)context;

            if (!contextValues.Headers.ContainsKey("user-agent"))
            {
                contextValues.Headers.Add("user-agent", new[] { "Nancy.Testing.Browser" });
            }

            return contextValues;
        }

        private static Request CreateRequest(string method, Url url, IBrowserContextValues contextValues)
        {
            BuildRequestBody(contextValues);

            var requestStream =
                RequestStream.FromStream(contextValues.Body, 0, true);

            var certBytes = contextValues.ClientCertificate ?? null;

            var requestUrl = url;
            requestUrl.Scheme = string.IsNullOrWhiteSpace(contextValues.Protocol) ? requestUrl.Scheme : contextValues.Protocol;
            requestUrl.HostName = string.IsNullOrWhiteSpace(contextValues.HostName) ? requestUrl.HostName : contextValues.HostName;
            requestUrl.Query = string.IsNullOrWhiteSpace(url.Query) ? (contextValues.QueryString ?? string.Empty) : url.Query;

            return new Request(method, requestUrl, requestStream, contextValues.Headers, contextValues.UserHostAddress, certBytes);
        }
    }
}
