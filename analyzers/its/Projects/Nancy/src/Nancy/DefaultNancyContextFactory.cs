namespace Nancy
{
    using Nancy.Configuration;
    using Nancy.Culture;
    using Nancy.Diagnostics;
    using Nancy.Localization;

    /// <summary>
    /// Creates NancyContext instances
    /// </summary>
    public class DefaultNancyContextFactory : INancyContextFactory
    {
        private readonly ICultureService cultureService;
        private readonly IRequestTraceFactory requestTraceFactory;
        private readonly ITextResource textResource;
        private readonly INancyEnvironment environment;

        /// <summary>
        /// Creates a new instance of the <see cref="DefaultNancyContextFactory"/> class.
        /// </summary>
        /// <param name="cultureService">An <see cref="ICultureService"/> instance.</param>
        /// <param name="requestTraceFactory">An <see cref="IRequestTraceFactory"/> instance.</param>
        /// <param name="textResource">An <see cref="ITextResource"/> instance.</param>
        /// <param name="environment">An <see cref="INancyEnvironment"/> instance.</param>
        public DefaultNancyContextFactory(ICultureService cultureService, IRequestTraceFactory requestTraceFactory, ITextResource textResource, INancyEnvironment environment)
        {
            this.cultureService = cultureService;
            this.requestTraceFactory = requestTraceFactory;
            this.textResource = textResource;
            this.environment = environment;
        }

        /// <summary>
        /// Create a new <see cref="NancyContext"/> instance.
        /// </summary>
        /// <returns>A <see cref="NancyContext"/> instance.</returns>
        public NancyContext Create(Request request)
        {
            var context =
                new NancyContext();

            context.Trace = this.requestTraceFactory.Create(request);
            context.Request = request;
            context.Culture = this.cultureService.DetermineCurrentCulture(context);
            context.Text = new TextResourceFinder(this.textResource, context);
            context.Environment = this.environment;

            // Move this to DefaultRequestTrace.
            context.Trace.TraceLog.WriteLog(s => s.AppendLine("New Request Started"));

            return context;
        }
    }
}
