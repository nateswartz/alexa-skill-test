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
            if (request.Request.Intent.Name == "AMAZON.NoIntent")
            {
                return new AlexaResponse("Okay, goodbye");
            }

            string result = "";
            string card = "";
            (string speechResult, string cardResult, int numBeers, bool hasMore) results;
            int startingIndex = request.Session.Attributes != null ? request.Session.Attributes.LastListItem : 0;
            var bar = request.Session.Attributes == null ?request.Request.Intent.GetSlots().Single(s => s.Key == "Bar").Value : request.Session.Attributes.Bar;
            var formattedBar = knownBars.First(b => b.speechId.ToLower() == bar.ToLower()).name;
            if (!knownBars.Any(b => b.speechId.ToLower() == bar.ToLower()))
            {
                result = "I'm sorry, I don't know that bar.";
                card = result;
                return new AlexaResponse(result, card);
            }
            else
            {
                results = GetDraftListFromUntappd(bar, startingIndex);
                if (startingIndex == 0)
                {
                    result = $"Current draft list for {formattedBar} is {results.speechResult}";
                    card = $"Current draft list for {formattedBar} is:\n{results.cardResult}";
                }
                else
                {
                    result = results.speechResult;
                    card = results.cardResult;
                }

            }
            var response = new AlexaResponse(result, card);
            response.Session.Intent = "DraftListIntent";
            response.Session.Bar = bar;
            response.Session.LastListItem = (request.Session.Attributes != null ? request.Session.Attributes.LastListItem : 0) + results.numBeers;
            if (results.hasMore)
            {
                response.Response.OutputSpeech.Text += " Would you like to hear more?";
                response.Response.Card.Content += " Would you like to hear more?";
                response.Response.ShouldEndSession = false;
            }
            else
            {
                response.Response.ShouldEndSession = true;
            }
            return response;
        }

        private (string speechResult, string cardResult, int numBeers, bool hasMore) GetDraftListFromUntappd(string bar, int startingIndex = 0)
        {
            string url = "";
            int numBeers = 0;
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

            var beersString = JsonConvert.SerializeObject(scrapingResults["beers"], Formatting.Indented);
            var stylesString = JsonConvert.SerializeObject(scrapingResults["beerStyles"], Formatting.Indented);

            beersString = Regex.Replace(beersString, "\\[\r\n  \"\\d+. ", "").Replace("\"\r\n]", "");
            var beersList = Regex.Split(beersString, "\",\r\n  \"\\d+. ");

            stylesString = Regex.Replace(stylesString, "\\[\r\n  \"", "").Replace("\"\r\n]", "");
            stylesString = stylesString.Replace("IPA", "I.P.A.");
            var stylesList = Regex.Split(stylesString, "\",\r\n  \"");

            string speechResult = "";
            string cardResult = "";
            int endingIndex = beersList.Length > startingIndex + 5 ? startingIndex + 5 : beersList.Length;
            for (var i = startingIndex; i < endingIndex; i++)
            {
                var beerPieces = beersList[i].Split("\",\r\n  \"");
                string article = StartsWithVowel(stylesList[i]) ? "an" : "a";
                speechResult += beerPieces[0] + ", " + article + " " + stylesList[i] + ", by " + beerPieces[1] + ". ";
                cardResult += "* " + beerPieces[0] + ", " + article + " " + stylesList[i] + ", by " + beerPieces[1] + "\n";
                numBeers++;
            };
            return (speechResult, cardResult, numBeers, beersList.Length > endingIndex ? true : false);
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
