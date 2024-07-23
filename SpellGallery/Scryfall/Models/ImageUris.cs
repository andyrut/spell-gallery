namespace SpellGallery.Scryfall.Models
{
    /// <summary>
    /// Collection of image URIs for a given card
    /// </summary>
    public class ImageUris
    {
        /// <summary>
        /// URI to the small version of the card image
        /// </summary>
        public string Small { get; set; }

        /// <summary>
        /// URI to the large version of the card image
        /// </summary>
        public string Large { get; set; }
    }
}