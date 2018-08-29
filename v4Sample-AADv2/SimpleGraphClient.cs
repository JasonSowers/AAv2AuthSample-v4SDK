using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace v4Sample_AADv2
{
    public class SimpleGraphClient
    {
        private readonly string _token;

        public SimpleGraphClient(string token)
        {
            _token = token;
        }

        public async Task<bool> SendMail(string toAddress, string subject, string content)
        {
            try
            {
                var graphClient = GetAuthenticatedClient();

                List<Recipient> recipients = new List<Recipient>();
                recipients.Add(new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = toAddress
                    }
                });

                // Create the message.
                Message email = new Message
                {
                    Body = new ItemBody
                    {
                        Content = content,
                        ContentType = BodyType.Text,
                    },
                    Subject = subject,
                    ToRecipients = recipients
                };

                // Send the message.
                await graphClient.Me.SendMail(email, true).Request().PostAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<User> GetMe()
        {
            var graphClient = GetAuthenticatedClient();
            var me = await graphClient.Me.Request().GetAsync();
            return me;
        }

        public async Task<User> GetManager()
        {
            var graphClient = GetAuthenticatedClient();
            User manager = await graphClient.Me.Manager.Request().GetAsync() as User;
            return manager;
        }

        public async Task<PhotoResponse> GetPhoto()
        {
            var photoResponse =
                await new HttpClient().GetStreamWithAuthAsync(_token,
                    "https://graph.microsoft.com/v1.0/me/photo/$value");
            if (photoResponse != null)
            {
                photoResponse.Base64string = $"data:{photoResponse.ContentType};base64," +
                                             Convert.ToBase64String(photoResponse.Bytes);
            }

            return photoResponse;
        }

        private GraphServiceClient GetAuthenticatedClient()
        {
            GraphServiceClient graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        string accessToken = _token;

                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");
                    }));
            return graphClient;
        }

        public async Task<Message[]> GetRecentUnreadMail()
        {
            var graphClient = GetAuthenticatedClient();
            IMailFolderMessagesCollectionPage messages =
                await graphClient.Me.MailFolders.Inbox.Messages.Request().GetAsync();
            DateTime from = DateTime.Now.Subtract(TimeSpan.FromMinutes(60));
            List<Message> unreadMessages = new List<Message>();

            var done = false;
            while (messages?.Count > 0 && !done)
            {
                foreach (Message message in messages)
                {
                    if (message.ReceivedDateTime.HasValue && message.ReceivedDateTime.Value >= from)
                    {
                        if (message.IsRead.HasValue && !message.IsRead.Value)
                        {
                            unreadMessages.Add(message);
                            if (unreadMessages.Count >= 5)
                            {
                                done = true;
                            }
                        }
                    }
                    else
                    {
                        done = true;
                    }
                }
                messages = await messages.NextPageRequest.GetAsync();
            }

            return unreadMessages.ToArray();
        }
    }
}
