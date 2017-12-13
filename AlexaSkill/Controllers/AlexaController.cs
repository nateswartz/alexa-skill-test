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
        public AlexaResponse HelloTest(AlexaRequest request)
        {
            var text = $"There are {GetDaysTillChristmas()} days left until Christmas. {GetChristmasGreeting()}";
            var content = $"There are {GetDaysTillChristmas()} days left until Christmas.  {GetChristmasGreeting()}";

            var response = new AlexaResponse(text, content);
            response.Response.Card.Title = "Christmas Countdown 🎄";
            response.Response.ShouldEndSession = true;

            return response;
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
                "Ho, ho, ho",
                "Fa la la la la, la la, la, la"
            };

            return greetings[new Random(DateTime.Today.Millisecond).Next(greetings.Capacity)];
        }
    }
}