namespace Ember.Plugins
{
    /// <summary>
    /// A basic implementation of the plug-in interface.
    /// </summary>
    public abstract class PluginBase
        : IPlugin
    {

        #region IPlugin

        #region Fields

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private PluginManager manager;

        #endregion Fields


        #region Properties

        /// <summary>
        /// The name of the plug-in.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// The assembly name of the plug-in.
        /// </summary>
        public string AssemblyName
        {
            get { return GetType().Assembly.GetName().Name; }
        }

        /// <summary>
        /// The plug-in version.
        /// </summary>
        public string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        /// <summary>
        /// The plug-in manager.
        /// </summary>
        public PluginManager PluginManager
        {
            get { return manager; }
        }

        #endregion Properties


        #region Methods

        /// <summary>
        /// Initialises the plugin.
        /// </summary>
        /// <param name="manager">The plugin manager.</param>
        public virtual void InitPlugin(PluginManager manager)
        {
            this.manager = manager;

#if DEBUG
            if (log.IsDebugEnabled)
                log.Debug(string.Format(
                    "InitPlugin :: Assembly = {0}; Name = {1}; Version = {2}",
                    AssemblyName, Name, Version));
#endif
        }

        #endregion Methods

        #endregion IPlugin

    }
}
