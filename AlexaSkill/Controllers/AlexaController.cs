using AlexaSkill.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenScraping;
using OpenScraping.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace AlexaSkill.Controllers
{
    public class AlexaController : Controller
    {
        private List<string> knownUnits = new List<string>
        {
            "days", "hours", "minutes", "seconds"
        };

        private List<(string name, string speechId)> knownBars = new List<(string name, string speechId)>
        {
            ("The Friendly Greek", "friendly greek"), ("The Fridge", "fridge"), ("Hunger-n-Thirst", "and thirst")
        };

        [HttpPost]
        [Route("api/alexa/test")]
        public AlexaResponse HelloTest([FromBody]AlexaRequest request)
        {
            return HandleRequest(request);
        }

        private AlexaResponse HandleRequest(AlexaRequest request)
        {
            if (request.Request.Type == "LaunchRequest")
            {
                return HandleLaunchRequest(request);
            }
            if (request.Request.Type == "IntentRequest")
            {
                return HandleIntentRequest(request);
            }
            return DefaultResponse();
        }

        private AlexaResponse HandleIntentRequest(AlexaRequest request)
        {
            if (request.Request.Intent.Name == "CountdownIntent")
            {
                return GetChristmasResponse(request);
            }
            else if (request.Request.Intent.Name == "DraftListIntent")
            {
                string result = "";
                string card = "";
                var bar = request.Request.Intent.GetSlots().Single(s => s.Key == "Bar").Value;
                if (!knownBars.Any( b => b.speechId.ToLower() == bar.ToLower()))
                {
                    result = "I'm sorry, I don't know that bar.";
                    card = result;
                }
                else
                {
                    var results = GetDraftListFromUntappd(bar);
                    result = $"Current draft list for {knownBars.First( b => b.speechId.ToLower() == bar.ToLower()).name} is {results.speechResult}";
                    card = $"Current draft list for {knownBars.First(b => b.speechId.ToLower() == bar.ToLower()).name} is:\n{results.cardResult}";
                }
                return new AlexaResponse(result, card);
            }
            else if (request.Request.Intent.Name == "AMAZON.NoIntent" && !request.Session.New)
            {
                return new AlexaResponse("Fine, you don't have to be such a grinch.", true);
            }
            else if (request.Request.Intent.Name == "AMAZON.YesIntent" && !request.Session.New)
            {
                return new AlexaResponse("Good, I'm glad you're in the Christmas spirit.", true);
            }
            return DefaultResponse();
        }

        private AlexaResponse HandleLaunchRequest(AlexaRequest request)
        {
            return new AlexaResponse("Welcome to Christmas Countdown, just say how many days until Christmas", false);
        }

        private AlexaResponse DefaultResponse()
        {
            return new AlexaResponse("I don't know how to help with that", true);
        }

        private AlexaResponse GetChristmasResponse(AlexaRequest request)
        {
            var unit = request.Request.Intent.GetSlots().Single(s => s.Key == "Unit").Value;
            var shouldEnd = new Random(DateTime.Now.Millisecond).Next(4) != 1 ? true : false;
            var text = GetChristmasResponseText(unit, shouldEnd);

            var response = new AlexaResponse(text, text);
            response.Response.Card.Title = "🎄 Christmas Countdown 🎄";
            response.Response.ShouldEndSession = shouldEnd;
            return response;
        }

        private string GetTimeTillChristmas(string unit)
        {
            var utcNow = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(utcNow, easternZone);

            DateTime nextChristmas = new DateTime(today.Year, 12, 25);

            if (nextChristmas < today)
                nextChristmas = nextChristmas.AddYears(1);

            var ts = nextChristmas.Subtract(today);
            int value = 0;
            switch (unit.ToLower())
            {
                case "hours":
                    value = Convert.ToInt32(ts.TotalHours);
                    break;
                case "minutes":
                    value = Convert.ToInt32(ts.TotalMinutes);
                    break;
                case "seconds":
                    value = Convert.ToInt32(ts.TotalSeconds);
                    break;
                case "days":
                    value = Convert.ToInt32(ts.TotalDays) + 1;
                    break;
            }

            return $"{String.Format("{0:n0}", value)} {unit}";
        }

        private string GetChristmasResponseText(string unit, bool shouldEnd)
        {
            string text = "";
            if (!knownUnits.Contains(unit))
            {
                text = $"I'm sorry, I don't know what a {unit} is.  I only know about days, hours, minutes, and seconds";
                shouldEnd = false;
            }
            else
            {
                text = $"There are {GetTimeTillChristmas(unit)} left until Christmas.  {GetChristmasGreeting()}.";
                if (!shouldEnd)
                {
                    text += " Are you excited?";
                }
            }

            return text;
        }

        private string GetChristmasGreeting()
        {
            var greetings = new List<string>
            {
                "I can't wait",
                "Deck the Halls",
                "Ho ho ho",
                "Fa la la la la, la la, la, la",
                "Just listen to those sleigh bells ring",
                "I hope you've been good this year",
                "Ugly Christmas sweaters for everyone",
                "It's the most wonderful time of the year"
            };

            var randomNumber = new Random(DateTime.Now.Millisecond).Next(greetings.Count);

            return greetings[randomNumber];
        }

        private (string speechResult, string cardResult) GetDraftListFromUntappd(string bar)
        {
            string url = "";
            switch (bar.ToLower()) {
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

            var openScraping = new StructuredDataExtractor(config);
            var scrapingResults = openScraping.Extract(html);

            var beersResult = JsonConvert.SerializeObject(scrapingResults["beers"], Formatting.Indented);
            var stylesResult = JsonConvert.SerializeObject(scrapingResults["beerStyles"], Formatting.Indented);
            var unformattedBeers = Regex.Split(beersResult, @"\d+. ");
            var unformattedStyles = Regex.Split(stylesResult, "\r\n");
            var unformattedStylesList = unformattedStyles.ToList();
            unformattedStylesList.RemoveAt(unformattedStylesList.Count - 1);
            var formattedBeers = unformattedBeers
                                    .Skip(1)
                                    .Select( b => b.Remove(b.Length - 7).Trim().Replace("\",\r\n  \"", " by "))
                                    .ToList();
            var formattedStyles = unformattedStylesList
                                    .Skip(1)
                                    .Select(s => s.Remove(s.Length - 2).Substring(3))
                                    .ToList();

            string speechResult = "";
            string cardResult = "";
            for (var i = 0; i < formattedBeers.Count; i++)
            {
                var beerPieces = formattedBeers[i].Split(" by ");
                string article = "a";
                if (formattedStyles[i].ToLower().StartsWith("a") ||
                    formattedStyles[i].ToLower().StartsWith("e") ||
                    formattedStyles[i].ToLower().StartsWith("i") ||
                    formattedStyles[i].ToLower().StartsWith("o") ||
                    formattedStyles[i].ToLower().StartsWith("u"))
                {
                    article = "an";
                }
                speechResult += beerPieces[0] + ", " + article + " " + formattedStyles[i] + ", by " + beerPieces[1] + ". ";
                cardResult += "* " + beerPieces[0] + ", " + article + " " + formattedStyles[i] + ", by " + beerPieces[1] + "\n";
            };
            return (speechResult, cardResult);
        }
    }
}