using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace SmartMirror.Bot.Dialogs
{
    [LuisModel("141992cd-189a-415b-94ee-49a745b28271", "6879f9dd46e3476fbf0f95e9c118fe29")]
    [Serializable]
    public class RootDialog : LuisDialog<IMessageActivity>
    {
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("None");
        }

        [LuisIntent("AddMirrorPart")]
        public async Task AddMirrorPart(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("AddMirrorPart");
        }

        [LuisIntent("ClearUsers")]
        public async Task ClearUsers(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("ClearUsers");
        }

        [LuisIntent("EditMirror")]
        public async Task EditMirror(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("EditMirror");
        }
    }
}