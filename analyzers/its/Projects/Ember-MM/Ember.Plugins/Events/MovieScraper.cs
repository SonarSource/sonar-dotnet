using Ember.Plugins.Scraper;

namespace Ember.Plugins.Events
{

    /// <summary>
    /// Delegate for PreMovieInfoScraperAction.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public delegate MovieInfoScraperActionContext
        PreMovieInfoScraperActionHandler(MovieInfoScraperActionContext context);

    /// <summary>
    /// Delegate for PostMovieInfoScraperAction.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns></returns>
    public delegate PluginActionResult
        PostMovieInfoScraperActionHandler(PluginActionResult result);

    /// <summary>
    /// Delegate for PreMovieImageScraperAction.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public delegate MovieImageScraperActionContext
        PreMovieImageScraperActionHandler(MovieImageScraperActionContext context);

    /// <summary>
    /// Delegate for PostMovieImageScraperAction.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns></returns>
    public delegate PluginActionResult
        PostMovieImageScraperActionHandler(PluginActionResult result);

}
