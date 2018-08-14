using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;
using Microsoft.Bot.Builder.FormFlow;
using LuisBot.Dialogs;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    // Test
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        public const string FormType = "";



        [LuisIntent("")]
        public async Task EmptyIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            await context.SayAsync(text: "Hi, welcome to UMass medical IT help Chat bot! How can I help you?",
                                   speak: "Hi, welcome to UMass medical IT help Chat bot! How can I help you?");
        }
        
        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Umm, I don't think I understand that :(");
        }

        [LuisIntent("OkayIntent")]
        public async Task OkayIntent(IDialogContext context, LuisResult result)
        {
            PromptDialog.Confirm(context, ResumeAfterOkayIntent, "Do you have any other queries?");
        }

        public async Task ResumeAfterOkayIntent(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmation = await result;
            if (!confirmation)
            {
                await context.PostAsync("Okay. Have a wonderful day ahead. :)");
            }
            else
            {
                await context.PostAsync("How can I help you?");
            }
        }


        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {

            List<string> greetingMsg = new List<string>();
            greetingMsg.Add("Greetings!");
            greetingMsg.Add("Welcome.");
            greetingMsg.Add("Hello.");

            Random r = new Random();
            int rRandomPos = r.Next(greetingMsg.Count);
            string msg = greetingMsg[rRandomPos];
            await context.PostAsync(msg);
            await context.PostAsync("How may I help you?");
        }

        [LuisIntent("WhoAreYou")]
        public async Task Introduction(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hello, I am a UMASS chatbot. \n I can help you with the following requests regarding content management system." +
                        $"\n- Fill CMS Authorization form\n- Fill New Website Request Form\n- Training Tutorials \n- Forgot Password \n- Forgot Username \n- Login Issues\n - Help editing a website " +
                        "\nHow can I help you today?");
        }

        [LuisIntent("Nothing")]
        public async Task NothingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I wish I could help :(\n Anyway, have a great day. Bye.");
        }


        private enum Selection
        {
            [Describe("1. CMS Request")]
            cmsrequest = 1,

            [Describe("2. New Website Request")]
            newwebsiterequest = 2
        }

        [LuisIntent("GetFormRequest")]
        public async Task GetFormRequest(IDialogContext context, LuisResult result)
        {
            EntityRecommendation formType;
            if(!result.TryFindEntity(FormType, out formType))
            {
                var options = new Selection[] { Selection.cmsrequest, Selection.newwebsiterequest };
                var descriptions = new string[] { "CMS Request", "New Website Request" };
                PromptDialog.Choice<Selection>(context, ResumeAfterFormSelection,
                    options, "Which of the following forms are you requesting?", descriptions: descriptions);
            }
        }

        private async Task ResumeAfterFormSelection(IDialogContext context, IAwaitable<Selection> result)
        {
            var selection = await result;

            if(selection.ToString().ToLower() == "cmsrequest")
            {
                await context.PostAsync("For setting up a CMS Account, I would need the following information: ");
                var myform = new FormDialog<CMSFormRequest>(new CMSFormRequest(), CMSFormRequest.BuildForm, FormOptions.PromptInStart, null);
                context.Call<CMSFormRequest>(myform, CMSFormComplete);
            }
            else if(selection.ToString().ToLower() == "newwebsiterequest")
            {
                var myform = new FormDialog<NewWebsiteRequest>(new NewWebsiteRequest(), NewWebsiteRequest.BuildForm, FormOptions.PromptInStart, null);
                context.Call<NewWebsiteRequest>(myform, NewWebsiteFormComplete);
            }
        }
        
        [LuisIntent("CMSRequestForm")]
        public async Task CMSRequestFormIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("For setting up a CMS Account, I would need the following information: ");
            var myform = new FormDialog<CMSFormRequest>(new CMSFormRequest(), CMSFormRequest.BuildForm, FormOptions.PromptInStart, null);
            context.Call<CMSFormRequest>(myform, CMSFormComplete);
            //await this.CallCMSRequestForm(context, result);
        }

        [LuisIntent("NewWebisteRequest")]
        public async Task NewWebsiteRequestIntent(IDialogContext context, LuisResult result)
        {
            var myform = new FormDialog<NewWebsiteRequest>(new NewWebsiteRequest(), NewWebsiteRequest.BuildForm, FormOptions.PromptInStart, null);
            context.Call<NewWebsiteRequest>(myform, NewWebsiteFormComplete);
        }

        [LuisIntent("ForgotUsername")]
        public async Task ForgotUsernameIntent(IDialogContext context, LuisResult result)
        {
            PromptDialog.Confirm(context, ResumeAfterForgotUserName, "Are you a UMass Memorial User?");
        }

        private async Task ResumeAfterForgotUserName(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmation = await result;
            
            if (confirmation)
            {
                await context.PostAsync("UMassmemorial users will need a CMS account created. Due to a change in the Active Directory structure.");
                var myform = new FormDialog<CMSFormRequest>(new CMSFormRequest(), CMSFormRequest.BuildForm, FormOptions.PromptInStart, null);
                context.Call(myform, CMSFormComplete);
            }
            else
            {
                await context.PostAsync("Use your umassmed account, the one you were assigned and use to log onto your computer.");
            }
        }

        [LuisIntent("HelpEditing")]
        public async Task HelpEditingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You can attend the Genius Cafe for a walk through of Episerver and how it functions as well as one-on-one support." +
                                        $"On Monday, Wednesdays and Fridays, the location is in the Albert Sherman Center Caf� from 10AM to 1PM. " + 
                                        $"And every 1st and 3rd Tuesday at South Street in IT in the WolfPack Pod from 10 AM to 12 PM.");

            await context.PostAsync("Live Chat is also available Monday through Friday from 9 AM to 5 PM.");
            var message = context.MakeMessage();

            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = $"https://www.umassmed.edu/it/services/web-services/geniuscafe/",
                Type = "openUrl",
                Title = "Yes"
            };
            CardAction plButton1 = new CardAction()
            {
                Value = $"no",
                Type = "imBack",
                Title = "No"
            };

            cardButtons.Add(plButton);
            cardButtons.Add(plButton1);

            HeroCard plCard = new HeroCard()
            {
                Title = $"Would you like to open Live Chat?",
                Buttons = cardButtons
            };

            Attachment attachment = plCard.ToAttachment();
            message.Attachments = new List<Attachment> { attachment };
            await context.PostAsync(message);

        }
        
        [LuisIntent("ForgotPassword")]
        public async Task ForgotPasswordIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("If you don't know your password, call, email, or visit the help desk. The password reset tool will be returning later this year.");

            var message = context.MakeMessage();
            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = $"https://umassmed.service-now.com/sp/?id=sc_cat_item&sys_id=f62d86334feef60026bcf0318110c776",
                Type = "openUrl",
                Title = "Report an issue"
            };
            CardAction plButton1 = new CardAction()
            {
                Value = "5088568643",
                Type = "call",
                Title = "Call Help Desk"
            };


            cardButtons.Add(plButton);
            cardButtons.Add(plButton1);

            HeroCard plCard = new HeroCard()
            {
                Title = $"What would you like to do?",
                Buttons = cardButtons
            };

            Attachment attachment = plCard.ToAttachment();
            message.Attachments = new List<Attachment> { attachment };
            await context.PostAsync(message);

        }
        
        [LuisIntent("FindTraining")]
        public async Task FindTrainingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("We have many tutorials online and provide in person support when needed." +
                $"The Genius Cafe provides more targeted trainings in using Calendars, Blocks, Hero Sliders (Slideshows) and Navigation.");

            var message = context.MakeMessage();
            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = $"https://www.umassmed.edu/it/services/web-services/episerver/tutorials/",
                Type = "openUrl",
                Title = "Browse Online Tutorials"
            };
            CardAction plButton1 = new CardAction()
            {
                Value = "https://www.umassmed.edu/it/services/web-services/genius-cafe-workshops/",
                Type = "openUrl",
                Title = "Attend Genius Cafe Workshop Series"
            };
            CardAction plButton2 = new CardAction()
            {
                //  Value = this.HelpEditingIntent(context, result),
                Value = "One-on-one support",
                Type = "imBack",
                Title = "One-on-one support at Genius Cafe"
            };

            cardButtons.Add(plButton);
            cardButtons.Add(plButton1);
            cardButtons.Add(plButton2);

            HeroCard plCard = new HeroCard()
            {
                Title = $"Please select an option: ",
                Buttons = cardButtons
            };

            Attachment attachment = plCard.ToAttachment();
            message.Attachments = new List<Attachment> { attachment };
            await context.PostAsync(message);
        }

        [LuisIntent("LoginIssues")]
        public async Task LoginIssuesIntent(IDialogContext context, LuisResult result)
        {
            context.Call(new LoginIssuesDialog(), AfterLoginissuesIntent);
        }

        private async Task AfterLoginissuesIntent(IDialogContext context, IAwaitable<object> result)
        {
            //await context.PostAsync("You are all set. Bye :)");
        }

        [LuisIntent("MeaningOfLife")]
        public async Task MeaningOfLifeIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("42");
        }

        [LuisIntent("Laughing")]
        public async Task LaughingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I am glad you find me amusing :)");
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hello, I am a UMASS chatbot. \n I can help you with the following requests regarding content management system." +
                        $"\n- Fill CMS Authorization form\n- Fill New Website Request Form\n- Training Tutorials \n- Forgot Password \n- Forgot Username \n- Login Issues\n - Help editing a website " +
                        "\nHow can I help you today?");
        }

        [LuisIntent("LoveIntent")]
        public async Task LoveIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Aww, I am flattered.");
        }

        [LuisIntent("WhoMadeYou")]
        public async Task WhoMadeYouIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I am created by the coolest team, the WolfPack. ");
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result) 
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }

        [LuisIntent("EndConversation")]
        public async Task EndConversation(IDialogContext context, LuisResult result)
        {
            context.Done(true);
        }

        public static async Task CMSFormComplete(IDialogContext context, IAwaitable<CMSFormRequest> result)
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

                context.PrivateConversationData.SetValue("ProfileComplete", true);
                context.PrivateConversationData.SetValue("FullName", cmsRequest.FullName);
                context.PrivateConversationData.SetValue("Department", cmsRequest.Department);
                context.PrivateConversationData.SetValue("UserEmail", cmsRequest.userEmail);
                context.PrivateConversationData.SetValue("WebAddress", cmsRequest.WebAddress);
                if(cmsRequest.PhoneNumber == null)
                {
                    context.PrivateConversationData.SetValue("Phone", "");
                }
                else
                {
                    context.PrivateConversationData.SetValue("Phone", cmsRequest.PhoneNumber);
                }

                if (cmsRequest.ExtraNotes == null)
                {
                    context.PrivateConversationData.SetValue("ExtraNotes", "");
                }
                else
                {
                    context.PrivateConversationData.SetValue("ExtraNotes", cmsRequest.ExtraNotes);
                }
                
                context.PrivateConversationData.SetValue("SupervisorName", cmsRequest.SupervisorName);
                context.PrivateConversationData.SetValue("SupervisorEmail", cmsRequest.SupervisorEmail);

                SendEmail(cmsRequest, null);

                await context.PostAsync("A request for CMS Account is successfuly raised. You will receive a confirmation email.");
                
            }
            else
            {
                await context.PostAsync("Form returned empty response!");
                
            }
            context.Done(true);
        }

        public static void SendEmail(CMSFormRequest CmsRequest, NewWebsiteRequest NewWebsiteRequest)
        {
            var smtpClient = new System.Net.Mail.SmtpClient();
            var msg = new System.Net.Mail.MailMessage();
            msg.To.Add("mahima.parashar@umassmed.edu");
            if (NewWebsiteRequest == null)
            {
                msg.Subject = "Test";
                msg.Body = "CMS Request";
            }
            else
            {
                msg.Subject = "Test";
                msg.Body = "New Website Request";
            }
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

            if (siteRequest != null)
            {

                context.PrivateConversationData.SetValue("ProfileComplete", true);
                context.PrivateConversationData.SetValue("FullName", siteRequest.FullName);
                context.PrivateConversationData.SetValue("Department", siteRequest.Department);
                context.PrivateConversationData.SetValue("UserEmail", siteRequest.userEmail);
                context.PrivateConversationData.SetValue("WebAddress", siteRequest.WebAddress);
                context.PrivateConversationData.SetValue("Phone", siteRequest.PhoneNumber);
                context.PrivateConversationData.SetValue("TargteDate", siteRequest.TargetDate);

                if (siteRequest.ExtraNotes == null)
                {
                    context.PrivateConversationData.SetValue("ExtraNotes", "");
                }
                else
                {
                    context.PrivateConversationData.SetValue("ExtraNotes", siteRequest.ExtraNotes);
                }

                SendEmail(null, siteRequest);
                await context.PostAsync("A request for New Website is successfuly raised. You will receive a confirmation email shortly.");

                if (siteRequest.PermissionForYourself.ToString().ToLower() == "no")
                {
                    await context.PostAsync("As you need CMS Permissions, you'll have to fill out an authorization form.");
                    var myform = new FormDialog<CMSFormRequest>(new CMSFormRequest(), CMSFormRequest.BuildForm, FormOptions.PromptInStart, null);
                    context.Call(myform, CMSFormComplete);
                }
                else
                {
                    context.Done(true);
                }
                

            }
            else
            {
                await context.PostAsync("Form returned empty response!");
                context.Done(true);
            }

           


        }
    }
}
