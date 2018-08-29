using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace v4Sample_AADv2
{
    public static class OAuthHelpers
    {
        public static async Task SendMail(ITurnContext context, TokenResponse tokenResponse, string recipient)
        {
            var token = tokenResponse;
            var client = new SimpleGraphClient(token.Token);

            var me = await client.GetMe();

            await client.SendMail(recipient, "Message from a bot!", $"Hi there! I had this message sent from a bot. - Your friend, {me.DisplayName}");

            await context.SendActivityAsync($"I sent a message to '{recipient}' from your account :)");
        }


        public static async Task ListMe(ITurnContext context, TokenResponse tokenResponse)
        {
            var token = tokenResponse;
            var client = new SimpleGraphClient(token.Token);
            
            var me = await client.GetMe();
            var manager = await client.GetManager();

            var reply = context.Activity.CreateReply();
            var photoResponse = await client.GetPhoto();
            var photoText = string.Empty;
            if (photoResponse != null)
            {
                var replyAttachment = new Attachment(photoResponse.ContentType, photoResponse.Base64string);
                reply.Attachments.Add(replyAttachment);
            }
            else
            {
                photoText = "You should really add an image to your Outlook profile :)";
            }
            reply.Text = $"You are {me.DisplayName} and you report to {manager.DisplayName}.  {photoText}";
            await context.SendActivityAsync(reply);
        }
        public static async Task ListRecentMail(ITurnContext context, TokenResponse tokenResponse)
        {
            
            var client = new SimpleGraphClient(tokenResponse.Token);
            var messages = await client.GetRecentUnreadMail();
            var reply = context.Activity.CreateReply();

            if (messages.Any())
            {
                int count = messages.Length;
                if (count > 5)
                {
                    count = 5;
                }
                reply.Attachments = new List<Attachment>();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                for (int i = 0; i < count; i++)
                {
                    var mail = messages[i];
                    var card = new HeroCard(mail.Subject,$"{mail.From.EmailAddress.Name} <{mail.From.EmailAddress.Address}>",
                        mail.BodyPreview,new List<CardImage>(){new CardImage("https://jasonazurestorage.blob.core.windows.net/files/OutlookLogo.jpg","Outlook Logo") });
                    reply.Attachments.Add(card.ToAttachment());
                }
            }
            else
            {
                reply.Text = "Unable to find any unread mail in the past 60 minutes";
            }

            await context.SendActivityAsync(reply);
        }

        public static OAuthPrompt Prompt(string connectionName)
        {
            return new OAuthPrompt("loginPrompt",
                new OAuthPromptSettings
                {
                    ConnectionName = connectionName,
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000 // User has 5 minutes to login
                });
        }
    }
}
