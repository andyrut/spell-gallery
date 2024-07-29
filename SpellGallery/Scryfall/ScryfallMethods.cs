#region Using Directives
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpellGallery.Scryfall.Models;
#endregion

namespace SpellGallery.Scryfall
{
    /// <summary>
    /// Collection of methods for accessing the Scryfall API
    /// </summary>
    public static class ScryfallMethods
    {
        // The URL of the Scryfall API
        private const string ApiUrl = "https://api.scryfall.com";

        // Reusable HTTP Client
        private static readonly HttpClient HttpClient = new HttpClient();

        static ScryfallMethods()
        {
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.Add("User-Agent", $"SpellGallery/{Assembly.GetCallingAssembly().GetName().Version}");
        }

        /// <summary>
        /// Gets all unique prints of a specific card
        /// </summary>
        /// <param name="cardName">The card to search for</param>
        /// <returns>List of cards</returns>
        public static async Task<List<Card>> GetCardsByNameAsync(string cardName)
        {
            string endpoint = $"/cards/search?unique=prints&q={Uri.EscapeUriString(cardName)}";
            var searchResponse = await GetAsync<SearchResponse>(endpoint);
            return searchResponse.Data;
        }

        /// <summary>
        /// Gets suggestions for card names from a filter
        /// </summary>
        /// <param name="filter">The filter criteria</param>
        /// <returns>List of suggestions that meet the filter criteria</returns>
        public static async Task<List<string>> AutoCompleteAsync(string filter)
        {
            string endpoint = $"/cards/autocomplete?q={Uri.EscapeUriString(filter)}";
            var autoCompleteResponse = await GetAsync<AutoCompleteResponse>(endpoint);
            return autoCompleteResponse.Data;
        }

        // GET REST operation helper
        private static async Task<T> GetAsync<T>(string endpoint)
        {
            string url = $"{ApiUrl}{endpoint}";
            var response = await HttpClient.GetAsync(url);
            string responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage;
                try
                {
                    var error = JsonConvert.DeserializeObject<Error>(responseString);
                    errorMessage = error?.Message ?? responseString;
                }
                catch
                {
                    errorMessage = responseString;
                }

                throw new WebException($"{(int)response.StatusCode} Error accessing Scrfall URL [{url}]:{Environment.NewLine}{Environment.NewLine}{errorMessage}");
            }

            var apiResponse = JsonConvert.DeserializeObject<T>(responseString);
            if (apiResponse == null)
                throw new WebException("Scryfall returned invalid response");
            
            return apiResponse;
        }
    }
}