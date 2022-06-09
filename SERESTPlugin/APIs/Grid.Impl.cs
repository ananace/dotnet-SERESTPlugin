using SERESTPlugin.Attributes;
using SERESTPlugin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;

namespace SERESTPlugin.APIs
{

[API("/r0/grid", OnLocal = false)]
public class GlobalGridAPI : BaseAPI
{
    [APIEndpoint("GET", "/")]
    public IEnumerable<DataTypes.GridInformation> GetGrids()
    {
        return MyEntities.GetEntities().OfType<MyCubeGrid>().Select(g => new DataTypes.GridInformation(g));
    }
    [APIEndpoint("POST", "/")]
    [APIEndpoint("PUT", "/")]
    [APIEndpoint("DELETE", "/")]
    public void TrySubmitGrid()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }
}

[API("/r0/grid/local", Needs = new string[] { "grid" }, OnDedicated = false)]
public class LocalGridAPI : R0GridAPI
{
    [APIData("grid", Description = "A local grid")]
    public override MyCubeGrid FindGrid()
    {
        var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
        if (grid == null)
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
        return grid;
    }
    [APIData("canCommunicate")]
    [Hidden]
    public override bool CanCommunicate() { return true; }

    [API("/block/id/(?<block_id>[0-9]+)", Needs = new string[] { "grid", "block" }, OnDedicated = false)]
    public class BlockAPIByID : R0BlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate() { return true; }

        [APIData("block", Description = "A valid block ID (numerical)")]
        public override IMyTerminalBlock FindBlock()
        {
            if (!long.TryParse(EventArgs.Components["block_id"], out long blockId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid block ID specified");
            if (!((Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlockWithId(blockId) is IMyTerminalBlock block))
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block found with the given ID");
            return block;
        }
    }

    [API("/block/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "block" }, OnDedicated = false)]
    public class BlockAPIByName : R0BlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate() { return true; }

        [APIData("block", Description = "A valid block name")]
        public override IMyTerminalBlock FindBlock()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            if (!((Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlockWithName(name) is IMyTerminalBlock block))
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block with specified name found");
            return block;
        }
    }

    [API("/blocks/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "blocks" }, OnDedicated = false)]
    public class MultiBlockAPIByName : R0MultiBlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate() { return true; }

        [APIData("blocks", Description = "A valid name matching multiple blocks")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            (Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).SearchBlocksOfName(name, blocks);
            return blocks;
        }
    }

    [API("/blocks/group/(?<group_name>[^/]+)", Needs = new string[] { "grid", "blocks" }, OnDedicated = false)]
    public class MultiBlockAPIByGroup : R0MultiBlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate() { return true; }

        [APIData("blocks", Description = "A valid block group name")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["group_name"]);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            (Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlockGroupWithName(name).GetBlocks(blocks);
            return blocks;
        }
    }

    [API("/blocks/all", Needs = new string[] { "grid", "blocks" }, OnDedicated = false)]
    public class MultiBlockAPIAll : R0MultiBlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate() { return true; }

        [APIData("blocks")]
        [Hidden]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            (Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlocks(blocks);
            return blocks;
        }
    }
}

[API("/r0/grid/id/(?<grid_id>[0-9]+)", Needs = new string[] { "grid", "canCommunicate" })]
public class GridByIDAPI : R0GridAPI
{
    [APIData("grid", Description = "A valid grid ID (numerical)")]
    public override MyCubeGrid FindGrid()
    {
        if (!long.TryParse(EventArgs.Components["grid_id"], out long gridId))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid grid ID specified");
        var entity = MyEntities.GetEntityById(gridId);
        if (entity == null)
            throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified ID found");
        if (!(entity is MyCubeGrid grid))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Entity with given ID is not a grid");

        return grid;
    }

    [APIData("canCommunicate", Description = "A method of communication with the target grid")]
    public override bool CanCommunicate()
    {
        return base.CanCommunicate();
    }

    [API("/block/id/(?<block_id>[0-9]+)", Needs = new string[] { "grid", "block", "canCommunicate" })]
    public class BlockAPIByID : R0BlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            if (!long.TryParse(EventArgs.Components["grid_id"], out long gridId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid grid ID specified");
            var entity = MyEntities.GetEntityById(gridId);
            if (entity == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified ID found");
            if (!(entity is MyCubeGrid grid))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Entity with given ID is not a grid");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("block", Description = "A valid block ID (numerical)")]
        public override IMyTerminalBlock FindBlock()
        {
            if (!long.TryParse(EventArgs.Components["block_id"], out long blockId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid block ID specified");
            if (!((Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlockWithId(blockId) is IMyTerminalBlock block))
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block found with the given ID");
            return block;
        }
    }

    [API("/block/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "block", "canCommunicate" })]
    public class BlockAPIByName : R0BlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            if (!long.TryParse(EventArgs.Components["grid_id"], out long gridId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid grid ID specified");
            var entity = MyEntities.GetEntityById(gridId);
            if (entity == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified ID found");
            if (!(entity is MyCubeGrid grid))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Entity with given ID is not a grid");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("block", Description = "A valid block name")]
        public override IMyTerminalBlock FindBlock()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            if (!((Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlockWithName(name) is IMyTerminalBlock block))
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block with specified name found");
            return block;
        }
    }

    [API("/blocks/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIByName : R0MultiBlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            if (!long.TryParse(EventArgs.Components["grid_id"], out long gridId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid grid ID specified");
            var entity = MyEntities.GetEntityById(gridId);
            if (entity == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified ID found");
            if (!(entity is MyCubeGrid grid))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Entity with given ID is not a grid");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks", Description = "A valid name matching multiple blocks")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            (Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).SearchBlocksOfName(name, blocks);
            return blocks;
        }
    }

    [API("/blocks/group/(?<group_name>[^/]+)", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIByGroup : R0MultiBlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            if (!long.TryParse(EventArgs.Components["grid_id"], out long gridId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid grid ID specified");
            var entity = MyEntities.GetEntityById(gridId);
            if (entity == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified ID found");
            if (!(entity is MyCubeGrid grid))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Entity with given ID is not a grid");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks", Description = "A valid block group name")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["group_name"]);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            (Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlockGroupWithName(name).GetBlocks(blocks);
            return blocks;
        }
    }

    [API("/blocks/all", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIAll : R0MultiBlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            if (!long.TryParse(EventArgs.Components["grid_id"], out long gridId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid grid ID specified");
            var entity = MyEntities.GetEntityById(gridId);
            if (entity == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified ID found");
            if (!(entity is MyCubeGrid grid))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Entity with given ID is not a grid");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks")]
        [Hidden]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            (Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlocks(blocks);
            return blocks;
        }
    }
}

[API("/r0/grid/name/(?<grid_name>[^/]+)", Needs = new string[] { "grid", "canCommunicate" })]
public class GridByNameAPI : R0GridAPI
{
    [APIData("grid", Description = "A valid grid by name")]
    public override MyCubeGrid FindGrid()
    {
        var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["grid_name"]);
        var grid = MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.DisplayName == name);
        if (grid == null)
            throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified name found");

        return grid;
    }

    [APIData("canCommunicate", Description = "A valid method of communicating with the grid")]
    public override bool CanCommunicate()
    {
        return base.CanCommunicate();
    }

    [API("/block/id/(?<block_id>[0-9]+)", Needs = new string[] { "grid", "block", "canCommunicate" })]
    public class BlockAPIByID : R0BlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["grid_name"]);
            var grid = MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.DisplayName == name);
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified name found");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("block", Description = "A valid block ID (numerical)")]
        public override IMyTerminalBlock FindBlock()
        {
            if (!long.TryParse(EventArgs.Components["block_id"], out long blockId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid block ID specified");
            if (!((Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlockWithId(blockId) is IMyTerminalBlock block))
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block found with the given ID");
            return block;
        }
    }

    [API("/block/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "block", "canCommunicate" })]
    public class BlockAPIByName : R0BlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["grid_name"]);
            var grid = MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.DisplayName == name);
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified name found");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("block", Description = "A valid block name")]
        public override IMyTerminalBlock FindBlock()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            if (!((Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlockWithName(name) is IMyTerminalBlock block))
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block with specified name found");
            return block;
        }
    }

    [API("/blocks/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIByName : R0MultiBlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["grid_name"]);
            var grid = MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.DisplayName == name);
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified name found");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks", Description = "A valid name for multiple blocks")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            (Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).SearchBlocksOfName(name, blocks);
            return blocks;
        }
    }

    [API("/blocks/group/(?<group_name>[^/]+)", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIByGroup : R0MultiBlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["grid_name"]);
            var grid = MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.DisplayName == name);
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified name found");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks", Description = "A valid block group name")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["group_name"]);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            (Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlockGroupWithName(name).GetBlocks(blocks);
            return blocks;
        }
    }

    [API("/blocks/all", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIAll : R0MultiBlockAPI
    {
        [APIData("grid")]
        [Hidden]
        public MyCubeGrid FindGrid()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["grid_name"]);
            var grid = MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.DisplayName == name);
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified name found");

            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        [Hidden]
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks")]
        [Hidden]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            (Grid.GridSystems.TerminalSystem as IMyGridTerminalSystem).GetBlocks(blocks);
            return blocks;
        }
    }
}

[API("/r0/block/id/(?<block_id>[0-9]+)", Needs = new string[] { "block", "canCommunicate" })]
public class BlockAPIByID : R0BlockAPI
{
    [APIData("canCommunicate", Description = "A valid method to communicate with the block")]
    public override bool CanCommunicate()
    {
        return base.CanCommunicate();
    }

    [APIData("block", Description = "A valid block ID (numerical)")]
    public override IMyTerminalBlock FindBlock()
    {
        if (!long.TryParse(EventArgs.Components["block_id"], out long blockId))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid block ID specified");
        var entity = MyEntities.GetEntityById(blockId);
        if (entity == null)
            throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No entity with specified ID found");
        if (!(entity is IMyTerminalBlock block))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Entity with given ID is not a block");
        return block;
    }
}

}
