#region Using Directives

using Newtonsoft.Json;
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

        /// <summary>
        /// An optional URL pointing to the next page of output
        /// </summary>
        [JsonProperty("next_page")]
        public string NextPage { get; set; }
    }
}
