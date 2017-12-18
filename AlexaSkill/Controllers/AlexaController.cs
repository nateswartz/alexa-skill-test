using AlexaSkill.Handlers;
using AlexaSkill.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlexaSkill.Controllers
{
    public class AlexaController : Controller
    {

        [HttpPost]
        [Route("api/alexa/test")]
        public AlexaResponse AlexaEntrypoint([FromBody]AlexaRequest request)
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
                return new ChristmasIntentHandler().HandleRequest(request);
            }
            else if (request.Request.Intent.Name == "DraftListIntent")
            {
                return new DraftListIntentHandler().HandleRequest(request);
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
    }
}