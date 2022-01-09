using SERESTPlugin.Attributes;
using SERESTPlugin.Util;
using System.Collections.Generic;
using System.Linq;

namespace SERESTPlugin.APIs
{

[API("/r0/gps", OnDedicated = false)]
public class R0LocalGPSAPI : BaseAPI
{
    [APIEndpoint("GET", "/")]
    public DataTypes.GPSList GetList()
    {
        var list = new List<VRage.Game.ModAPI.IMyGps>();
        Sandbox.Game.World.MySession.Static.Gpss.GetGpsList(Sandbox.Game.World.MySession.Static.LocalPlayerId, list);

        return new DataTypes.GPSList { GPSes = list.Select(g => new DataTypes.GPS(g)).ToArray() };
    }

    [APIEndpoint("POST", "/")]
    public void CreateGPS(DataTypes.GPS data)
    {
        var modapiGpss = Sandbox.Game.World.MySession.Static.Gpss as VRage.Game.ModAPI.IMyGpsCollection;

        var gps = modapiGpss.Create(data.Name, data.Description, data.Coordinates.ToVector3D(), data.Visible ?? true);
        gps.GPSColor = data.Color?.ToColor() ?? VRageMath.Color.AliceBlue;

        modapiGpss.AddGps(Sandbox.Game.World.MySession.Static.LocalPlayerId, gps);
    }

    [API("/gps/(?<name>[^/]+)", OnDedicated = false, Needs = new string[] { "gps" })]
    public class SpecificLocalGPSAPI : BaseAPI
    {
        [APIData("gps")]
        public VRage.Game.ModAPI.IMyGps FindGPS()
        {
            var gps = Sandbox.Game.World.MySession.Static.Gpss.GetGpsByName(Sandbox.Game.World.MySession.Static.LocalPlayerId, EventArgs.Components["name"]);
            if (gps == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "Specified GPS doesn't exist");
            return gps;
        }
        public VRage.Game.ModAPI.IMyGps GPS { get { return Data["gps"] as VRage.Game.ModAPI.IMyGps; } }

        [APIEndpoint("GET", "/")]
        public DataTypes.GPS GetGPS()
        {
            return new DataTypes.GPS(GPS);
        }

        [APIEndpoint("POST", "/")]
        public void UpdateGPS(DataTypes.GPS data)
        {
            if (data.Color != null)
                GPS.GPSColor = data.Color.ToColor();
            if (data.Coordinates != null)
                GPS.Coords = data.Coordinates.ToVector3D();
            if (data.Description != null)
                GPS.Description = data.Description;
            if (data.Name != null)
                GPS.Name = data.Name;
            if (data.Visible.HasValue)
                GPS.ShowOnHud = data.Visible.Value;
            if (data.Lifespan.HasValue)
                GPS.DiscardAt = data.Lifespan.Value;
        }

        [APIEndpoint("DELETE", "/")]
        public void DeleteGPS()
        {
            Sandbox.Game.World.MySession.Static.Gpss.SendDelete(Sandbox.Game.World.MySession.Static.LocalPlayerId, GPS.Hash);
        }
    }
}

}