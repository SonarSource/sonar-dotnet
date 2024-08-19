using System.Text.RegularExpressions;
using Ember.Plugins.Scraper;

namespace Ember.Plugins.Ember
{
    class EmberWorkAroundPlugin
        : PluginBase
    {

        #region Fields

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Regex yearRegex;

        #endregion


        #region IPlugin

        #region Properties

        /// <summary>
        /// The name of the plug-in.
        /// </summary>
        public override string Name
        {
            get { return "Ember Work Around Plug-in"; }
        }

        #endregion Properties


        #region Methods

        public override void InitPlugin(PluginManager manager)
        {
            base.InitPlugin(manager);

            manager.MovieScraper.PreMovieInfoScrape += PreMovieInfoScraperAction;
        }

        #endregion Methods

        #endregion IPlugin


        #region Event Handlers

        /// <summary>
        /// Fixes the title and year before scraping movie info.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private MovieInfoScraperActionContext PreMovieInfoScraperAction(
            MovieInfoScraperActionContext context)
        {
            // Check for titles that end with the, a or an that should be at the
            // start of the title.
            string title = context.DBMovie.Movie.Title;
            string lTitle = title.ToLower();
            if (lTitle.EndsWith(", the"))
                title = string.Format("The {0}", title.Substring(0, title.LastIndexOf(',')));
            else if (lTitle.EndsWith(", a"))
                title = string.Format("A {0}", title.Substring(0, title.LastIndexOf(',')));
            else if (lTitle.EndsWith(", an"))
                title = string.Format("An {0}", title.Substring(0, title.LastIndexOf(',')));
            context.DBMovie.Movie.Title = title;

            // if year is unset try and scrape it from the file name.
            if (string.IsNullOrEmpty(context.DBMovie.Movie.Year))
            {
                if (yearRegex == null)
                    yearRegex = new Regex(@"[\(\[](19|20[0-9]{2})[\]\)]");
                string filename = context.DBMovie.Filename;
                Match match = yearRegex.Match(filename);
                if (match.Success)
                    context.DBMovie.Movie.Year = match.Groups[1].Value;
            }

            return context;
        }

        #endregion EventHandlers

    }
}
