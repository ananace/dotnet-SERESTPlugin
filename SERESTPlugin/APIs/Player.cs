using SERESTPlugin.APIs.DataTypes;
using SERESTPlugin.Util;
using System.IO;
using System.Linq;

namespace SERESTPlugin.APIs
{
    class Player : IAPI
    {
        public void Register(APIServer server)
        {
            var api = server.RegisterAPI("player");

            api.RegisterRequest("GET", "", (s, ev) => {
                ev.Handled = true;
                var status = new PlayerStatus(Sandbox.Game.World.MySession.Static.LocalCharacter);
                ev.Context.Response.CloseJSON(status);
            });

            api.RegisterRequest("GET", "jetpack", (s, ev) => {
                ev.Handled = true;
                ev.Context.Response.CloseString(Sandbox.Game.World.MySession.Static.LocalCharacter.JetpackRunning.ToString());
            });
            api.RegisterRequest("POST", "jetpack", (s, ev) => {
                ev.Handled = true;
                var wanted = true;
                if (ev.Context.Request.TryReadObject(out string data))
                {
                    if (data == "yes" || data == "on" || data == "no" || data == "off")
                        wanted = data == "yes" || data == "on";
                    else if (data.TryConvert(out bool asBool))
                        wanted = asBool;
                    else if (data.TryConvert(out int asInt))
                        wanted = asInt != 0;
                }

                Sandbox.Game.World.MySession.Static.LocalCharacter.JetpackComp.TurnOnJetpack(wanted);
            });

            var multiApi = server.RegisterAPI("players");
            multiApi.RegisterRequest("GET", (s, ev) => {
                ev.Handled = true;
                var names = Sandbox.Game.World.MySession.Static.Players.GetAllIdentitiesOrderByName().Select((id) => id.Value.DisplayName);
                var list = new PlayerList{ Names = names.ToArray() };

                ev.Context.Response.CloseJSON(list);
            });

            var other = multiApi.RegisterSubAPI("(?<name>[^/]+)");
            other.RegisterRequest("GET", "friendly", (s, ev) => {
                ev.Handled = true;
                var name = ev.Components["name"];
                var player = Sandbox.Game.World.MySession.Static.Players.GetPlayerByName(name);

                if (player == null)
                    ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such player");
                else
                {
                    var faction = Sandbox.Game.World.MySession.Static.Factions.GetPlayerFaction(Sandbox.Game.World.MySession.Static.LocalPlayerId);
                    if (faction == null)
                        ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Local player is not in a faction");
                    else
                        ev.Context.Response.CloseString(faction.IsFriendly(player.Identity.IdentityId).ToString());
                }
            });
        }
    }
}
