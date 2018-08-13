using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using LuisBot.Dialogs;

namespace Microsoft.Bot.Sample.LuisBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message

            if (activity.GetActivityType() == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity isTypingReply = activity.CreateReply();
                isTypingReply.Type = ActivityTypes.Typing;
                await connector.Conversations.ReplyToActivityAsync(isTypingReply);
                await Conversation.SendAsync(activity, () => new BasicLuisDialog());
                //PromptDialog.Text(activity, AfterAnyHelp, "Is there anything else I can help you with?");

            }
            else
            {
                await HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                //ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                //Activity reply = message.CreateReply($"Hey I am a chatbot");
                //await connector.Conversations.ReplyToActivityAsync(reply);


            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        //public async Task AfterAnyHelp(IDialogContext context, IAwaitable<string> result)
        //{
        //    Activity activity = (Activity)context.Activity;
            
        //    //await connector.Conversations.ReplyToActivityAsync(reply);
        //    string response = await result;
        //    if(response.ToLower() != "no")
        //    {
        //        await Conversation.SendAsync(activity, () => new BasicLuisDialog());
        //    }
        //    else
        //    {
        //        Activity reply = activity.CreateReply("Okay. Have a wonderful day ahead.");
        //        ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
        //        await connector.Conversations.ReplyToActivityAsync(reply);
        //        context.Done(true);
        //    }
        //}
    }
}