using System.Collections.Generic;
using System.IO;
using System.Linq;
using SERESTPlugin.Util;

namespace SERESTPlugin.APIs
{

public class GPS : IAPI
{
    public void Register(APIServer server)
    {
        var api = server.RegisterAPI("gps");

        api.RegisterRequest("GET", (s, ev) => {
            ev.Handled = true;
            var list = new List<VRage.Game.ModAPI.IMyGps>();
            Sandbox.Game.World.MySession.Static.Gpss.GetGpsList(Sandbox.Game.World.MySession.Static.LocalPlayerId, list);

            ev.Context.Response.CloseJSON(new DataTypes.GPSList {
                GPSes = list.Select(g => new DataTypes.GPS(g)).ToArray()
            });
        });
        api.RegisterRequest("POST", (s, ev) => {
            ev.Handled = true;
            if (!ev.Context.Request.HasEntityBody)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide GPS data");
                return;
            }
            if (!ev.Context.Request.TryReadJSON(out DataTypes.GPS data))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Invalid GPS data provided");
                return;
            }
            var modapiGpss = Sandbox.Game.World.MySession.Static.Gpss as VRage.Game.ModAPI.IMyGpsCollection;

            var gps = modapiGpss.Create(data.Name, data.Description, data.Coordinates.ToVector3D(), data.Visible ?? true);
            gps.GPSColor = data.Color?.ToColor() ?? VRageMath.Color.AliceBlue;

            modapiGpss.AddGps(Sandbox.Game.World.MySession.Static.LocalPlayerId, gps);

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Created);
        });

        var specific = api.RegisterSubAPI("(?<name>[^/]+)");
        specific.RegisterRequest("GET", (s, ev) => {
            ev.Handled = true;

            var gps = Sandbox.Game.World.MySession.Static.Gpss.GetGpsByName(Sandbox.Game.World.MySession.Static.LocalPlayerId, ev.Components["name"]);

            ev.Context.Response.CloseJSON(new DataTypes.GPS(gps));
        });
        specific.RegisterRequest("POST", (s, ev) => {
            ev.Handled = true;
            if (!ev.Context.Request.HasEntityBody)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide GPS data");
                return;
            }
            if (!ev.Context.Request.TryReadJSON(out DataTypes.GPS data))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Invalid GPS update data");
                return;
            }

            var gps = Sandbox.Game.World.MySession.Static.Gpss.GetGpsByName(Sandbox.Game.World.MySession.Static.LocalPlayerId, ev.Components["name"]);
            if (gps == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Specified GPS doesn't exist");
                return;
            }

            if (data.Color != null)
                gps.GPSColor = data.Color.ToColor();
            if (data.Coordinates != null)
                gps.Coords = data.Coordinates.ToVector3D();
            if (data.Description != null)
                gps.Description = data.Description;
            if (data.Name != null)
                gps.Name = data.Name;
            if (data.Visible.HasValue)
                gps.ShowOnHud = data.Visible.Value;
            if (data.Lifespan.HasValue)
                gps.DiscardAt = data.Lifespan.Value;

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
        specific.RegisterRequest("DELETE", (s, ev) => {
            ev.Handled = true;
            var id = Sandbox.Game.World.MySession.Static.LocalPlayerId;
            var gps = Sandbox.Game.World.MySession.Static.Gpss.GetGpsByName(id, ev.Components["name"]);

            if (gps != null)
            {
                Sandbox.Game.World.MySession.Static.Gpss.SendDelete(id, gps.Hash);
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NoContent);
            }
            else
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Specified GPS doesn't exist");
        });
    }
}

}