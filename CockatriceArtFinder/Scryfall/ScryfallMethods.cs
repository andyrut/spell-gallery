#region Using Directives

using System;
using CockatriceArtFinder.Scryfall.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

#endregion

namespace CockatriceArtFinder.Scryfall
{
    public class ScryfallMethods
    {
        public string ApiUrl { get; set; }

        private HttpClient httpClient = new HttpClient();

        public ScryfallMethods(string apiUrl)
        {
            ApiUrl = apiUrl;
        }

        public List<Card> GetCardsByName(string cardName)
        {
            string url = $"{ApiUrl}/cards/search?unique=prints&q={Uri.EscapeUriString(cardName)}";
            var response = httpClient.GetAsync(url).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
                throw new WebException($"Error searching for [{cardName}]: {(int) response.StatusCode} - {responseString}");
            var searchResponse = JsonConvert.DeserializeObject<SearchResponse>(responseString);
            if (searchResponse == null)
                throw new WebException("Scryfall search returned invalid response");
            return searchResponse.Data;
        }
    }
}