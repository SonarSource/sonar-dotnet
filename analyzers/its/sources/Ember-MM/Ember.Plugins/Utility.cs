using System;
using System.IO;
using System.Linq;

namespace Ember.Plugins
{
    /// <summary>
    /// Utility Methods
    /// </summary>
    public static class Utility
    {

        #region Static Methods

        /// <summary>
        /// Combines multiple path elements.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        /// <returns>The combined path.</returns>
        public static string CombinePath(params string[] paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            return paths.Aggregate((current, next) => Path.Combine(current, next));
        }

        /// <summary>
        /// Sanitises an IMDb ID.
        /// </summary>
        /// <param name="imdbId">The IMDb ID.</param>
        /// <returns>An IMDb ID in the proper format or an empty string if it's invalid.</returns>
        /// <remarks>This won't be needed in the future once the EmberAPI code has been cleaned up.</remarks>
        public static string SanitiseIMDbId(string imdbId)
        {
            if (string.IsNullOrEmpty(imdbId) || imdbId.Trim().Length == 0)
                return string.Empty;

            imdbId = imdbId.Trim();
            if ("tt".Equals(imdbId))
                imdbId = string.Empty;
            else
            {
                if (!imdbId.StartsWith("tt"))
                    imdbId = string.Concat("tt", imdbId);
                if (imdbId.Length != 9)
                    imdbId = string.Empty;
            }

            return imdbId;
        }

        #endregion Static Methods

    }
}
