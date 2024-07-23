#region Using Directives

using System.Collections.Generic;

#endregion

namespace SpellGallery.Scryfall.Models
{
    /// <summary>
    /// The response model for /cards/search
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        /// A list of cards returned from a card search
        /// </summary>
        public List<Card> Data { get; set; }
    }
}
