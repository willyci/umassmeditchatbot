using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using Microsoft.Bot.Builder.Azure;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var greetings = new string[] { "hi", "hi there", "hello", "hola" };
            var message = await result;

            bool isGreetings = greetings.Any(s => message.Text.ToLower().Contains(s));
            if (isGreetings)
            {


                List<CardAction> Go = new List<CardAction> { new CardAction(ActionTypes.ImBack, title: "Request for forms", value: "Request for forms"), new CardAction(ActionTypes.ImBack, "FAQs", value: "FAQs"), new CardAction(ActionTypes.ImBack, "Help", value: "Help") };
                HeroCard card = new HeroCard { Buttons = Go };
                var reply = ((Activity)context.Activity).CreateReply("Hello! How may I help you?");
                reply.Attachments.Add(card.ToAttachment());
                await context.PostAsync(reply);
                context.Wait(OptionsChosen);


            }
            else
            {
                await context.PostAsync($"You said {message.Text}");
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task OptionsChosen(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var receivedMessage = await message;

            if (receivedMessage.Text.ToLower() == "request for forms")
            {
                List<CardAction> Go = new List<CardAction> { new CardAction(ActionTypes.ImBack, title: "CMS Account", value: "CMS Account"), new CardAction(ActionTypes.ImBack, "New Website", value: "New Website") };
                HeroCard card = new HeroCard { Buttons = Go };
                var reply = ((Activity)context.Activity).CreateReply("Which of the following forms are you requesting?");

                reply.Attachments.Add(card.ToAttachment());
                await context.PostAsync(reply);
                context.Wait(OptionsChosen);
            }
            else if (receivedMessage.Text.ToLower() == "cms account")
            {
                var myform = new FormDialog<CMSFormRequest>(new CMSFormRequest(), CMSFormRequest.BuildForm, FormOptions.PromptInStart, null);
                context.Call<CMSFormRequest>(myform, CMSFormComplete);

            }
            else if (receivedMessage.Text.ToLower() == "new website")
            {
                var myform = new FormDialog<NewWebsiteRequest>(new NewWebsiteRequest(), NewWebsiteRequest.BuildForm, FormOptions.PromptInStart, null);
                context.Call<NewWebsiteRequest>(myform, NewWebsiteFormComplete);
            }
            else if (receivedMessage.Text.ToLower() == "faqs")
            {
                // call to AskQuestion
                this.AskQuestion(context);
                //await context.PostAsync("FAQs Done!");

            }
            else
            {
                await context.PostAsync("Functionality not implemented!");
                context.Wait(MessageReceivedAsync);
            }

        }
        public static string GetSetting(string key)
        {
            var value = Utils.GetAppSetting(key);
            if (String.IsNullOrEmpty(value) && key == "QnAAuthKey")
            {
                value = Utils.GetAppSetting("QnASubscriptionKey"); // QnASubscriptionKey for backward compatibility with QnAMaker (Preview)
            }
            return value;
        }

        private void AskQuestion(IDialogContext context)
        {
            PromptDialog.Text(context, ResumeCalltoQnADialog, "Please enter your query.");
        }

        private async Task ResumeCalltoQnADialog(IDialogContext context, IAwaitable<object> result)
        {

            var qnaAuthKey = GetSetting("QnAAuthKey");
            var qnaKBId = Utils.GetAppSetting("QnAKnowledgebaseId");
            var endpointHostName = Utils.GetAppSetting("QnAEndpointHostName");

            // QnA Subscription Key and KnowledgeBase Id null verification
            if (!string.IsNullOrEmpty(qnaAuthKey) && !string.IsNullOrEmpty(qnaKBId))
            {
                // Forward to the appropriate Dialog based on whether the endpoint hostname is present
                if (string.IsNullOrEmpty(endpointHostName))
                    await context.Forward(new BasicQnAMakerPreviewDialog(), ConfirmEndToQnADialog, context.Activity, CancellationToken.None);
                else
                    await context.Forward(new BasicQnAMakerDialog(), ConfirmEndToQnADialog, context.Activity, CancellationToken.None);
            }
            else
            {
                await context.PostAsync("Please set QnAKnowledgebaseId, QnAAuthKey and QnAEndpointHostName (if applicable) in App Settings. Learn how to get them at https://aka.ms/qnaabssetup.");
            }
        }

        private async Task ConfirmEndToQnADialog(IDialogContext context, IAwaitable<object> argument)
        {
            PromptDialog.Text(context, AnyOtherQuestions, "Do you have any other questions?.");
        }

        private async Task AnyOtherQuestions(IDialogContext context, IAwaitable<object> argument)
        {
            var anyOtherQues = await argument as string;
            if (anyOtherQues.ToString().ToLower() == "yes")
            {
                // Call to AskQuestion
                this.AskQuestion(context);
            }
            else
            {
                await context.PostAsync("qna dialog finished!!!");
                context.Done<object>(null);
            }
        }



        private async Task CMSFormComplete(IDialogContext context, IAwaitable<CMSFormRequest> result)
        {
            CMSFormRequest cmsRequest = null;

            try
            {
                cmsRequest = await result;
            }
            catch (OperationCanceledException)
            {
                await context.PostAsync("You Cancelled the form!");
                return;
            }

            if (cmsRequest != null)
            {

                RootDialog.SendEmail();

                await context.PostAsync("A request for CMS Account is successfuly raised. You will receive a confirmation email.");
                await context.PostAsync("Is there anything else I can help you with?");
            }
            else
            {
                await context.PostAsync("Form returned empty response!");
            }

            context.Wait(MessageReceivedAsync);
        }

        private static void SendEmail()
        {
            //Activity activity = (Activity)context.Activity;
            //StateClient sc = activity.GetStateClient();
            ////BotData userData = sc.BotState.GetUserDataAsync(activity.ChannelId, activity.Conversation.Id, activity.From.Id);
            //BotData userData = sc.BotState.GetUserData(activity.ChannelId, activity.Conversation.Id);
            //var FirstName = userData.GetProperty<string>("FirstName");
            //var LastName = userData.GetProperty<string>("LastName");
            //var Gender = userData.GetProperty<string>("Gender");

            var smtpClient = new SmtpClient();
            var msg = new MailMessage();
            msg.To.Add("thewolfpack@umassmed.edu");
            msg.Subject = "Sending from AI Chatbot - Test";
            msg.Body = "This is just a test email";
            smtpClient.Send(msg);

        }



        private async Task NewWebsiteFormComplete(IDialogContext context, IAwaitable<NewWebsiteRequest> result)
        {
            NewWebsiteRequest siteRequest = null;

            try
            {
                siteRequest = await result;
            }
            catch (OperationCanceledException)
            {
                await context.PostAsync("You Cancelled the form!");
                return;
            }


            if (siteRequest.PermissionForYourself.ToString().ToLower() == "no")
            {
                var myform = new FormDialog<CMSFormRequest>(new CMSFormRequest(), CMSFormRequest.BuildForm, FormOptions.PromptInStart, null);
                context.Call<CMSFormRequest>(myform, CMSFormComplete);
                //await context.PostAsync("A request for New Website is successfuly raised. You will receive a confirmation email.");
                //await context.PostAsync("Is there anything else I can help you with?");
            }

        }


    }
}