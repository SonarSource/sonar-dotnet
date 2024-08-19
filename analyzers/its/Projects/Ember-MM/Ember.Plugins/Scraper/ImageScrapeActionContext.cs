namespace Ember.Plugins.Scraper
{

    /// <summary>
    /// The type of image scrape to perform.
    /// </summary>
    public enum ImageScrapeType
    {
        /// <summary>
        /// Scrape a poster.
        /// </summary>
        Poster,

        /// <summary>
        /// Scrape fanart.
        /// </summary>
        Fanart,
    }


    /// <summary>
    /// Context for image scraper actions.
    /// </summary>
    public class ImageScrapeActionContext
        : ScraperActionContext
    {

        #region Fields

        private ImageScrapeType imageScrapeType;

        #endregion Fields


        #region Properties

        /// <summary>
        /// Gets the type of image scrape to perform.
        /// </summary>
        /// <value>
        /// The type of image scrape to perform.
        /// </value>
        public ImageScrapeType ImageScrapeType
        {
            get { return imageScrapeType; }
        }

        #endregion Properties


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageScrapeActionContext"/> class.
        /// </summary>
        /// <param name="imageScrapeType">Type of the image scrape.</param>
        /// <param name="scrapeType">Type of the scrape.</param>
        /// <param name="askIfMultipleResults">if set to <c>true</c> [ask if multiple results].</param>
        public ImageScrapeActionContext(
            ImageScrapeType imageScrapeType,
            ScrapeType scrapeType,
            bool askIfMultipleResults)
            : base(scrapeType, askIfMultipleResults)
        {
            this.imageScrapeType = imageScrapeType;
        }

        #endregion Constructor

    }

}
