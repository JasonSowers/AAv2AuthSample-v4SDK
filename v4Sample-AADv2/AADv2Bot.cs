using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace v4Sample_AADv2
{
    public class AADv2Bot : IBot
    {
        private readonly AADv2BotAccessors _stateAccessors;
        private readonly DialogSet _dialogs;
        public const string ConnectionSettingName = ""; // Your connection name

        private const string HelpText =
            "You can type 'send <recipient_email>' to send an email,'recent' to view recent unread mail" +
            " 'me' to see information about yourself, or 'help' to view the commands" +
            " again. Any other text will display your token.";

        public AADv2Bot(AADv2BotAccessors accessors)
        {
            _stateAccessors = accessors;
            _dialogs = _dialogs = new DialogSet(_stateAccessors.ConversationDialogState);
            _dialogs.Add(OAuthHelpers.Prompt(ConnectionSettingName));
            _dialogs.Add(
                new WaterfallDialog("displayToken", new WaterfallStep[]
                {
                    async (dc, step, ct) =>
                    {
                            var activity = dc.Context.Activity;
                        if (dc.Context.Activity.Type == ActivityTypes.Message &&
                            !Regex.IsMatch(dc.Context.Activity.Text, @"(\d{6})"))
                        {
                             await _stateAccessors.CommandState.SetAsync(dc.Context, activity.Text);
                        }

                            return await dc.BeginAsync("loginPrompt");
                        
                    },
                    async (dc, step, ct) =>
                    {
                        var activity = dc.Context.Activity;
                        if (step.Result != null)
                        {
                            var tokenResponse = step.Result as TokenResponse;

                            if (tokenResponse?.Token != null)
                            {
                                var parts = _stateAccessors.CommandState.GetAsync(dc.Context).Result.Split(' ');

                                if (parts[0].ToLowerInvariant() == "me")
                                {
                                    await OAuthHelpers.ListMe(dc.Context, tokenResponse);
                                    await _stateAccessors.CommandState.DeleteAsync(dc.Context);
                                }
                                else if (parts[0].ToLowerInvariant().StartsWith("send"))
                                {
                                    await OAuthHelpers.SendMail(dc.Context, tokenResponse, parts[1]);
                                    await _stateAccessors.CommandState.DeleteAsync(dc.Context);
                                }
                                else if (parts[0].ToLowerInvariant().StartsWith("recent"))
                                {
                                    await OAuthHelpers.ListRecentMail(dc.Context, tokenResponse);
                                    await _stateAccessors.CommandState.DeleteAsync(dc.Context);
                                }
                                else
                                {
                                    await dc.Context.SendActivityAsync($"your token is: {tokenResponse.Token}");
                                    await _stateAccessors.CommandState.DeleteAsync(dc.Context);
                                }
                            }
                        }
                        else
                        {
                            await dc.Context.SendActivityAsync("sorry... We couldn't log you in. Try again later.");
                        }

                        return await dc.EndAsync();
                    }
                }));
        }



        public async Task OnTurnAsync(ITurnContext turnContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                DialogContext dc = null;

                switch (turnContext.Activity.Type)
                {
                    case ActivityTypes.Message:

                        switch (turnContext.Activity.Text.ToLowerInvariant())
                        {
                            case "signout":
                            case "logout":
                            case "signoff":
                            case "logoff":
                                var botAdapter = (BotFrameworkAdapter) turnContext.Adapter;
                                await botAdapter.SignOutUserAsync(turnContext, ConnectionSettingName,
                                    CancellationToken.None);
                                await turnContext.SendActivityAsync("You are now signed out.");
                                break;
                            case "help":
                                await turnContext.SendActivityAsync(HelpText);
                                break;
                            default:
                                dc = await _dialogs.CreateContextAsync(turnContext);
                                await dc.ContinueAsync();
                                if (!turnContext.Responded) await dc.BeginAsync("displayToken");
                                break;
                        }

                        break;
                    case ActivityTypes.Event:
                    case ActivityTypes.Invoke:
                        // This handles the MS Teams invoke activity sent when magic code is not used
                        //See: https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/authentication/auth-oauth-card#getting-started-with-oauthcard-in-teams
                        //Manifest Schema Here: https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema
                        dc = await _dialogs.CreateContextAsync(turnContext);
                        await dc.ContinueAsync();
                        if (!turnContext.Responded) await dc.BeginAsync("displayToken");
                        break;
                    case ActivityTypes.ConversationUpdate:
                        var newUserName = turnContext.Activity.MembersAdded.FirstOrDefault()?.Name;
                        if (!string.Equals("Bot", newUserName))
                        {
                            var reply = turnContext.Activity.CreateReply();
                            reply.Text = HelpText;
                            reply.Attachments = new List<Attachment> {GetHeroCard(newUserName).ToAttachment()};
                            await turnContext.SendActivityAsync(reply);
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                await turnContext.SendActivityAsync($"Exception: {e.Message}");
            }
        }

        private static HeroCard GetHeroCard(string newUserName)
        {
            var heroCard = new HeroCard($"Welcome {newUserName}", "OAuthBot", HelpText);
            heroCard.Images = new List<CardImage>
            {
                new CardImage(
                    "https://jasonazurestorage.blob.core.windows.net/files/aadlogo.png",
                    "AAD Logo",
                    new CardAction(
                        ActionTypes.OpenUrl,
                        value: "https://ms.portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Overview")
                )
            };
            heroCard.Buttons = new List<CardAction>
            {
                new CardAction(ActionTypes.ImBack, "Me", text: "Me", displayText: "Me", value: "Me"),
                new CardAction(ActionTypes.ImBack, "Recent", text: "Recent", displayText: "Recent", value: "Recent"),
                new CardAction(ActionTypes.ImBack, "View Token", text: "View Token", displayText: "View Token", value: "View Token"),
                new CardAction(ActionTypes.ImBack, "Help", text: "Help", displayText: "Help", value: "Help"),
                new CardAction(ActionTypes.ImBack, "Signout", text: "Signout", displayText: "Signout", value: "Signout")
            };
            return heroCard;
        }
    }
}