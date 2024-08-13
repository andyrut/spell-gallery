#region Using Directives
using Newtonsoft.Json;
#endregion

namespace SpellGallery.Scryfall.Models
{
    /// <summary>
    /// The face of a card
    /// </summary>
    public class CardFace
    {
        /// <summary>
        /// The name of the card face
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The image URIs associated with this card face
        /// </summary>
        [JsonProperty("image_uris")]
        public ImageUris ImageUris { get; set; }
    }
}