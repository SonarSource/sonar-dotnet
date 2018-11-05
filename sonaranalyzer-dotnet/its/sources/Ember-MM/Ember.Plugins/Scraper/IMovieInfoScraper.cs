using EmberAPI;

namespace Ember.Plugins.Scraper
{

    /// <summary>
    /// Defines a movie information scraper.
    /// </summary>
    public interface IMovieInfoScraper
    {

        /// <summary>
        /// Scrapes the movie info.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        PluginActionResult ScrapeMovieInfo(MovieInfoScraperActionContext context);

    }

    /// <summary>
    /// Context for a movie information scraper action.
    /// </summary>
    public class MovieInfoScraperActionContext
        : ScraperActionContext
    {

        #region Fields

        private Structures.DBMovie dbMovie;
        private Structures.ScrapeOptions options;

        #endregion Fields


        #region Properties

        /// <summary>
        /// Gets the movie.
        /// </summary>
        public Structures.DBMovie DBMovie
        {
            get { return dbMovie; }
        }

        /// <summary>
        /// Gets the global scraper options.
        /// </summary>
        public Structures.ScrapeOptions Options
        {
            get { return options; }
        }

        #endregion Properties


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MovieInfoScraperActionContext"/> class.
        /// </summary>
        /// <param name="dbMovie">The movie.</param>
        /// <param name="scrapeType">The type of scrape to perform.</param>
        /// <param name="askIfMultipleResults">if set to <c>true</c> ask the user to select a movie if multiple results are found.</param>
        /// <param name="options">The global scraper options.</param>
        public MovieInfoScraperActionContext(
            Structures.DBMovie dbMovie,
            ScrapeType scrapeType,
            bool askIfMultipleResults,
            Structures.ScrapeOptions options)
            : base(scrapeType, askIfMultipleResults)
        {
            this.dbMovie = dbMovie;
            this.options = options;
        }

        #endregion Constructor

    }
}
