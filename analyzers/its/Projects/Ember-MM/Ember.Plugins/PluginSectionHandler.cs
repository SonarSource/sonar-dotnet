using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace Ember.Plugins
{
    /// <summary>
    /// A class to parse plugins section from Application Config
    /// </summary>
    public class PluginSectionHandler
        : IConfigurationSectionHandler
    {

        #region Fields

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSectionHandler"/> class.
        /// </summary>
        public PluginSectionHandler()
        {
        }

        #endregion


        #region IConfigurationSectionHandler

        /// <summary>
        /// Creates a configuration section handler.
        /// </summary>
        /// <param name="parent">Parent object.</param>
        /// <param name="configContext">Configuration context object.</param>
        /// <param name="section">Section XML node.</param>
        /// <returns>
        /// The created section handler object.
        /// </returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            List<IPlugin> plugins = new List<IPlugin>();

            if (!Properties.Settings.Default.AllowPlugins)
            {
#if DEBUG
                if (log.IsDebugEnabled) log.Debug("Load Plug-in :: Plug-ins disabled.");
#endif
                return plugins;
            }

            foreach (XmlNode node in section.ChildNodes)
            {
                string className = null;
                try
                {
                    if (node.Attributes["class"] == null)
                        throw new PluginSectionHandler.InvalidConfigException();
                    className = node.Attributes["class"].Value;
                    if (string.IsNullOrEmpty(className)) continue;

#if DEBUG
                    if (log.IsDebugEnabled)
                        log.Debug(string.Format("Load Plug-in :: ClassName = {0}", className));
#endif

                    Type pluginType = Type.GetType(className);
                    if (pluginType == null)
                        throw new PluginSectionHandler.UnknownClassException();
                    object pluginObject = Activator.CreateInstance(pluginType);
                    if (pluginObject == null || !(pluginObject is IPlugin)) continue;

                    IPlugin plugin = (IPlugin)pluginObject;
                    plugins.Add(plugin);
                }
                catch (PluginSectionHandler.InvalidConfigException)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Load Plug-in :: Plug-in configuration error. No class type found for plug-in.");
                }
                catch (PluginSectionHandler.UnknownClassException)
                {
                    if (log.IsErrorEnabled)
                    {
                        string[] classSplit = className.Split(',');
                        log.ErrorFormat(
                            "Load Plug-in :: Plug-in configuration error. Unable to load plugin class. [Assembly={1}; Class={0}]",
                            classSplit[0].TrimStart(), classSplit[1]);
                        log.Error("Load Plug-in :: Check the plug-in entry and that it's located in the Modules directory.");
                    }
                }
                catch (Exception ex)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Load Plug-in :: Plug-in configuration error.", ex);
                }
            }

            return plugins;
        }

        #endregion


        #region PluginSectionHandler Exceptions

        private class InvalidConfigException : Exception { }
        private class UnknownClassException : Exception { }

        #endregion PluginSectionHandler Exceptions
    }
}
