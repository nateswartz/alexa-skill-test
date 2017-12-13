using AlexaSkill.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace AlexaSkill.Controllers
{
    public class AlexaController : Controller
    {
        [HttpPost]
        [Route("api/alexa/test")]
        public AlexaResponse HelloTest([FromBody]AlexaRequest request)
        {
            if (request.Request.Type == "LaunchRequest")
            {
                return new AlexaResponse("Welcome to Christmas Countdown, just say how many days until Christmas", false);
            }
            if (request.Request.Intent.Name == "CountdownIntent")
            {
                var text = $"There are {GetDaysTillChristmas()} days left until Christmas. {GetChristmasGreeting()}.  Are you excited?";
                var content = $"There are {GetDaysTillChristmas()} days left until Christmas.  {GetChristmasGreeting()}.";

                var response = new AlexaResponse(text, content);
                response.Response.Card.Title = "🎄 Christmas Countdown 🎄";
                response.Response.ShouldEndSession = false;

                return response;
            }
            else if (request.Request.Intent.Name == "AMAZON.NoIntent")
            {
                return new AlexaResponse("Fine, you don't have to be such a grinch.", true);
            }
            else if (request.Request.Intent.Name == "AMAZON.YesIntent")
            {
                return new AlexaResponse("Good, I'm glad you're in the Christmas spirit.", true);
            }
            return new AlexaResponse("I don't know how to help with that", true);
        }

        private int GetDaysTillChristmas()
        {
            var utcNow = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(utcNow, easternZone);

            DateTime nextChristmas = new DateTime(today.Year, 12, 25);

            if (nextChristmas < today)
                nextChristmas = nextChristmas.AddYears(1);

            return ((nextChristmas - today).Days + 1);
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
                "I hope you've been good this year"
            };

            var randomNumber = new Random(DateTime.Now.Millisecond).Next(greetings.Count);

            return greetings[randomNumber];
        }
    }
}