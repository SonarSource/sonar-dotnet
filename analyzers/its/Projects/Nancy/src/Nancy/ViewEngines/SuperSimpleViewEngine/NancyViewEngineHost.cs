namespace Nancy.ViewEngines.SuperSimpleViewEngine
{
    using System;

    /// <summary>
    /// Nancy view engine host
    /// </summary>
    /// <seealso cref="Nancy.ViewEngines.SuperSimpleViewEngine.IViewEngineHost" />
    public class NancyViewEngineHost : IViewEngineHost
    {
        private IRenderContext renderContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NancyViewEngineHost"/> class, with
        /// the provided <paramref name="renderContext"/>.
        /// </summary>
        /// <param name="renderContext">
        /// The render context.
        /// </param>
        public NancyViewEngineHost(IRenderContext renderContext)
        {
            this.renderContext = renderContext;
            this.Context = this.renderContext.Context;
        }

        /// <summary>
        /// Context object of the host application.
        /// </summary>
        /// <value>An instance of the context object from the host.</value>
        public object Context { get; private set; }

        /// <summary>
        /// Html "safe" encode a string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Encoded string</returns>
        public string HtmlEncode(string input)
        {
            return this.renderContext.HtmlEncode(input);
        }

        /// <summary>
        /// Get the contents of a template
        /// </summary>
        /// <param name="templateName">Name/location of the template</param>
        /// <param name="model">Model to use to locate the template via conventions</param>
        /// <returns>Contents of the template, or null if not found</returns>
        public string GetTemplate(string templateName, object model)
        {
            var viewLocationResult = this.renderContext.LocateView(templateName, model);

            if (viewLocationResult == null)
            {
                return "[ERR!]";
            }

            using(var reader = viewLocationResult.Contents.Invoke())
                return reader.ReadToEnd();
        }

        /// <summary>
        /// Gets a uri string for a named route
        /// </summary>
        /// <param name="name">Named route name</param>
        /// <param name="parameters">Parameters to use to expand the uri string</param>
        /// <returns>Expanded uri string, or null if not found</returns>
        public string GetUriString(string name, params string[] parameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Expands a path to include any base paths
        /// </summary>
        /// <param name="path">Path to expand</param>
        /// <returns>Expanded path</returns>
        public string ExpandPath(string path)
        {
            return this.renderContext.ParsePath(path);
        }

        /// <summary>
        /// Get the anti forgery token form element
        /// </summary>
        /// <returns>String containing the form element</returns>
        public string AntiForgeryToken()
        {
            var tokenKeyValue = this.renderContext.GetCsrfToken();

            return string.Format("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", tokenKeyValue.Key, tokenKeyValue.Value);
        }
    }
}