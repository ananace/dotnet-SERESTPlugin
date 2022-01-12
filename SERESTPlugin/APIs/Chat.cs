using SERESTPlugin.Attributes;
using SERESTPlugin.Util;
using System.Collections.Generic;
using System.Linq;

namespace SERESTPlugin.APIs
{

[API("/r0/chat")]
public class ChatAPI : BaseAPI
{
    [APIEndpoint("POST", "/", NeedsBody = true)]
    public void PostMessage(string message)
    {
        var sys = Sandbox.Game.World.MySession.Static.ChatSystem;
        if (sys.CommandSystem.CanHandle(message))
        {
            string name = Sandbox.Game.World.MySession.Static.LocalHumanPlayer.DisplayName;
            Sandbox.Game.Gui.MyHud.Chat.ShowMessage(name, message);
            sys.CommandSystem.Handle(message);
        }
        else
        {
            var color = VRageMath.Color.Yellow;
            var query = System.Web.HttpUtility.ParseQueryString(Request.Url.Query);
            if (!string.IsNullOrEmpty(query["color"]))
            {
                var parsed = typeof(VRageMath.Color).GetProperties(System.Reflection.BindingFlags.Static).FirstOrDefault(c => c.Name.Equals(query["color"], System.StringComparison.OrdinalIgnoreCase));
                if (parsed != null)
                    color = (VRageMath.Color)parsed.GetValue(null);
            }

            if (!string.IsNullOrEmpty(query["author"]))
                Sandbox.Game.MyVisualScriptLogicProvider.SendChatMessageColored(message, color, query["author"], Sandbox.Game.World.MySession.Static.LocalPlayerId);
            else
                Sandbox.Game.MyVisualScriptLogicProvider.SendChatMessageColored(message, color, Sandbox.Game.World.MySession.Static.LocalHumanPlayer.DisplayName, Sandbox.Game.World.MySession.Static.LocalPlayerId);
        }
    }

    [APIEndpoint("GET", "/history")]
    public DataTypes.ChatHistory GetHistory()
    {
        var history = Sandbox.Game.World.MySession.Static.ChatSystem.ChatHistory;
        var query = System.Web.HttpUtility.ParseQueryString(Request.Url.Query);
        var list = new List<Sandbox.Game.Entities.Character.MyUnifiedChatItem>();
        if (!string.IsNullOrEmpty(query["channel"]))
        {
            if (!System.Enum.TryParse(query["channel"], true, out Sandbox.Game.Gui.ChatChannel channel))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "No such chat channel");

            switch (channel)
            {
            case Sandbox.Game.Gui.ChatChannel.ChatBot: history.GetChatbotHistory(ref list); break;
            case Sandbox.Game.Gui.ChatChannel.Faction:
                if (string.IsNullOrEmpty(query["faction"]))
                    throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Need to provide a faction ID");

                if (!long.TryParse(query["faction"], out long factionId))
                    throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Faction ID is not valid");

                history.GetFactionHistory(ref list, factionId);
                break;
            case Sandbox.Game.Gui.ChatChannel.Global: history.GetGeneralHistory(ref list); break;
            case Sandbox.Game.Gui.ChatChannel.GlobalScripted: history.GetGeneralHistory(ref list); break;
            case Sandbox.Game.Gui.ChatChannel.Private:
                if (string.IsNullOrEmpty(query["player"]))
                    throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Need to provide a player ID");

                if (!long.TryParse(query["player"], out long playerId))
                    throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Player ID is not valid");

                history.GetPrivateHistory(ref list, playerId);
                break;
            }
        }
        else
            history.GetCompleteHistory(ref list);

        return new DataTypes.ChatHistory{ Messages = list.Select(m => new DataTypes.ChatMessage(m)).ToArray() };
    }

    [APIEndpoint("GET", "/sse", ClosesResponse = true)]
    public void GetEvents()
    {
        if (!Request.AcceptTypes.Any(type => type == "text/event-stream"))
            throw new HTTPException(System.Net.HttpStatusCode.NotAcceptable, "Need to accept text/event-stream");

        var sse = new SSEWrapper(Context);
        Sandbox.ModAPI.MyAPIGateway.Utilities.MessageEntered += (string msg, ref bool _) => {
            sse.SendJSON("message.entered", new DataTypes.ChatMessage{ Sender = Sandbox.Game.World.MySession.Static.LocalPlayerId, Author = Sandbox.Game.World.MySession.Static.LocalHumanPlayer.DisplayName, Message = msg });
        };
        Sandbox.ModAPI.MyAPIGateway.Utilities.MessageRecieved += (ulong sender, string msg) => {
            var senderId = Sandbox.ModAPI.MyAPIGateway.Players.TryGetIdentityId(sender);
            List<VRage.Game.ModAPI.IMyPlayer> players = new List<VRage.Game.ModAPI.IMyPlayer>();
            Sandbox.ModAPI.MyAPIGateway.Players.GetPlayers(players);
            sse.SendJSON("message.received", new DataTypes.ChatMessage{ Sender = senderId, Author = players.FirstOrDefault(p => p.SteamUserId == sender)?.DisplayName, Message = msg });
        };

        APIServer.AddSSE(sse);
    }
}

}
