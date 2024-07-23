#region Using Directives

using System.Collections.Generic;

#endregion

namespace MTGArtFinder.Scryfall.Models
{
    /// <summary>
    /// The response object for /cards/autocomplete
    /// </summary>
    public class AutoCompleteResponse
    {
        /// <summary>
        /// The list of suggestions for the given filter
        /// </summary>
        public List<string> Data { get; set; }
    }
}