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
            if (request.Request.Intent.Name == "CountdownIntent" || 
                (request.Session.Attributes != null && request.Session.Attributes.Intent == "CountdownIntent"))
            {
                return new ChristmasIntentHandler().HandleRequest(request);
            }
            else if (request.Request.Intent.Name == "DraftListIntent" ||
                (request.Session.Attributes != null && request.Session.Attributes.Intent == "DraftListIntent"))
            {
                return new DraftListIntentHandler().HandleRequest(request);
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