﻿namespace Nancy.Responses
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Nancy.Configuration;
    using Nancy.IO;
    using Nancy.Json;
    using Nancy.Responses.Negotiation;

    /// <summary>
    /// Default <see cref="ISerializer"/> implementation for JSON serialization.
    /// </summary>
    public class DefaultJsonSerializer : ISerializer
    {
        private readonly JsonConfiguration jsonConfiguration;
        private readonly TraceConfiguration traceConfiguration;
        private readonly GlobalizationConfiguration globalizationConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultJsonSerializer"/> class,
        /// with the provided <see cref="INancyEnvironment"/>.
        /// </summary>
        /// <param name="environment">An <see cref="INancyEnvironment"/> instance.</param>
        public DefaultJsonSerializer(INancyEnvironment environment)
        {
            this.jsonConfiguration = environment.GetValue<JsonConfiguration>();
            this.traceConfiguration = environment.GetValue<TraceConfiguration>();
            this.globalizationConfiguration = environment.GetValue<GlobalizationConfiguration>();
        }

        /// <summary>
        /// Whether the serializer can serialize the content type
        /// </summary>
        /// <param name="mediaRange">Content type to serialise</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool CanSerialize(MediaRange mediaRange)
        {
            return Json.IsJsonContentType(mediaRange);
        }

        /// <summary>
        /// Gets the list of extensions that the serializer can handle.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of extensions if any are available, otherwise an empty enumerable.</value>
        public IEnumerable<string> Extensions
        {
            get { yield return "json"; }
        }

        /// <summary>
        /// Serialize the given model with the given contentType
        /// </summary>
        /// <param name="mediaRange">Content type to serialize into</param>
        /// <param name="model">Model to serialize</param>
        /// <param name="outputStream">Stream to serialize to</param>
        /// <returns>Serialised object</returns>
        public void Serialize<TModel>(MediaRange mediaRange, TModel model, Stream outputStream)
        {
            using (var writer = new StreamWriter(new UnclosableStreamWrapper(outputStream)))
            {
                var serializer = new JavaScriptSerializer(this.jsonConfiguration, this.globalizationConfiguration);

                serializer.RegisterConverters(this.jsonConfiguration.Converters,
                    this.jsonConfiguration.PrimitiveConverters);

                try
                {
                    serializer.Serialize(model, writer);
                }
                catch (Exception exception)
                {
                    if (this.traceConfiguration.DisplayErrorTraces)
                    {
                        writer.Write(exception.Message);
                    }
                }
            }
        }
    }
}
