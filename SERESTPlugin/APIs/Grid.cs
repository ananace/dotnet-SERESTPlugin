using System;
using System.Collections.Generic;
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
        var api = server.RegisterAPI("grid");

        var local = api.RegisterSubAPI("local");
        RegisterGridActions(local, (ev) => { 
            var grid = FindLocalGrid();
            if (grid == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No local grid");
                return null;
            }
            return grid;
        });

        var byId = api.RegisterSubAPI("id/(?<id>[0-9]+)");
        RegisterGridActions(byId, (ev) => {
            var grid = FindGrid(long.Parse(ev.Components["id"]));
            if (grid == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No grid found with the given ID");
                return null;
            }
            return grid;
        });

        var byName = api.RegisterSubAPI("name/(?<name>[^/]+)");
        RegisterGridActions(byName, (ev) => {
            var grid = FindGrid(ev.Components["name"]);
            if (grid == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No grids found with the given name");
                return null;
            }
            return grid;
        });

        var multiGridByName = api.RegisterSubAPI("names/(?<name>[^/]+)");
        RegisterMultiGridActions(byName, (ev) => FindGrids(ev.Components["name"]));

        api = server.RegisterAPI("block/id/(?<block_id>[0-9]+)");
        RegisterBlockActions(api, (ev) => {
            var block = FindGlobalBlock(long.Parse(ev.Components["block_id"]));
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No block found with the given ID");
                return null;
            }
            return block;
        });
    }

    void RegisterGridActions(APIRegister api, Func<HTTPEventArgs, MyCubeGrid> findMethod)
    {
        api.RegisterRequest("GET", (s, ev) => {
            ev.Handled = true;
            var grid = findMethod(ev);
            if (grid == null)
                return;

            ev.Context.Response.CloseJSON(new DataTypes.GridInformation(grid));
        });

        api.RegisterRequest("GET", "dampeners", (s, ev) => {
            ev.Handled = true;
            var grid = findMethod(ev);
            if (grid == null)
                return;

            ev.Context.Response.CloseString(grid.DampenersEnabled.ToString());
        });

        var blockApiByName = api.RegisterSubAPI("block/name/(?<block_name>[^/]+)");
        RegisterBlockActions(blockApiByName, (ev) => {
            var grid = findMethod(ev);
            if (grid == null)
                return null;
            var block = FindBlock(grid, System.Web.HttpUtility.UrlDecode(ev.Components["block_name"]));
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No block found with the given name");
                return null;
            }
            return block;
        });

        var multiBlockApiByName = api.RegisterSubAPI("blocks/name/(?<block_name>[^/]+)");
        RegisterMultiBlockActions(multiBlockApiByName, (ev) => {
            var grid = findMethod(ev);
            if (grid == null)
                return null;
            return FindBlocks(grid, System.Web.HttpUtility.UrlDecode(ev.Components["block_name"]));
        });

        var multiBlockApiByGroup = api.RegisterSubAPI("blocks/group/(?<block_group>[^/]+)");
        RegisterMultiBlockActions(multiBlockApiByGroup, (ev) => {
            var grid = findMethod(ev);
            if (grid == null)
                return null;
            return FindBlocksByGroup(grid, System.Web.HttpUtility.UrlDecode(ev.Components["block_group"]));
        });

        var blockApiById = api.RegisterSubAPI("block/id/(?<block_id>[0-9]+)");
        RegisterBlockActions(blockApiById, (ev) => {
            var grid = findMethod(ev);
            if (grid == null)
                return null;
            var block = FindBlock(grid, long.Parse(ev.Components["block_id"]));
            if (block == null)
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "No block found with the given ID");
                return null;
            }
            return block;
        });
    }

    void RegisterMultiGridActions(APIRegister api, Func<HTTPEventArgs, IEnumerable<MyCubeGrid>> findMethod)
    {
        api.RegisterRequest("GET", (s, ev) => {
            ev.Handled = true;
            var grids = findMethod(ev);
            ev.Context.Response.CloseJSON(grids.Select(g => new DataTypes.GridInformation(g)));
        });

        var multiBlockApiByName = api.RegisterSubAPI("blocks/name/(?<block_name>[^/]+)");
        RegisterMultiBlockActions(multiBlockApiByName, (ev) => {
            var grids = findMethod(ev);
            return grids.SelectMany(g => FindBlocks(g, System.Web.HttpUtility.UrlDecode(ev.Components["block_name"])));
        });
    }

    void RegisterBlockActions(APIRegister blockApi, Func<HTTPEventArgs, IMyTerminalBlock> findMethod)
    {
        blockApi.RegisterRequest("GET", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;

            ev.Context.Response.CloseJSON(new DataTypes.BlockInformation(block));
        });

        blockApi.RegisterRequest("GET", "enabled", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;
            if (!(block is IMyFunctionalBlock functionalBlock))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Not a functional block");
                return;
            }

            ev.Context.Response.CloseString(functionalBlock.Enabled.ToString());
        });
        blockApi.RegisterRequest("POST", "enabled", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;
            if (!(block is IMyFunctionalBlock functionalBlock))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Not a functional block");
                return;
            }

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

            functionalBlock.Enabled = wanted;
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
        blockApi.RegisterRequest("DELETE", "enabled", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;
            if (!(block is IMyFunctionalBlock functionalBlock))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Not a functional block");
                return;
            }

            functionalBlock.Enabled = false;
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });

        blockApi.RegisterRequest("GET", "name", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;

            ev.Context.Response.CloseString(block.CustomName);
        });
        blockApi.RegisterRequest("POST", "name", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;

            if (!ev.Context.Request.TryReadObject(out string text))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide a new name");
                return;
            }

            block.CustomName = text;

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });

        blockApi.RegisterRequest("GET", "text", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;

            if (!(block is IMyProgrammableBlock || block is IMyTextSurface))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Block contains no text");
                return;
            }

            if (block is IMyProgrammableBlock pb)
                ev.Context.Response.CloseString(pb.ProgramData);
            else if (block is IMyTextSurface panel)
                ev.Context.Response.CloseString(panel.GetText());
        });
        blockApi.RegisterRequest("POST", "text", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;
            if (!(block is IMyProgrammableBlock || block is IMyTextSurface))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Block contains no text");
                return;
            }

            if (!ev.Context.Request.TryReadObject(out string text))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide new text");
                return;
            }

            if (block is IMyProgrammableBlock pb)
                pb.ProgramData = text;
            else if (block is IMyTextSurface panel)
                panel.WriteText(text);

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });

        blockApi.RegisterRequest("GET", "data", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;

            ev.Context.Response.CloseString(block.CustomData);
        });
        blockApi.RegisterRequest("POST", "data", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;

            if (!ev.Context.Request.TryReadObject(out string text))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide the new data");
                return;
            }

            block.CustomData = text;
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });

        blockApi.RegisterRequest("GET", "light", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;

            if (!(findMethod(ev) is IMyLightingBlock lightBlock))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Not a light block");
                return;
            }

            ev.Context.Response.CloseJSON(new DataTypes.LightBlock(lightBlock));
        });
        blockApi.RegisterRequest("POST", "light", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;
            if (!(block is IMyLightingBlock lightBlock))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Not a light block");
                return;
            }

            var settings = ev.Context.Request.ReadJSON<DataTypes.LightBlock>();

            if (settings.BlinkIntervalSeconds.HasValue)
                lightBlock.BlinkIntervalSeconds = settings.BlinkIntervalSeconds.Value;
            if (settings.BlinkLength.HasValue)
                lightBlock.BlinkLength = settings.BlinkLength.Value;
            if (settings.BlinkOffset.HasValue)
                lightBlock.BlinkOffset = settings.BlinkOffset.Value;
            if (settings.Color != null)
                lightBlock.Color = settings.Color.ToColor();
            if (settings.Falloff.HasValue)
                lightBlock.Falloff = settings.Falloff.Value;
            if (settings.Intensity.HasValue)
                lightBlock.Intensity = settings.Intensity.Value;
            if (settings.Radius.HasValue)
                lightBlock.Radius = settings.Radius.Value;

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });

        blockApi.RegisterRequest("GET", "thruster", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;
            if (!(block is IMyThrust thrustBlock))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Not a thruster block");
                return;
            }

            ev.Context.Response.CloseJSON(new DataTypes.ThrustBlock(thrustBlock));
        });
        blockApi.RegisterRequest("POST", "thruster", (s, ev) => {
            ev.Handled = true;
            var block = findMethod(ev);
            if (block == null)
                return;
            if (!(block is IMyThrust thrustBlock))

            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.NotFound, "Not a thruster block");
                return;
            }

            var settings = ev.Context.Request.ReadJSON<DataTypes.ThrustBlock>();

            if (settings.Override.HasValue)
                thrustBlock.ThrustOverride = settings.Override.Value;
            if (settings.OverridePercentage.HasValue)
                thrustBlock.ThrustOverridePercentage = settings.OverridePercentage.Value;

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
    }

    void RegisterMultiBlockActions(APIRegister blockApi, Func<HTTPEventArgs, IEnumerable<IMyTerminalBlock>> findMethod)
    {
        blockApi.RegisterRequest("GET", (s, ev) => {
            ev.Handled = true;
            var blocks = findMethod(ev);
            ev.Context.Response.CloseJSON(blocks.Select(b => new DataTypes.BlockInformation(b)));
        });
        blockApi.RegisterRequest("POST", "enabled", (s, ev) => {
            ev.Handled = true;
            var blocks = findMethod(ev);

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

            foreach (var functionalBlock in blocks.Select(b => b as IMyFunctionalBlock).Where(b => b != null))
                functionalBlock.Enabled = wanted;

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
        blockApi.RegisterRequest("DELETE", "enabled", (s, ev) => {
            ev.Handled = true;
            var blocks = findMethod(ev);
            foreach (var functionalBlock in blocks.Select(b => b as IMyFunctionalBlock).Where(b => b != null))
                functionalBlock.Enabled = false;
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
        blockApi.RegisterRequest("POST", "name", (s, ev) => {
            ev.Handled = true;
            var blocks = findMethod(ev);
            if (!ev.Context.Request.TryReadObject(out string text))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide a new name");
                return;
            }
            foreach (var terminalBlock in blocks)
                terminalBlock.CustomName = text;
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
        blockApi.RegisterRequest("POST", "data", (s, ev) => {
            ev.Handled = true;
            var blocks = findMethod(ev);
            if (!ev.Context.Request.TryReadObject(out string text))
            {
                ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.BadRequest, "Need to provide new data");
                return;
            }
            foreach (var terminalBlock in blocks)
                terminalBlock.CustomData = text;
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
        blockApi.RegisterRequest("POST", "light", (s, ev) => {
            ev.Handled = true;
            var blocks = findMethod(ev);

            var settings = ev.Context.Request.ReadJSON<DataTypes.LightBlock>();

            foreach (var lightBlock in blocks.Select(b => b as IMyLightingBlock).Where(b => b != null))
            {
                if (settings.BlinkIntervalSeconds.HasValue)
                    lightBlock.BlinkIntervalSeconds = settings.BlinkIntervalSeconds.Value;
                if (settings.BlinkLength.HasValue)
                    lightBlock.BlinkLength = settings.BlinkLength.Value;
                if (settings.BlinkOffset.HasValue)
                    lightBlock.BlinkOffset = settings.BlinkOffset.Value;
                if (settings.Color != null)
                    lightBlock.Color = settings.Color.ToColor();
                if (settings.Falloff.HasValue)
                    lightBlock.Falloff = settings.Falloff.Value;
                if (settings.Intensity.HasValue)
                    lightBlock.Intensity = settings.Intensity.Value;
                if (settings.Radius.HasValue)
                    lightBlock.Radius = settings.Radius.Value;
            }

            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
        blockApi.RegisterRequest("POST", "thruster", (s, ev) => {
            ev.Handled = true;
            var blocks = findMethod(ev);

            var settings = ev.Context.Request.ReadJSON<DataTypes.ThrustBlock>();

            foreach (var thrustBlock in blocks.Select(b => b as IMyThrust).Where(b => b != null))
            {
                if (settings.Override.HasValue)
                    thrustBlock.ThrustOverride = settings.Override.Value;
                if (settings.OverridePercentage.HasValue)
                    thrustBlock.ThrustOverridePercentage = settings.OverridePercentage.Value;
            }

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
    IEnumerable<MyCubeGrid> FindGrids(string name)
    {
        name = System.Web.HttpUtility.UrlDecode(name);
        return MyEntities.GetEntities().OfType<MyCubeGrid>().Where(x => x.DisplayName == name);
    }

    MyCubeGrid FindGrid(long id)
    {
        return MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.EntityId == id);
    }

    IMyTerminalBlock FindBlock(MyCubeGrid grid, string name)
    {
        return grid.GetFatBlocks().Select(b => b as IMyTerminalBlock).Where(b => b != null).FirstOrDefault(b => b.CustomName == name);
    }
    IEnumerable<IMyTerminalBlock> FindBlocks(MyCubeGrid grid, string name)
    {
        return grid.GetFatBlocks().Where(b => b is IMyTerminalBlock && (b as IMyTerminalBlock).CustomName == name).Select(b => b as IMyTerminalBlock);
    }
    IEnumerable<IMyTerminalBlock> FindBlocksByGroup(MyCubeGrid grid, string groupName)
    {
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
        foreach (var group in grid.GridSystems.TerminalSystem.BlockGroups.Where(g => g.Name.ToString() == groupName))
            (group as IMyBlockGroup).GetBlocks(blocks);
        return blocks;
    }
    IMyTerminalBlock FindBlock(MyCubeGrid grid, long id)
    {
        return grid.GetFatBlocks().FirstOrDefault(b => b.EntityId == id) as IMyTerminalBlock;
    }

    IMyTerminalBlock FindGlobalBlock(long id)
    {
        return MyEntities.GetEntityById(id) as IMyTerminalBlock;
    }
}

}