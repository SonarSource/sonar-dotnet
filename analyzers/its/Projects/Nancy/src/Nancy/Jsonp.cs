﻿namespace Nancy
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Nancy.Bootstrapper;
    using Nancy.Configuration;
    using Nancy.Json;

    /// <summary>
    /// Handles JSONP requests.
    /// </summary>
    public static class Jsonp
    {
        private static readonly PipelineItem<Action<NancyContext>> JsonpItem;
        private static Encoding encoding;

        static Jsonp()
        {
            JsonpItem = new PipelineItem<Action<NancyContext>>("JSONP", PrepareJsonp);
        }

        private static string Encoding
        {
            get { return string.Concat("; charset=", encoding.WebName); }
        }

        /// <summary>
        /// Enable JSONP support in the application
        /// </summary>
        /// <param name="pipelines">Application Pipeline to Hook into</param>
        /// <param name="environment">An <see cref="INancyEnvironment"/> instance.</param>
        public static void Enable(IPipelines pipelines, INancyEnvironment environment)
        {
            var jsonpEnabled = pipelines.AfterRequest.PipelineItems.Any(ctx => ctx.Name == "JSONP");

            if (!jsonpEnabled)
            {
                encoding = environment.GetValue<JsonConfiguration>().DefaultEncoding;
                pipelines.AfterRequest.AddItemToEndOfPipeline(JsonpItem);
            }
        }

        /// <summary>
        /// Disable JSONP support in the application
        /// </summary>
        /// <param name="pipelines">Application Pipeline to Hook into</param>
        public static void Disable(IPipelines pipelines)
        {
            pipelines.AfterRequest.RemoveByName("JSONP");
        }

        /// <summary>
        /// Transmogrify original response and apply JSONP Padding
        /// </summary>
        /// <param name="context">Current Nancy Context</param>
        private static void PrepareJsonp(NancyContext context)
        {
            var isJson = Json.Json.IsJsonContentType(context.Response.ContentType);
            bool hasCallback = context.Request.Query["callback"].HasValue;

            if (!isJson || !hasCallback)
            {
                return;
            }

            // grab original contents for running later
            var original = context.Response.Contents;
            string callback = context.Request.Query["callback"].Value;

            // set content type to application/javascript so browsers can handle it by default
            // http://stackoverflow.com/questions/111302/best-content-type-to-serve-jsonp
            context.Response.ContentType = string.Concat("application/javascript", Encoding);

            context.Response.Contents = stream =>
            {
                // disposing of stream is handled elsewhere
                var writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                };

                writer.Write("{0}(", callback);
                original(stream);
                writer.Write(");");
            };
        }
    }
}
