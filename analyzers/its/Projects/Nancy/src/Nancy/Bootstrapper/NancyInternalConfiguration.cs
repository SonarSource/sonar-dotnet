namespace Nancy.Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nancy.Configuration;
    using Nancy.Culture;
    using Nancy.Diagnostics;
    using Nancy.ErrorHandling;
    using Nancy.Localization;
    using Nancy.ModelBinding;
    using Nancy.Responses;
    using Nancy.Responses.Negotiation;
    using Nancy.Routing;
    using Nancy.Routing.Constraints;
    using Nancy.Routing.Trie;
    using Nancy.Security;
    using Nancy.Validation;
    using Nancy.ViewEngines;

    /// <summary>
    /// Configuration class for Nancy's internals.
    /// Contains implementation types/configuration for Nancy that usually
    /// do not require overriding in "general use".
    /// </summary>
    public sealed class NancyInternalConfiguration
    {
        /// <summary>
        /// Gets the Nancy default configuration
        /// </summary>
        public static Func<ITypeCatalog, NancyInternalConfiguration> Default
        {
            get
            {
                return typeCatalog => new NancyInternalConfiguration
                {
                    Binder = typeof(DefaultBinder),
                    BindingDefaults = typeof(BindingDefaults),
                    ContextFactory = typeof(DefaultNancyContextFactory),
                    CsrfTokenValidator = typeof(DefaultCsrfTokenValidator),
                    CultureService = typeof(DefaultCultureService),
                    DefaultConfigurationProviders = typeCatalog.GetTypesAssignableTo<INancyDefaultConfigurationProvider>().ToList(),
                    Diagnostics = typeof(DefaultDiagnostics),
                    EnvironmentFactory = typeof(DefaultNancyEnvironmentFactory),
                    EnvironmentConfigurator = typeof(DefaultNancyEnvironmentConfigurator),
                    FieldNameConverter = typeof(DefaultFieldNameConverter),
                    InteractiveDiagnosticProviders = new List<Type>(typeCatalog.GetTypesAssignableTo<IDiagnosticsProvider>()),
                    ModelBinderLocator = typeof(DefaultModelBinderLocator),
                    ModelValidatorLocator = typeof(DefaultValidatorLocator),
                    NancyEngine = typeof(NancyEngine),
                    NancyModuleBuilder = typeof(DefaultNancyModuleBuilder),
                    ObjectSerializer = typeof(DefaultObjectSerializer),
                    RenderContextFactory = typeof(DefaultRenderContextFactory),
                    RequestDispatcher = typeof(DefaultRequestDispatcher),
                    RequestTraceFactory = typeof(DefaultRequestTraceFactory),
                    RequestTracing = typeof(DefaultRequestTracing),
                    ResourceAssemblyProvider = typeof(ResourceAssemblyProvider),
                    ResourceReader = typeof(DefaultResourceReader),
                    ResponseFormatterFactory = typeof(DefaultResponseFormatterFactory),
                    ResponseNegotiator = typeof(DefaultResponseNegotiator),
                    ResponseProcessors = typeCatalog.GetTypesAssignableTo<IResponseProcessor>().ToList(),
                    RouteCache = typeof(RouteCache),
                    RouteCacheProvider = typeof(DefaultRouteCacheProvider),
                    RouteInvoker = typeof(DefaultRouteInvoker),
                    RouteResolver = typeof(DefaultRouteResolver),
                    RouteResolverTrie = typeof(RouteResolverTrie),
                    RouteSegmentConstraints = typeCatalog.GetTypesAssignableTo<IRouteSegmentConstraint>().ToList(),
                    RouteSegmentExtractor = typeof(DefaultRouteSegmentExtractor),
                    RouteMetadataProviders = typeCatalog.GetTypesAssignableTo<IRouteMetadataProvider>().ToList(),
                    RouteDescriptionProvider = typeof(DefaultRouteDescriptionProvider),
                    RuntimeEnvironmentInformation = typeof(DefaultRuntimeEnvironmentInformation),
                    SerializerFactory = typeof(DefaultSerializerFactory),
                    Serializers = typeCatalog.GetTypesAssignableTo<ISerializer>(TypeResolveStrategies.ExcludeNancy).Union(new List<Type>(new[] { typeof(DefaultJsonSerializer), typeof(DefaultXmlSerializer) })).ToList(),
                    StaticContentProvider = typeof(DefaultStaticContentProvider),
                    StatusCodeHandlers = new List<Type>(typeCatalog.GetTypesAssignableTo<IStatusCodeHandler>(TypeResolveStrategies.ExcludeNancy).Concat(new[] { typeof(DefaultStatusCodeHandler) })),
                    TextResource = typeof(ResourceBasedTextResource),
                    TrieNodeFactory = typeof(TrieNodeFactory),
                    ViewLocator = typeof(DefaultViewLocator),
                    ViewFactory = typeof(DefaultViewFactory),
                    ViewResolver = typeof(DefaultViewResolver),
                    ViewCache = typeof(DefaultViewCache),
                    ViewLocationProvider = typeof(FileSystemViewLocationProvider),
                };
            }
        }

        /// <summary>
        /// Gets or sets the runtime environment information
        /// </summary>
        public Type RuntimeEnvironmentInformation { get; set; }

        /// <summary>
        /// Gets or sets the serializer factory.
        /// </summary>
        public Type SerializerFactory { get; set; }

        /// <summary>
        /// Gets or sets the default configuration providers
        /// </summary>
        public IList<Type> DefaultConfigurationProviders { get; set; }

        /// <summary>
        /// Gets or sets the environment configurator
        /// </summary>
        public Type EnvironmentConfigurator { get; set; }

        /// <summary>
        ///Gets or sets the environment factory
        /// </summary>
        public Type EnvironmentFactory { get; set; }

        /// <summary>
        /// Gets or sets the route metadata providers
        /// </summary>
        public IList<Type> RouteMetadataProviders { get; set; }

        /// <summary>
        /// Gets or sets the route resolver
        /// </summary>
        public Type RouteResolver { get; set; }

        /// <summary>
        /// Gets or sets the context factory
        /// </summary>
        public Type ContextFactory { get; set; }

        /// <summary>
        /// Gets or sets the nancy engine
        /// </summary>
        public Type NancyEngine { get; set; }

        /// <summary>
        /// Gets or sets the route cache
        /// </summary>
        public Type RouteCache { get; set; }

        /// <summary>
        /// Gets or sets the route cache provider
        /// </summary>
        public Type RouteCacheProvider { get; set; }

        /// <summary>
        /// Gets or sets the view locator
        /// </summary>
        public Type ViewLocator { get; set; }

        /// <summary>
        /// Gets or sets the view factory
        /// </summary>
        public Type ViewFactory { get; set; }

        /// <summary>
        /// Gets or sets the nancy module builder
        /// </summary>
        public Type NancyModuleBuilder { get; set; }

        /// <summary>
        /// Gets or sets the response formatter factory
        /// </summary>
        public Type ResponseFormatterFactory { get; set; }

        /// <summary>
        /// Gets or sets themodel binder locator
        /// </summary>
        public Type ModelBinderLocator { get; set; }

        /// <summary>
        /// Gets or sets the binder
        /// </summary>
        public Type Binder { get; set; }

        /// <summary>
        /// Gets or sets the binding defaults
        /// </summary>
        public Type BindingDefaults { get; set; }

        /// <summary>
        /// Gets or sets the field name converter
        /// </summary>
        public Type FieldNameConverter { get; set; }

        /// <summary>
        /// Gets or sets the model validator locator
        /// </summary>
        public Type ModelValidatorLocator { get; set; }

        /// <summary>
        ///Gets or sets the view resolver
        /// </summary>
        public Type ViewResolver { get; set; }

        /// <summary>
        /// Gets or sets the view cache
        /// </summary>
        public Type ViewCache { get; set; }

        /// <summary>
        /// Gets or sets the render context factory
        /// </summary>
        public Type RenderContextFactory { get; set; }

        /// <summary>
        /// Gets or sets the view location provider
        /// </summary>
        public Type ViewLocationProvider { get; set; }

        /// <summary>
        /// Gets or sets the status code handlers
        /// </summary>
        public IList<Type> StatusCodeHandlers { get; set; }

        /// <summary>
        /// Gets or sets the CSRF token validator
        /// </summary>
        public Type CsrfTokenValidator { get; set; }

        /// <summary>
        /// Gets or sets the object serializer
        /// </summary>
        public Type ObjectSerializer { get; set; }

        /// <summary>
        /// Gets or sets the types for serializers
        /// </summary>
        public IList<Type> Serializers { get; set; }

        /// <summary>
        /// Gets or sets the interactive diagnostic providers
        /// </summary>
        public IList<Type> InteractiveDiagnosticProviders { get; set; }

        /// <summary>
        /// Gets or sets the request tracing
        /// </summary>
        public Type RequestTracing { get; set; }

        /// <summary>
        /// Gets or sets the route invoker
        /// </summary>
        public Type RouteInvoker { get; set; }

        /// <summary>
        /// Gets or sets the response processors
        /// </summary>
        public IList<Type> ResponseProcessors { get; set; }

        /// <summary>
        /// Gets or sets the request dispatcher
        /// </summary>
        public Type RequestDispatcher { get; set; }

        /// <summary>
        /// Gets or sets the diagnostics
        /// </summary>
        public Type Diagnostics { get; set; }

        /// <summary>
        /// Gets or sets the route segment extractor
        /// </summary>
        public Type RouteSegmentExtractor { get; set; }

        /// <summary>
        /// Gets or sets the route description provider
        /// </summary>
        public Type RouteDescriptionProvider { get; set; }

        /// <summary>
        /// Gets or sets the culture service
        /// </summary>
        public Type CultureService { get; set; }

        /// <summary>
        /// Gets or sets the text resource
        /// </summary>
        public Type TextResource { get; set; }

        /// <summary>
        /// Gets or sets the resource assembly provider
        /// </summary>
        public Type ResourceAssemblyProvider { get; set; }

        /// <summary>
        /// Gets or sets the resource reader
        /// </summary>
        public Type ResourceReader { get; set; }

        /// <summary>
        /// Gets or sets the static content provider
        /// </summary>
        public Type StaticContentProvider { get; set; }

        /// <summary>
        /// Gets or sets the route resolver trie
        /// </summary>
        public Type RouteResolverTrie { get; set; }

        /// <summary>
        /// Gets or sets the trie node factory
        /// </summary>
        public Type TrieNodeFactory { get; set; }

        /// <summary>
        /// Gets or sets the route segment constraints
        /// </summary>
        public IList<Type> RouteSegmentConstraints { get; set; }

        /// <summary>
        /// Gets or sets the request trace factory
        /// </summary>
        public Type RequestTraceFactory { get; set; }

        /// <summary>
        /// Gets or sets the response negotiator
        /// </summary>
        public Type ResponseNegotiator { get; set; }

        /// <summary>
        /// Gets a value indicating whether the configuration is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                try
                {
                    return this.GetTypeRegistrations().All(tr => tr.RegistrationType != null);
                }
                catch (ArgumentNullException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates a new nancy internal configuration initializer with overrides for default types.
        /// </summary>
        /// <param name="builder">Action that overrides default configuration types</param>
        /// <returns>Initializer with overriden default types</returns>
        public static Func<ITypeCatalog, NancyInternalConfiguration> WithOverrides(Action<NancyInternalConfiguration> builder)
        {
            return catalog =>
            {
                var configuration =
                    Default.Invoke(catalog);

                builder.Invoke(configuration);

                return configuration;
            };
        }

        /// <summary>
        /// Returns the configuration types as a TypeRegistration collection
        /// </summary>
        /// <returns>TypeRegistration collection representing the configuration types</returns>
        public IEnumerable<TypeRegistration> GetTypeRegistrations()
        {
            return new[]
            {
                new TypeRegistration(typeof(IRouteResolver), this.RouteResolver),
                new TypeRegistration(typeof(INancyEngine), this.NancyEngine),
                new TypeRegistration(typeof(IRouteCache), this.RouteCache),
                new TypeRegistration(typeof(IRouteCacheProvider), this.RouteCacheProvider),
                new TypeRegistration(typeof(IViewLocator), this.ViewLocator),
                new TypeRegistration(typeof(IViewFactory), this.ViewFactory),
                new TypeRegistration(typeof(INancyContextFactory), this.ContextFactory),
                new TypeRegistration(typeof(INancyModuleBuilder), this.NancyModuleBuilder),
                new TypeRegistration(typeof(IResponseFormatterFactory), this.ResponseFormatterFactory),
                new TypeRegistration(typeof(IModelBinderLocator), this.ModelBinderLocator),
                new TypeRegistration(typeof(IBinder), this.Binder),
                new TypeRegistration(typeof(BindingDefaults), this.BindingDefaults),
                new TypeRegistration(typeof(IFieldNameConverter), this.FieldNameConverter),
                new TypeRegistration(typeof(IViewResolver), this.ViewResolver),
                new TypeRegistration(typeof(IViewCache), this.ViewCache),
                new TypeRegistration(typeof(IRenderContextFactory), this.RenderContextFactory),
                new TypeRegistration(typeof(IViewLocationProvider), this.ViewLocationProvider),
                new TypeRegistration(typeof(ICsrfTokenValidator), this.CsrfTokenValidator),
                new TypeRegistration(typeof(IObjectSerializer), this.ObjectSerializer),
                new TypeRegistration(typeof(IModelValidatorLocator), this.ModelValidatorLocator),
                new TypeRegistration(typeof(IRequestTracing), this.RequestTracing),
                new TypeRegistration(typeof(IRouteInvoker), this.RouteInvoker),
                new TypeRegistration(typeof(IRequestDispatcher), this.RequestDispatcher),
                new TypeRegistration(typeof(IDiagnostics), this.Diagnostics),
                new TypeRegistration(typeof(IRouteSegmentExtractor), this.RouteSegmentExtractor),
                new TypeRegistration(typeof(IRouteDescriptionProvider), this.RouteDescriptionProvider),
                new TypeRegistration(typeof(ICultureService), this.CultureService),
                new TypeRegistration(typeof(ITextResource), this.TextResource),
                new TypeRegistration(typeof(IResourceAssemblyProvider), this.ResourceAssemblyProvider),
                new TypeRegistration(typeof(IResourceReader), this.ResourceReader),
                new TypeRegistration(typeof(IStaticContentProvider), this.StaticContentProvider),
                new TypeRegistration(typeof(IRouteResolverTrie), this.RouteResolverTrie),
                new TypeRegistration(typeof(ITrieNodeFactory), this.TrieNodeFactory),
                new TypeRegistration(typeof(IRequestTraceFactory), this.RequestTraceFactory),
                new TypeRegistration(typeof(IResponseNegotiator), this.ResponseNegotiator),
                new TypeRegistration(typeof(INancyEnvironmentConfigurator), this.EnvironmentConfigurator),
                new TypeRegistration(typeof(INancyEnvironmentFactory), this.EnvironmentFactory),
                new TypeRegistration(typeof(ISerializerFactory), this.SerializerFactory),
                new TypeRegistration(typeof(IRuntimeEnvironmentInformation), this.RuntimeEnvironmentInformation)
            };
        }

        /// <summary>
        /// Returns the collection configuration types as a CollectionTypeRegistration collection
        /// </summary>
        /// <returns>CollectionTypeRegistration collection representing the configuration types</returns>
        public IEnumerable<CollectionTypeRegistration> GetCollectionTypeRegistrations()
        {
            return new[]
            {
                new CollectionTypeRegistration(typeof(IResponseProcessor), this.ResponseProcessors),
                new CollectionTypeRegistration(typeof(ISerializer), this.Serializers),
                new CollectionTypeRegistration(typeof(IStatusCodeHandler), this.StatusCodeHandlers),
                new CollectionTypeRegistration(typeof(IDiagnosticsProvider), this.InteractiveDiagnosticProviders),
                new CollectionTypeRegistration(typeof(IRouteSegmentConstraint), this.RouteSegmentConstraints),
                new CollectionTypeRegistration(typeof(IRouteMetadataProvider), this.RouteMetadataProviders),
                new CollectionTypeRegistration(typeof(INancyDefaultConfigurationProvider), this.DefaultConfigurationProviders),
            };
        }
    }
}
