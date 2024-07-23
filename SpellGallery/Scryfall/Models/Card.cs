using Newtonsoft.Json;

namespace SpellGallery.Scryfall.Models
{
    /// <summary>
    /// An MTG card
    /// </summary>
    public class Card
    {
        /// <summary>
        /// The name of the card
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The image URI object for the card
        /// </summary>
        [JsonProperty("image_uris")]
        public ImageUris ImageUris { get; set; }
    }
}