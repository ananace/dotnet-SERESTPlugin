using System.Collections.Generic;
using System.Linq;
using SERESTPlugin.Util;

namespace SERESTPlugin.APIs
{

public class Chat : IAPI
{
    public void Register(APIServer server)
    {
        var api = server.RegisterAPI("chat");

        api.RegisterRequest("POST", (s, ev) => {
            ev.Handled = true;
            if (!ev.Context.Request.TryReadObject(out string message))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide a chat message");
                return;
            }

            var sys = Sandbox.Game.World.MySession.Static.ChatSystem;
            if (sys.CommandSystem.CanHandle(message))
            {
                string name = Sandbox.Game.World.MySession.Static.LocalHumanPlayer.DisplayName;
                Sandbox.Game.Gui.MyHud.Chat.ShowMessage(name, message);
                sys.CommandSystem.Handle(message);
            }
            else
            {
                var query = System.Web.HttpUtility.ParseQueryString(ev.Context.Request.Url.Query);
                if (!string.IsNullOrEmpty(query["author"]))
                    Sandbox.Game.Gui.MyHud.Chat.ShowMessageScripted(query["author"], message);
                else
                {
                    string name = Sandbox.Game.World.MySession.Static.LocalHumanPlayer.DisplayName;
                    Sandbox.Game.Gui.MyHud.Chat.ShowMessageColoredSP(message, Sandbox.Game.Gui.ChatChannel.Global);
                    sys.ChatHistory.EnqueueMessage(message, sys.CurrentChannel, Sandbox.Game.World.MySession.Static.LocalPlayerId);
                }
            }
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
        api.RegisterRequest("GET", "history", (s, ev) => {
            ev.Handled = true;

            var history = Sandbox.Game.World.MySession.Static.ChatSystem.ChatHistory;
            var query = System.Web.HttpUtility.ParseQueryString(ev.Context.Request.Url.Query);
            var list = new List<Sandbox.Game.Entities.Character.MyUnifiedChatItem>();
            if (!string.IsNullOrEmpty(query["channel"]))
            {
                if (!System.Enum.TryParse(query["channel"], true, out Sandbox.Game.Gui.ChatChannel channel))
                {
                    ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "No such chat channel");
                    return;
                }

                switch (channel)
                {
                case Sandbox.Game.Gui.ChatChannel.ChatBot: history.GetChatbotHistory(ref list); break;
                case Sandbox.Game.Gui.ChatChannel.Faction:
                    if (string.IsNullOrEmpty(query["faction"]))
                    {
                        ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide a faction ID");
                        return;
                    }

                    if (!long.TryParse(query["faction"], out long factionId))
                    {
                        ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Faction ID is not valid");
                        return;
                    }

                    history.GetFactionHistory(ref list, factionId);
                    break;
                case Sandbox.Game.Gui.ChatChannel.Global: history.GetGeneralHistory(ref list); break;
                case Sandbox.Game.Gui.ChatChannel.GlobalScripted: history.GetGeneralHistory(ref list); break;
                case Sandbox.Game.Gui.ChatChannel.Private:
                    if (string.IsNullOrEmpty(query["player"]))
                    {
                        ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide a player ID");
                        return;
                    }

                    if (!long.TryParse(query["player"], out long playerId))
                    {
                        ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Player ID is not valid");
                        return;
                    }
                    history.GetPrivateHistory(ref list, playerId);
                    break;
                }
            }
            else
                history.GetCompleteHistory(ref list);

            ev.Context.Response.CloseJSON(new DataTypes.ChatHistory{ Messages = list.Select(m => new DataTypes.ChatMessage(m)).ToArray() });
        });
        api.RegisterRequest("GET", "sse", (s, ev) => {
            ev.Handled = true;
            if (!ev.Context.Request.AcceptTypes.Any(type => type == "text/event-stream"))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotAcceptable, "Need to accept text/event-stream");
                return;
            }

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotImplemented);
            return;

            /*
            ev.Context.Response.KeepAlive = true;
            var sse = new SSEWrapper(ev.Context);

            var sys = Sandbox.Game.World.MySession.Static.ChatSystem;

            sys.FactionMessageReceived += (message) => {
                
            };
            sys.PlayerMessageReceived += (message) => {
                
            };

            server.AddSSE(sse);
            */
        });
    }

}

}