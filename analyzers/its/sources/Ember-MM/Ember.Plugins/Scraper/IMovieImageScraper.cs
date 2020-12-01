using EmberAPI;

namespace Ember.Plugins.Scraper
{
    /// <summary>
    /// Defines a movie image scraper.
    /// </summary>
    public interface IMovieImageScraper
    {

        /// <summary>
        /// Scrapes the movie posters.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        PluginActionResult ScrapeMovieImage(MovieImageScraperActionContext context);

    }

    /// <summary>
    /// Context for a movie image scraper action.
    /// </summary>
    public class MovieImageScraperActionContext
        : ImageScrapeActionContext
    {

        #region Fields

        private Structures.DBMovie dbMovie;

        #endregion Fields


        #region Properties

        /// <summary>
        /// Gets the movie.
        /// </summary>
        public Structures.DBMovie DBMovie
        {
            get { return dbMovie; }
        }

        #endregion Properties


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MovieImageScraperActionContext"/> class.
        /// </summary>
        /// <param name="dbMovie">The movie.</param>
        /// <param name="scrapeType">The type of scrape to perform.</param>
        /// <param name="askIfMultipleResults">if set to <c>true</c> ask the user to select a movie if multiple results are found.</param>
        public MovieImageScraperActionContext(
            Structures.DBMovie dbMovie,
            ImageScrapeType imageScrapeType,
            ScrapeType scrapeType,
            bool askIfMultipleResults)
            : base(imageScrapeType, scrapeType, askIfMultipleResults)
        {
            this.dbMovie = dbMovie;
        }

        #endregion Constructor

    }
}
