using System;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SERESTPlugin.Util;

namespace SERESTPlugin.APIs
{

public class Grid : IAPI
{
    public void Register(APIServer server)
    {
        var api = server.RegisterAPI("grids");

        var local = api.RegisterSubAPI("local");
        RegisterGridActions(local, (ev) => FindLocalGrid());

        var byId = api.RegisterSubAPI("id/(?<id>[0-9]+)");
        RegisterGridActions(byId, (ev) => FindGrid(long.Parse(ev.Components["id"])));

        var byName = api.RegisterSubAPI("name/(?<name>[^/]+)");
        RegisterGridActions(byName, (ev) => FindGrid(ev.Components["name"]));

        api = server.RegisterAPI("blocks/id/(?<block_id>[0-9]+)");
        RegisterBlockActions(api, (ev) => FindGlobalBlock(long.Parse(ev.Components["block_id"])));
    }

    void RegisterGridActions(APIRegister api, Func<HTTPEventArgs, MyCubeGrid> findMethod)
    {
        api.RegisterRequest("GET", (s, ev) => {
            ev.Handled = true;
            var grid = findMethod(ev);
            if (grid == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such grid");
                return;
            }

            ev.Context.Response.CloseJSON(new DataTypes.GridInformation(grid));
        });

        var blockApiByName = api.RegisterSubAPI("blocks/name/(?<block_name>[^/]+)");
        RegisterBlockActions(blockApiByName, (ev) => FindBlock(findMethod(ev), System.Web.HttpUtility.UrlDecode(ev.Components["block_name"])));

        var blockApiById = api.RegisterSubAPI("blocks/id/(?<block_id>[0-9]+)");
        RegisterBlockActions(blockApiById, (ev) => FindBlock(findMethod(ev), long.Parse(ev.Components["block_id"])));
    }

    void RegisterBlockActions(APIRegister blockApi, Func<HTTPEventArgs, MyCubeBlock> findMethod)
    {
        blockApi.RegisterRequest("GET", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            ev.Context.Response.CloseJSON(new DataTypes.BlockInformation(block));
        });

        blockApi.RegisterRequest("GET", "enabled", (s, ev) => {
            ev.Handled = true;
            if (!(findMethod(ev) is IMyFunctionalBlock block))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            ev.Context.Response.CloseString(block.Enabled.ToString());
        });
        blockApi.RegisterRequest("POST", "enabled", (s, ev) => {
            ev.Handled = true;
            if (!(findMethod(ev) is IMyFunctionalBlock block))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            block.Enabled = true;
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
        blockApi.RegisterRequest("DELETE", "enabled", (s, ev) => {
            ev.Handled = true;
            if (!(findMethod(ev) is IMyFunctionalBlock block))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            block.Enabled = false;
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });

        blockApi.RegisterRequest("GET", "name", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            ev.Context.Response.CloseString((block as IMyTerminalBlock).CustomName);
        });
        blockApi.RegisterRequest("POST", "name", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            string text = null;
            using (var reader = new System.IO.StreamReader(ev.Context.Request.InputStream))
                text = reader.ReadLine();

            (block as IMyTerminalBlock).CustomName = text;

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });

        blockApi.RegisterRequest("GET", "text", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            if (block is IMyProgrammableBlock pb)
                ev.Context.Response.CloseString(pb.ProgramData);
            else if (block is IMyTextSurface panel)
                ev.Context.Response.CloseString(panel.GetText());
            else
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Block contains no text");
        });
        blockApi.RegisterRequest("POST", "text", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            string text = null;
            using (var reader = new System.IO.StreamReader(ev.Context.Request.InputStream))
                text = reader.ReadToEnd();

            if (block is IMyProgrammableBlock pb)
                pb.ProgramData = text;
            else if (block is IMyTextSurface panel)
                panel.WriteText(text);
            else
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Block contains no text");
                return;
            }

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });

        blockApi.RegisterRequest("GET", "data", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            ev.Context.Response.CloseString((block as IMyTerminalBlock).CustomData);
        });
        blockApi.RegisterRequest("POST", "data", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such block");
                return;
            }

            string text = null;
            using (var reader = new System.IO.StreamReader(ev.Context.Request.InputStream))
                text = reader.ReadToEnd();

            (block as IMyTerminalBlock).CustomData = text;
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NoContent);
        });

        blockApi.RegisterRequest("GET", "light", (s, ev) => {
            ev.Handled = true;
            if (!(findMethod(ev) is IMyLightingBlock block))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such light block");
                return;
            }

            ev.Context.Response.CloseJSON(new DataTypes.LightBlock(block));
        });
        blockApi.RegisterRequest("POST", "light", (s, ev) => {
            ev.Handled = true;
            if (!(findMethod(ev) is IMyLightingBlock block))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No such light block");
                return;
            }

            var settings = ev.Context.Request.ReadJSON<DataTypes.LightBlock>();

            if (settings.BlinkIntervalSeconds.HasValue)
                block.BlinkIntervalSeconds = settings.BlinkIntervalSeconds.Value;
            if (settings.BlinkLength.HasValue)
                block.BlinkLength = settings.BlinkLength.Value;
            if (settings.BlinkOffset.HasValue)
                block.BlinkOffset = settings.BlinkOffset.Value;
            if (settings.Color != null)
                block.Color = settings.Color.ToColor();
            if (settings.Falloff.HasValue)
                block.Falloff = settings.Falloff.Value;
            if (settings.Intensity.HasValue)
                block.Intensity = settings.Intensity.Value;
            if (settings.Radius.HasValue)
                block.Radius = settings.Radius.Value;

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });

    }

    MyCubeGrid FindLocalGrid()
    {
        return (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
    }

    MyCubeGrid FindGrid(string name)
    {
        name = System.Web.HttpUtility.UrlDecode(name);
        return MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.DisplayName == name);
    }

    MyCubeGrid FindGrid(long id)
    {
        return MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.EntityId == id);
    }

    MyCubeBlock FindBlock(MyCubeGrid grid, string name)
    {
        return grid.GetFatBlocks().Where(b => b is IMyTerminalBlock).FirstOrDefault(b => (b as IMyTerminalBlock).CustomName == name);
    }
    MyCubeBlock FindBlock(MyCubeGrid grid, long id)
    {
        return grid.GetFatBlocks().FirstOrDefault(b => b.EntityId == id);
    }

    MyCubeBlock FindGlobalBlock(long id)
    {
        return MyEntities.GetEntityById(id) as MyCubeBlock;
    }
}

}