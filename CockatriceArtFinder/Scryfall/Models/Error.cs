#region Using Directives

using System.Collections.Generic;
using System.Linq;

#endregion

namespace MTGArtFinder.Scryfall.Models
{
    /// <summary>
    /// The error model from Scryfall
    /// </summary>
    public class Error
    {
        /// <summary>
        /// The error's details
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Warnings that may accompany the error
        /// </summary>
        public List<string> Warnings { get; set; }

        /// <summary>
        /// A formatted error message string from the details and warnings
        /// </summary>
        public string Message => $"{Details}{(Warnings != null && Warnings.Any() ? string.Join(" ", Warnings.ToArray()) : null)}";
    }
}