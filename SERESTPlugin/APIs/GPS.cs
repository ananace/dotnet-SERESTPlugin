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

        api.RegisterRequest("GET", "", (s, ev) => {
            ev.Handled = true;
            var list = new List<VRage.Game.ModAPI.IMyGps>();
            Sandbox.Game.World.MySession.Static.Gpss.GetGpsList(Sandbox.Game.World.MySession.Static.LocalPlayerId, list);

            ev.Context.Response.CloseJSON(new DataTypes.GPSList {
                GPSes = list.Select(g => new DataTypes.GPS {
                    Name = g.Name,
                    Description = g.Description,
                    Coordinates = new DataTypes.Coordinate(g.Coords),
                    Visible = g.ShowOnHud,
                    Color = new DataTypes.Color(g.GPSColor)
                }).ToArray()
            });
        });
        api.RegisterRequest("POST", "", (s, ev) => {
            ev.Handled = true;
            var data = ev.Context.Request.ReadJSON<DataTypes.GPS>();
            var modapiGpss = Sandbox.Game.World.MySession.Static.Gpss as VRage.Game.ModAPI.IMyGpsCollection;

            var gps = modapiGpss.Create(data.Name, data.Description, data.Coordinates.ToVector3D(), data.Visible);
            gps.GPSColor = data.Color?.ToColor() ?? VRageMath.Color.AliceBlue;

            modapiGpss.AddGps(Sandbox.Game.World.MySession.Static.LocalPlayerId, gps);

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Created);
        });
        api.RegisterRequest("DELETE", "(?<name>[^/]+)", (s, ev) => {
            ev.Handled = true;
            var id = Sandbox.Game.World.MySession.Static.LocalPlayerId;
            var gps = Sandbox.Game.World.MySession.Static.Gpss.GetGpsByName(id, ev.Components["name"]);

            if (gps != null)
            {
                Sandbox.Game.World.MySession.Static.Gpss.SendDelete(id, gps.Hash);
                ev.Context.Response.Close();
            }
            else
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Specified GPS doesn't exist");
        });
    }
}

}