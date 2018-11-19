using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;

namespace Ember.Plugins
{
    /// <summary>
    /// The plug-in manager for Ember Media Manager.
    /// </summary>
    public class PluginManager
        : IDisposable
    {

        #region Events

        /// <summary>
        /// Occurs when a plugin wants to show a form on the UI thread.
        /// </summary>
        public event Events.ShowFormOnUIThreadHandler ShowFormOnUIThread;

        #endregion Events


        #region Fields

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<PluginManager.EmberPlugin> plugins = new List<PluginManager.EmberPlugin>();
        private Scraper.MovieScraperManager movieScraper;

        #endregion


        #region Properties

        /// <summary>
        /// Gets the loaded plug-ins.
        /// </summary>
        public List<PluginManager.EmberPlugin> Plugins
        {
            get { return plugins; }
        }

        /// <summary>
        /// Gets the Ember.Plugins settings.
        /// </summary>
        public Properties.Settings Settings
        {
            get { return Properties.Settings.Default; }
        }

        /// <summary>
        /// Gets the movie scraper.
        /// </summary>
        public Scraper.MovieScraperManager MovieScraper
        {
            get { return movieScraper; }
        }

        #endregion Properties


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// </summary>
        public PluginManager()
        {
            movieScraper = new Scraper.MovieScraperManager(this);
        }

        #endregion Constructor


        #region Methods

        /// <summary>
        /// Loads and initialise plug-ins listed in the configuration file.
        /// </summary>
        public void LoadPlugins()
        {
            // Plug-ins loaded by Ember.Plugins.PluginSectionHandler
            List<IPlugin> loaded = (List<IPlugin>)ConfigurationManager.GetSection("plugins");
            if (loaded == null || loaded.Count == 0)
                return;

            foreach (IPlugin plugin in loaded)
            {
                bool enabled = true;
                int order = 0;

                string pluginProp = plugin.AssemblyName.Replace('.', '_');
                try
                {
                    string prop = String.Format("Plugin__{0}__Enabled", pluginProp);
                    object value = Settings[prop];
                    enabled = Convert.ToBoolean(value);
                }
                catch (SettingsPropertyNotFoundException) { }
                try
                {
                    string prop = String.Format("Plugin__{0}__Order", pluginProp);
                    object value = Settings[prop];
                    order = Convert.ToInt32(value);
                }
                catch (SettingsPropertyNotFoundException) { }

                PluginManager.EmberPlugin ePlugin = new PluginManager.EmberPlugin(plugin, enabled, order);
                if (plugins.Contains(ePlugin))
                {
#if DEBUG
                    if (log.IsDebugEnabled)
                        log.DebugFormat(
                            "Load Plug-in :: Plugin already loaded. [{0}]",
                            plugin.GetType().Name);
#endif
                    continue;
                }

                plugins.Add(ePlugin);
                ePlugin.Plugin.InitPlugin(this);
            }

            plugins.Sort();
        }

        /// <summary>
        /// Show a form on the UI thread.
        /// </summary>
        /// <param name="plugin">The plugin making the call.</param>
        /// <param name="form">The form to show.</param>
        /// <param name="asDialog">if set to <c>true</c> as show as a dialog.</param>
        public void ShowForm(IPlugin plugin, Form form, bool asDialog)
        {
            if (ShowFormOnUIThread != null)
                ShowFormOnUIThread(plugin, new Events.ShowFormOnUIThreadEventArgs(form, asDialog));
        }

        #endregion


        #region IDisposable

        #region Fields

        private bool disposed = false;

        #endregion


        #region Properties

        /// <summary>
        /// Gets a value indicating whether the plug-in manager has been disposed of.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has been disposed of; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        #endregion Properties


        #region Methods

        /// <summary>
        /// Releases any resources used by the plug-in manager or the loaded plug-ins that implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposed = true;

                // Free other state (managed objects).
                movieScraper.Dispose();

                foreach (PluginManager.EmberPlugin plugin in plugins)
                {
                    if (plugin.Plugin is IDisposable)
                    {
                        IDisposable disposable = (IDisposable)plugin.Plugin;
                        disposable.Dispose();
                    }
                    plugins.Remove(plugin);

                    foreach (Delegate d in ShowFormOnUIThread.GetInvocationList())
                        ShowFormOnUIThread -= (Events.ShowFormOnUIThreadHandler)d;
                }
            }

            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PluginManager"/> is reclaimed by garbage collection.
        /// </summary>
        ~PluginManager()
        {
            Dispose(false);
        }

        #endregion Methods

        #endregion IDisposable


        #region Class PluginManager.EmberPlugin

        /// <summary>
        /// A container for a plug-in.
        /// </summary>
        public class EmberPlugin
            : IComparable<EmberPlugin>, IEquatable<EmberPlugin>
        {

            #region Fields

            private IPlugin plugin;
            private bool enabled;
            private int order;

            #endregion Fields

            #region Properties

            /// <summary>
            /// Gets the plug-in.
            /// </summary>
            public IPlugin Plugin
            {
                get { return plugin; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="EmberPlugin"/> is enabled.
            /// </summary>
            /// <value>
            ///   <c>true</c> if enabled; otherwise, <c>false</c>.
            /// </value>
            public bool Enabled
            {
                get { return enabled; }
                set { enabled = value; }
            }

            /// <summary>
            /// Gets or sets the order this plug-in is called.
            /// </summary>
            /// <value>
            /// The order this plug-in is called..
            /// </value>
            public int Order
            {
                get { return order; }
                set { order = value; }
            }

            #endregion Properties

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="EmberPlugin"/> class.
            /// </summary>
            /// <param name="plugin">The plug-in.</param>
            /// <param name="enabled">if set to <c>true</c> the plug-in is enabled.</param>
            /// <param name="order">The order this plug-in is called.</param>
            public EmberPlugin(IPlugin plugin, bool enabled, int order)
            {
                if (plugin == null)
                    throw new ArgumentNullException("plugin");

                this.plugin = plugin;
                this.enabled = enabled;
                this.order = order;
            }

            #endregion Constructor

            #region IComparable<EmberPlugin>

            /// <summary>
            /// Compares the current object with another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
            /// Value
            /// Meaning
            /// Less than zero
            /// This object is less than the <paramref name="other"/> parameter.
            /// Zero
            /// This object is equal to <paramref name="other"/>.
            /// Greater than zero
            /// This object is greater than <paramref name="other"/>.
            /// </returns>
            public int CompareTo(EmberPlugin other)
            {
                if (other == null)
                    return 0;

                return this.Order.CompareTo(other.Order);
            }

            #endregion IComparable<EmberPlugin>

            #region IEquatable<EmberPlugin>

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            public bool Equals(EmberPlugin other)
            {
                if (Object.ReferenceEquals(this, other))
                    return true;

                if (Object.ReferenceEquals(other, null))
                    return false;

                return this.Plugin.GetType() == other.Plugin.GetType();
            }

            #endregion IEquatable<EmberPlugin>

        }

        #endregion Class PluginManager.Plugin

    }
}
