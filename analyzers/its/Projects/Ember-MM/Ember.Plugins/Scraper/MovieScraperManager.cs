using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ember.Plugins.Scraper
{
    /// <summary>
    /// The main interface for calling movie scraper plug-ins.
    /// </summary>
    public class MovieScraperManager
        : IDisposable, IMovieInfoScraper, IMovieImageScraper
    {

        #region Events

        /// <summary>
        /// Occurs before movie information is scraped.
        /// </summary>
        public event Events.PreMovieInfoScraperActionHandler PreMovieInfoScrape;

        /// <summary>
        /// Occurs after movie information is scraped.
        /// </summary>
        public event Events.PostMovieInfoScraperActionHandler PostMovieInfoScrape;

        /// <summary>
        /// Occurs before movie poster is scraped.
        /// </summary>
        public event Events.PreMovieImageScraperActionHandler PreMovieImageScrape;

        /// <summary>
        /// Occurs after movie poster is scraped.
        /// </summary>
        public event Events.PostMovieImageScraperActionHandler PostMovieImageScrape;

        #endregion Events


        #region Fields

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private PluginManager manager;

        #endregion Fields


        #region Constructor

        internal MovieScraperManager(PluginManager manager)
        {
            this.manager = manager;
        }

        #endregion Constructor


        #region IMovieInfoScraper

        /// <summary>
        /// Scrapes the movie info.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public PluginActionResult ScrapeMovieInfo(MovieInfoScraperActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (manager.Plugins.Count == 0)
                return new PluginActionResult();

            if (PreMovieInfoScrape != null)
                context = PreMovieInfoScrape(context);

            PluginActionResult result = null;

            foreach (IMovieInfoScraper plugin in manager.Plugins
                .Where(p => p.Enabled && p.Plugin is IMovieInfoScraper)
                .OrderBy(p => p.Order)
                .Select(p => p.Plugin))
            {
                result = plugin.ScrapeMovieInfo(context);
                if (result != null && result.BreakChain) break;
            }

            if (result == null)
                result = new PluginActionResult();

            if (PostMovieInfoScrape != null)
                result = PostMovieInfoScrape(result);

            return result;
        }

        #endregion IMovieInfoScraper


        #region IMovieImageScraper

        /// <summary>
        /// Scrapes the movie posters.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public PluginActionResult ScrapeMovieImage(MovieImageScraperActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (manager.Plugins.Count == 0)
                return new PluginActionResult();

            if (PreMovieImageScrape != null)
                context = PreMovieImageScrape(context);

            PluginActionResult result = null;

            foreach (IMovieImageScraper plugin in manager.Plugins
                .Where(p => p.Enabled && p.Plugin is IMovieImageScraper)
                .OrderBy(p => p.Order)
                .Select(p => p.Plugin))
            {
                result = plugin.ScrapeMovieImage(context);
                if (result != null && result.BreakChain) break;
            }

            if (result == null)
                result = new PluginActionResult();

            if (PostMovieImageScrape != null)
                result = PostMovieImageScrape(result);

            return result;
        }

        #endregion IMovieImageScraper

        
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
        /// Releases resources used by this object.
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
                foreach (Delegate d in PreMovieInfoScrape.GetInvocationList())
                    PreMovieInfoScrape -= (Events.PreMovieInfoScraperActionHandler)d;
                foreach (Delegate d in PostMovieInfoScrape.GetInvocationList())
                    PostMovieInfoScrape -= (Events.PostMovieInfoScraperActionHandler)d;
            }

            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PluginManager"/> is reclaimed by garbage collection.
        /// </summary>
        ~MovieScraperManager()
        {
            Dispose(false);
        }

        #endregion Methods

        #endregion IDisposable

    }
}
