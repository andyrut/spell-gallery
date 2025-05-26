#region Using Directives
using Newtonsoft.Json;
using SpellGallery.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

#endregion

namespace SpellGallery.Scryfall.Models
{
    /// <summary>
    /// An MTG card
    /// </summary>
    public class Card : ICloneable
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

        /// <summary>
        /// The card layout
        /// </summary>
        public string Layout { get; set; }

        /// <summary>
        /// The card faces for this card
        /// </summary>
        [JsonProperty("card_faces")]
        public List<CardFace> CardFaces { get; set; }

        /// <summary>
        /// The name of the back of the card, if there is one
        /// </summary>
        public string BackName { get; set; }

        /// <summary>
        /// The image URI object for the back face of the card, if there is one
        /// </summary>
        public ImageUris BackImageUris { get; set; }

        #region Public Methods
        /// <summary>
        /// Stores the card image(s) to the custom pics folder
        /// </summary>
        /// <param name="httpClient">The HTTP Client</param>
        /// <param name="settings">The Spell Gallery Settings</param>
        /// <returns>A task</returns>
        public async Task StoreAsync(HttpClient httpClient, SpellGallerySettings settings)
        {
            string frontName = Name.Contains("//") && CardFaces.Count > 1
                                ? CardFaces[0].Name
                                : Name;

            var imageBytes = await httpClient.GetByteArrayAsync(ImageUris.Large);
            var artPath = Path.Combine(settings.CustomPicsFolder, $"{frontName}.jpg");
            File.WriteAllBytes(artPath, imageBytes);

            if (string.IsNullOrEmpty(BackName))
                return;

            imageBytes = await httpClient.GetByteArrayAsync(BackImageUris.Large);
            artPath = Path.Combine(settings.CustomPicsFolder, $"{BackName}.jpg");
            File.WriteAllBytes(artPath, imageBytes);
        }
        #endregion  

        #region ICloneable
        /// <summary>
        /// Creates a clone which has handled the back side of transform cards
        /// </summary>
        /// <returns>A copy of the card</returns>
        public object Clone()
        {
            var card = (Card) MemberwiseClone();

            if (!string.Equals(Layout, "transform", StringComparison.OrdinalIgnoreCase))
                return card;
            
            // Set some useful properties on the card when it's a transform layout
            card.Name = card.CardFaces[0].Name;
            card.ImageUris = card.CardFaces[0].ImageUris;
            card.BackName = card.CardFaces[1].Name;
            card.BackImageUris = card.CardFaces[1].ImageUris;

            return card;
        }
        #endregion
    }
}