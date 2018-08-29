using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace v4Sample_AADv2
{
    public class AADv2BotAccessors
    {
        public static string DialogStateName = $"{nameof(AADv2BotAccessors)}.DialogState";
        public static string CommandStateName = $"{nameof(AADv2BotAccessors)}.CommandState";
        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
        public IStatePropertyAccessor<string> CommandState { get; set; }
    }
}
