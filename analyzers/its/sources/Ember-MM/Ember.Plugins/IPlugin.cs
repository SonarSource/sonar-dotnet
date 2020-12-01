namespace Ember.Plugins
{
    /// <summary>
    /// The Ember Media Manager plug-in interface.
    /// </summary>
    public interface IPlugin
    {

        #region Properties

        /// <summary>
        /// The name of the plug-in.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The assembly name of the plug-in.
        /// </summary>
        string AssemblyName { get; }

        /// <summary>
        /// The plug-in version.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// The plug-in manager.
        /// </summary>
        PluginManager PluginManager { get; }

        #endregion Properties


        #region Methods

        /// <summary>
        /// Initialises the plugin.
        /// </summary>
        /// <param name="manager">The plugin manager.</param>
        void InitPlugin(PluginManager manager);

        #endregion Methods

    }
}
