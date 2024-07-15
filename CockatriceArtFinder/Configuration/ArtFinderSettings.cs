using System.Configuration;

namespace CockatriceArtFinder.Configuration
{
    public class ArtFinderSettings : ConfigurationSection
    {
        public const string SectionName = "artFinderSettings";

        /// <summary>
        /// The URL for the Scryfall API
        /// </summary>
        [ConfigurationProperty("scryfallApiUrl", IsRequired = true)]
        public string ScryfallApiUrl
        {
            get => (string)base["scryfallApiUrl"];
            set => base["scryfallApiUrl"] = value;
        }

        /// <summary>
        /// The path to the Cockatrice Custom Art directory
        /// </summary>
        [ConfigurationProperty("customArtDirectory", IsRequired = true)]
        public string CustomArtDirectory
        {
            get => (string)base["customArtDirectory"];
            set => base["customArtDirectory"] = value;
        }
    }
}
