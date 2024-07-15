#region Using Directives
using System.Collections.Generic;
#endregion

namespace CockatriceArtFinder.Scryfall.Models
{
    public class SearchResponse
    {
        public List<Card> Data { get; set; }
    }
}
