using SERESTPlugin.APIs.DataTypes;
using SERESTPlugin.Util;
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

            var other = api.RegisterSubAPI("(?<name>[^/]+)");
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
                        ev.Context.Response.CloseJSON(new SimpleResult{ Result = faction.IsFriendly(player.Identity.IdentityId) });
                }
            });

            api.RegisterRequest("GET", "list", (s, ev) => {
                ev.Handled = true;
                var names = Sandbox.Game.World.MySession.Static.Players.GetAllIdentitiesOrderByName().Select((id) => id.Value.DisplayName);
                var list = new PlayerList{ Names = names.ToArray() };

                ev.Context.Response.CloseJSON(list);
            });
        }
    }
}
