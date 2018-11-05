namespace Ember.Plugins.Scraper
{

    /// <summary>
    /// The type of scrape to perform.
    /// </summary>
    public enum ScrapeType
    {
        /// <summary>
        /// A manual scraped.
        /// </summary>
        Manual,

        /// <summary>
        /// An automatic scrape.
        /// </summary>
        Automatic,
    }


    /// <summary>
    /// Context for scraper actions.
    /// </summary>
    public class ScraperActionContext
        : PluginActionContext
    {

        #region Fields

        private ScrapeType scrapeType;
        private bool askIfMultipleResults;

        #endregion Fields


        #region Properties

        /// <summary>
        /// Gets the type of scrape to perform.
        /// </summary>
        /// <value>
        /// The type of scrape to perform.
        /// </value>
        public ScrapeType ScrapeType
        {
            get { return scrapeType; }
        }

        /// <summary>
        /// Gets a value indicating whether to ask or select the best match if multiple results are returned.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user should be asked to select a movie if multiple results are found; otherwise, <c>false</c>.
        /// </value>
        public bool AskIfMultipleResults
        {
            get { return askIfMultipleResults; }
        }

        #endregion Properties


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ScraperActionContext"/> class.
        /// </summary>
        /// <param name="scrapeType">The type of scrape to perform.</param>
        /// <param name="askIfMultipleResults">if set to <c>true</c> ask the user to select a movie if multiple results are found.</param>
        public ScraperActionContext(ScrapeType scrapeType, bool askIfMultipleResults)
        {
            this.scrapeType = scrapeType;
            this.askIfMultipleResults = askIfMultipleResults;
        }

        #endregion Constructor

    }
}
