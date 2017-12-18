using AlexaSkill.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlexaSkill.Handlers
{
    public class ChristmasIntentHandler
    {
        private List<string> knownUnits = new List<string>
        {
            "days", "hours", "minutes", "seconds"
        };

        public AlexaResponse HandleRequest(AlexaRequest request)
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
    }
}
