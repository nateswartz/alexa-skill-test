using Microsoft.AspNetCore.Mvc;

namespace AlexaSkill.Controllers
{
    public class AlexaController : Controller
    {
        [HttpPost]
        [Route("api/alexa/test")]
        public dynamic HelloTest(dynamic request)
        {
            return new
            {
                version = "1.0",
                sessionAttributes = new { },
                response = new
                {
                    outputSpeech = new
                    {
                        type = "PlainText",
                        text = "Hello Swartzentrubers"
                    },
                    card = new
                    {
                        type = "Simple",
                        title = "Hello World Test",
                        content = "Hello\nSwartzentrubers!"
                    },
                    shouldEndSession = true
                }
            };
        }
    }
}