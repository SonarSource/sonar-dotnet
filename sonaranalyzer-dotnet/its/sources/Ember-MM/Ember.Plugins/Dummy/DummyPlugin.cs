using Ember.Plugins.Scraper;

namespace Ember.Plugins.Dummy
{
    class DummyPlugin
        : PluginBase
    {

        #region Fields

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion


        #region IPlugin

        #region Properties

        /// <summary>
        /// The name of the plug-in.
        /// </summary>
        public override string Name
        {
            get { return "A Dummy Plug-in"; }
        }

        #endregion Properties


        #region Methods

        public override void InitPlugin(PluginManager manager)
        {
            base.InitPlugin(manager);

            manager.MovieScraper.PreMovieInfoScrape += PreMovieInfoScraperAction;
            manager.MovieScraper.PostMovieInfoScrape += PostMovieInfoScraperAction;
            manager.MovieScraper.PreMovieImageScrape += PreMovieImageScraperAction;
            manager.MovieScraper.PostMovieImageScrape += PostMovieImageScraperAction;
        }

        #endregion Methods

        #endregion IPlugin


        #region Event Handlers

        private MovieInfoScraperActionContext PreMovieInfoScraperAction(
            MovieInfoScraperActionContext context)
        {
            if (log.IsDebugEnabled)
                log.Debug("PreMovieInfoScraperAction[Dummy]");
            return context;
        }

        private PluginActionResult PostMovieInfoScraperAction(
            PluginActionResult result)
        {
            if (log.IsDebugEnabled)
                log.Debug("PostMovieInfoScraperAction[Dummy]");
            return result;
        }

        private MovieImageScraperActionContext PreMovieImageScraperAction(
            MovieImageScraperActionContext context)
        {
            if (log.IsDebugEnabled)
                log.Debug("PreMovieImageScraperAction[Dummy]");
            return context;
        }

        private PluginActionResult PostMovieImageScraperAction(
            PluginActionResult result)
        {
            if (log.IsDebugEnabled)
                log.Debug("PostMovieImageScraperAction[Dummy]");
            return result;
        }

        #endregion EventHandlers

    }
}
