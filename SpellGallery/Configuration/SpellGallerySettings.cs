#region Using Directives
using Newtonsoft.Json;
using System;
using System.IO;
#endregion

namespace SpellGallery.Configuration
{
    public class SpellGallerySettings : ICloneable
    {
        // The path to the program's settings file
        private static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SpellGallery", "Settings.json");

        #region Public Properties
        /// <summary>
        /// Path to the Cockatrice custom pics directory, e.g. C:\Users\Username\AppData\Local\Cockatrice\Cockatrice\pics\CUSTOM
        /// </summary>
        public string CustomPicsFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cockatrice", "Cockatrice", "pics", "CUSTOM");
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the program's settings from the user's app data
        /// </summary>
        /// <returns></returns>
        public static SpellGallerySettings Load()
        {
            if (!File.Exists(SettingsPath))
                return new SpellGallerySettings();

            string json = File.ReadAllText(SettingsPath);
            try
            {
                return JsonConvert.DeserializeObject<SpellGallerySettings>(json);
            }
            catch
            {
                return new SpellGallerySettings();
            }
        }

        /// <summary>
        /// Saves the settings to the settings file
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void Save()
        {
            string settingsDir = Path.GetDirectoryName(SettingsPath)
                                 ?? throw new DirectoryNotFoundException($"Could not determine directory of path [{SettingsPath}]");
            Directory.CreateDirectory(settingsDir);
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }
        #endregion

        #region ICloneable
        /// <summary>
        /// Performs a memberwise clone of the settings
        /// </summary>
        /// <returns>A cloned instance of the settings</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}