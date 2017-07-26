using AlexaSkillsKit.Speechlet;
using AlexaSkillsKit.UI;
using System;
using System.Threading.Tasks;
using AlexaSkillsKit.Slu;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlexaSkill
{
    public class DotNetApplet : SpeechletAsync
    {
        private string GetSensorStateIntentName = "GetSensorState";
        /// <summary>
        /// Creates and returns the visual and spoken response with shouldEndSesion flag
        /// </summary>
        /// <param name="title">Title for the companion application home card</param>
        /// <param name="output">Output content for speech and companion application home card</param>
        /// <param name="shouldEndSession">Should the session be closed</param>
        /// <returns>SpeechletResponse spoken and visual response for the given input</returns>
        private SpeechletResponse BuildSpeechletResponse(string title, string output, bool shouldEndSession)
        {
            // Create the Simple card content
            var card = new SimpleCard
            {
                Title = $"{title}",
                Content = $"{output}"
            };

            //Create the plain text output
            var speech = new PlainTextOutputSpeech { Text = output };

            var response = new SpeechletResponse
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = speech,
                Card = card
            };

            return response;
        }

        private SpeechletResponse GetWelcomeResponse()
        {
            var output = "Welcome to the Hackathon Alexa app. Please request the action you want us to take.";
            return BuildSpeechletResponse("Welcome to .NET Core Smart Home", output, false);
        }

        private string GetUserIdFromSession(string AccessToken)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://graph.facebook.com/me?access_token=" + AccessToken);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            string userId;
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new SpeechletException("Could not login with the credentials.");
            }
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string result = streamReader.ReadToEnd();
                JObject json = JsonConvert.DeserializeObject<JObject>(result);
                userId = json.Value<string>("name");
                if (string.IsNullOrEmpty(userId))
                    throw new SpeechletException("Could not login with the credentials.");
            }
            httpResponse.Close();
            return userId;
        }

        public override Task<SpeechletResponse> OnIntentAsync(IntentRequest intentRequest, Session session)
        {
            if (string.IsNullOrEmpty(session.User.AccessToken))
                throw new SpeechletException("User must link account before accesing these types of commands.");

            var intent = intentRequest.Intent;
            var intentName = intent?.Name;

            string userId = GetUserIdFromSession(session.User.AccessToken);

            if (GetSensorStateIntentName.Equals(intentName))
            {
               
                return Task.FromResult(BuildSpeechletResponse("Get state of sensor", $"Hello {userId}, nice to see you here", true));
            }

            throw new SpeechletException("Invalid Intent");
        }

        public override Task<SpeechletResponse> OnLaunchAsync(LaunchRequest launchRequest, Session session)
        {
            return Task.FromResult(GetWelcomeResponse());
        }

        public override Task OnSessionEndedAsync(SessionEndedRequest sessionEndedRequest, Session session)
        {
            return Task.FromResult(0);
        }

        public override Task OnSessionStartedAsync(SessionStartedRequest sessionStartedRequest, Session session)
        {
            return Task.FromResult(0);
        }
    }
}