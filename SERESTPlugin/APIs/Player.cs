using SERESTPlugin.APIs.DataTypes;
using SERESTPlugin.Attributes;
using SERESTPlugin.Util;
using System.Collections.Generic;
using System.Linq;

namespace SERESTPlugin.APIs
{

[API("/r0/player", OnDedicated = false)]
public class LocalPlayerAPI : BaseAPI
{
    [APIEndpoint("GET", "/")]
    public void PlayerStatus()
    {
        var status = new PlayerStatus(Sandbox.Game.World.MySession.Static.LocalCharacter);
        Response.CloseJSON(status);
    }

    [APIEndpoint("GET", "/jetpack")]
    public bool GetJetpack()
    {
        return Sandbox.Game.World.MySession.Static.LocalCharacter.JetpackRunning;
    }
    [APIEndpoint("POST", "/jetpack", NeedsBody = true)]
    public void SetJetpack()
    {
        var wanted = true;
        if (Request.TryReadObject(out string data))
        {
            if (data == "yes" || data == "on" || data == "no" || data == "off")
                wanted = data == "yes" || data == "on";
            else if (data.TryConvert(out bool asBool))
                wanted = asBool;
            else if (data.TryConvert(out int asInt))
                wanted = asInt != 0;
        }

        Sandbox.Game.World.MySession.Static.LocalCharacter.JetpackComp.TurnOnJetpack(wanted);
    }
}

[API("/r0/players")]
public class PlayerAPI : BaseAPI
{
    [APIEndpoint("GET", "/")]
    public IEnumerable<string> PlayerList()
    {
        return Sandbox.Game.World.MySession.Static.Players.GetAllIdentitiesOrderByName().Select((id) => id.Value.DisplayName);
    }

    [API("/(?<name>[^/]+)", Needs = new string[] { "player" })]
    public class SpecificPlayerAPI : BaseAPI
    {
        [APIData("player")]
        public Sandbox.Game.World.MyPlayer FindPlayer()
        {
            var name = EventArgs.Components["name"];
            var player = Sandbox.Game.World.MySession.Static.Players.GetPlayerByName(name);

            if (player == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No player with such name");

            return player;
        }
        public Sandbox.Game.World.MyPlayer Player { get { return Data["player"] as Sandbox.Game.World.MyPlayer; }}

        [APIData("localFaction")]
        public Sandbox.Game.World.MyFaction FindFaction()
        {
            var faction = Sandbox.Game.World.MySession.Static.Factions.GetPlayerFaction(Sandbox.Game.World.MySession.Static.LocalPlayerId);
            if (faction == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "Local player is not in a faction");

            return faction;
        }
        public Sandbox.Game.World.MyFaction LocalFaction { get { return Data["localFaction"] as Sandbox.Game.World.MyFaction; }}

        [APIEndpoint("GET", "/friendly", NeedsData = new string[] { "localFaction" })]
        public void IsFriendly()
        {
            Response.CloseString(LocalFaction.IsFriendly(Player.Identity.IdentityId).ToString());
        }
    }
}

}