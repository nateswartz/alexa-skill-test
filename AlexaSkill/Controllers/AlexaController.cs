using AlexaSkill.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlexaSkill.Controllers
{
    public class AlexaController : Controller
    {
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
                var shouldEnd = new Random(DateTime.Now.Millisecond).Next(4) != 1 ? true : false;

                var unit = request.Request.Intent.GetSlots().Single(s => s.Key == "Unit").Value;

                var text = $"There are {GetTimeTillChristmas(unit)} left until Christmas.  {GetChristmasGreeting()}.";
                var content = $"There are {GetTimeTillChristmas(unit)} left until Christmas.  {GetChristmasGreeting()}.";

                if (!shouldEnd)
                {
                    text += " Are you excited?";
                }

                var response = new AlexaResponse(text, content);
                response.Response.Card.Title = "🎄 Christmas Countdown 🎄";
                response.Response.ShouldEndSession = shouldEnd;

                return response;
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
                    value = Convert.ToInt32(ts.Seconds);
                    break;
                default:
                    value = Convert.ToInt32(ts.TotalDays) + 1;
                    unit = "days";
                    break;
            }

            return $"{String.Format("{0:n0}", value)} {unit}";
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
    }
}