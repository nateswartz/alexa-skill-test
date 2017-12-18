using AlexaSkill.Models;
using Newtonsoft.Json;
using OpenScraping;
using OpenScraping.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace AlexaSkill.Handlers
{
    public class DraftListIntentHandler
    {
        private List<(string name, string speechId)> knownBars = new List<(string name, string speechId)>
        {
            ("The Friendly Greek", "friendly greek"), ("The Fridge", "fridge"), ("Hunger-n-Thirst", "and thirst")
        };

        public AlexaResponse HandleRequest(AlexaRequest request)
        {
            string result = "";
            string card = "";
            var bar = request.Request.Intent.GetSlots().Single(s => s.Key == "Bar").Value;
            if (!knownBars.Any(b => b.speechId.ToLower() == bar.ToLower()))
            {
                result = "I'm sorry, I don't know that bar.";
                card = result;
            }
            else
            {
                var results = GetDraftListFromUntappd(bar);
                result = $"Current draft list for {knownBars.First(b => b.speechId.ToLower() == bar.ToLower()).name} is {results.speechResult}";
                card = $"Current draft list for {knownBars.First(b => b.speechId.ToLower() == bar.ToLower()).name} is:\n{results.cardResult}";
            }
            return new AlexaResponse(result, card);
        }

        private (string speechResult, string cardResult) GetDraftListFromUntappd(string bar)
        {
            string url = "";
            switch (bar.ToLower())
            {
                case "friendly greek":
                    url = "v/friendly-greek-bottle-shop/110232";
                    break;
                case "fridge":
                    url = "v/the-fridge/89213";
                    break;
                case "and thirst":
                    url = "v/hunger-n-thirst/747436";
                    break;
            }

            var configJson = @"
            {
                'beers': '//div[contains(@class, \'beer-details\')]//a',
                'beerStyles': '//div[contains(@class, \'beer-details\')]//em'
            }
            ";

            var config = StructuredDataConfig.ParseJsonString(configJson);

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://untappd.com");
            var response = client.GetAsync(url).Result;
            var content = response.Content;
            var html = content.ReadAsStringAsync().Result;

            var scrapingResults = new StructuredDataExtractor(config).Extract(html);

            var beersResult = JsonConvert.SerializeObject(scrapingResults["beers"], Formatting.Indented);
            var stylesResult = JsonConvert.SerializeObject(scrapingResults["beerStyles"], Formatting.Indented);
            var unformattedBeers = Regex.Split(beersResult, @"\d+. ");
            var unformattedStyles = Regex.Split(stylesResult, "\r\n").ToList();
            unformattedStyles.RemoveAt(unformattedStyles.Count - 1);
            var formattedBeers = unformattedBeers
                                    .Skip(1)
                                    .Select(b => b.Remove(b.Length - 7).Trim().Replace("\",\r\n  \"", " by "))
                                    .ToList();
            var formattedStyles = unformattedStyles
                                    .Skip(1)
                                    .Select(s => s.Remove(s.Length - 2).Substring(3))
                                    .ToList();

            string speechResult = "";
            string cardResult = "";
            for (var i = 0; i < formattedBeers.Count; i++)
            {
                var beerPieces = formattedBeers[i].Split(" by ");
                string article = StartsWithVowel(formattedStyles[i]) ? "an" : "a";
                speechResult += beerPieces[0] + ", " + article + " " + formattedStyles[i] + ", by " + beerPieces[1] + ". ";
                cardResult += "* " + beerPieces[0] + ", " + article + " " + formattedStyles[i] + ", by " + beerPieces[1] + "\n";
            };
            return (speechResult, cardResult);
        }

        private bool StartsWithVowel(string input)
        {
            if (input.ToLower().StartsWith("a") ||
                input.ToLower().StartsWith("e") ||
                input.ToLower().StartsWith("i") ||
                input.ToLower().StartsWith("o") ||
                input.ToLower().StartsWith("u"))
            {
                return true;
            }
            return false;
        }
    }
}
