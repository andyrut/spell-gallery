#region Using Directives

using System.Collections;
using AutoCompleteTextBox.Editors;
using SpellGallery.Scryfall;

#endregion

namespace SpellGallery.AutoComplete
{
    /// <summary>
    /// Auto-Complete Provider for MTG Cards
    /// </summary>
    public class CardsProvider : ISuggestionProvider
    {
        #region Public Methods
        /// <summary>
        /// Gets the auto-complete suggestions from the entered filter
        /// </summary>
        /// <param name="filter">The entered filter</param>
        /// <returns>List of suggestions</returns>
        public IEnumerable GetSuggestions(string filter)
        {
            if (filter == null || filter.Length < 2)
                return new string[] {};

            return ScryfallMethods.AutoCompleteAsync(filter).Result;
        }
        #endregion
    }
}