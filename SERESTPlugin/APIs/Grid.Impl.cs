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
    [APIEndpoint("GET", "/")
    public IEnumerable<DataTypes.GridInformation> GetGrids()
    {
        return MyEntities.GetEntities().OfType<MyCubeGrid>().Select(g => new DataTypes.GridInformation(g));
    }
    [APIEndpoint("POST", "/")]
    public void TrySubmitGrid()
    {
        throw HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }
}

[API("/r0/grid/local", Needs = new string[] { "grid" }, OnDedicated = false)]
public class LocalGridAPI : R0GridAPI
{
    [APIData("grid")]
    public override MyCubeGrid FindGrid()
    {
        var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
        if (grid == null)
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
        return grid;
    }
    [APIData("canCommunicate")]
    public override bool CanCommunicate() { return true; }

    [API("/block/id/(?<block_id>[0-9]+)", Needs = new string[] { "grid", "block" }, OnDedicated = false)]
    public class BlockAPIByID : R0BlockAPI
    {
        [APIData("grid")]
        public MyCubeGrid FindGrid()
        {
            var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        public override bool CanCommunicate() { return true; }

        [APIData("block")]
        public override IMyTerminalBlock FindBlock()
        {
            if (!long.TryParse(EventArgs.Components["block_id"], out long blockId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid block ID specified");
            var block = Grid.GetFatBlocks().OfType<IMyTerminalBlock>().FirstOrDefault(b => b.EntityId == blockId);
            if (block == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block found with the given ID");
            return block;
        }
    }

    [API("/block/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "block" }, OnDedicated = false)]
    public class BlockAPIByName : R0BlockAPI
    {
        [APIData("grid")]
        public MyCubeGrid FindGrid()
        {
            var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        public override bool CanCommunicate() { return true; }

        [APIData("block")]
        public override IMyTerminalBlock FindBlock()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            var block = Grid.GetFatBlocks().OfType<IMyTerminalBlock>().FirstOrDefault(b => b.CustomName == name);
            if (block == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block with specified name found");
            return block;
        }
    }

    [API("/blocks/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "blocks" }, OnDedicated = false)]
    public class MultiBlockAPIByName : R0MultiBlockAPI
    {
        [APIData("grid")]
        public MyCubeGrid FindGrid()
        {
            var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        public override bool CanCommunicate() { return true; }

        [APIData("blocks")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            return Grid.GetFatBlocks().OfType<IMyTerminalBlock>().Where(b => b.CustomName == name);
        }
    }

    [API("/blocks/group/(?<group_name>[^/]+)", Needs = new string[] { "grid", "blocks" }, OnDedicated = false)]
    public class MultiBlockAPIByGroup : R0MultiBlockAPI
    {
        [APIData("grid")]
        public MyCubeGrid FindGrid()
        {
            var grid = (Sandbox.Game.World.MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit)?.CubeGrid;
            if (grid == null)
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Local player is not on a grid");
            return grid;
        }
        public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

        [APIData("canCommunicate")]
        public override bool CanCommunicate() { return true; }

        [APIData("blocks")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["group_name"]);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            foreach (var group in Grid.GridSystems.TerminalSystem.BlockGroups.Where(g => g.Name.ToString() == name))
                (group as IMyBlockGroup).GetBlocks(blocks);
            return blocks;
        }
    }
}

[API("/r0/grid/id/(?<grid_id>[0-9]+)", Needs = new string[] { "grid", "canCommunicate" })]
public class GridByIDAPI : R0GridAPI
{
    [APIData("grid")]
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

    [APIData("canCommunicate")]
    public override bool CanCommunicate()
    {
        return base.CanCommunicate();
    }

    [API("/block/id/(?<block_id>[0-9]+)", Needs = new string[] { "grid", "block", "canCommunicate" })]
    public class BlockAPIByID : R0BlockAPI
    {
        [APIData("grid")]
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
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("block")]
        public override IMyTerminalBlock FindBlock()
        {
            if (!long.TryParse(EventArgs.Components["block_id"], out long blockId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid block ID specified");
            var block = Grid.GetFatBlocks().OfType<IMyTerminalBlock>().FirstOrDefault(b => b.EntityId == blockId);
            if (block == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block found with the given ID");
            return block;
        }
    }

    [API("/block/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "block", "canCommunicate" })]
    public class BlockAPIByName : R0BlockAPI
    {
        [APIData("grid")]
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
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("block")]
        public override IMyTerminalBlock FindBlock()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            var block = Grid.GetFatBlocks().OfType<IMyTerminalBlock>().FirstOrDefault(b => b.CustomName == name);
            if (block == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block with specified name found");
            return block;
        }
    }

    [API("/blocks/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIByName : R0MultiBlockAPI
    {
        [APIData("grid")]
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
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            return Grid.GetFatBlocks().OfType<IMyTerminalBlock>().Where(b => b.CustomName == name);
        }
    }

    [API("/blocks/group/(?<group_name>[^/]+)", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIByGroup : R0MultiBlockAPI
    {
        [APIData("grid")]
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
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["group_name"]);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            foreach (var group in Grid.GridSystems.TerminalSystem.BlockGroups.Where(g => g.Name.ToString() == name))
                (group as IMyBlockGroup).GetBlocks(blocks);
            return blocks;
        }
    }
}

[API("/r0/grid/name/(?<grid_name>[^/]+)", Needs = new string[] { "grid", "canCommunicate" })]
public class GridByNameAPI : R0GridAPI
{
    [APIData("grid")]
    public override MyCubeGrid FindGrid()
    {
        var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["grid_name"]);
        var grid = MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault(x => x.DisplayName == name);
        if (grid == null)
            throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No grid with specified name found");

        return grid;
    }

    [APIData("canCommunicate")]
    public override bool CanCommunicate()
    {
        return base.CanCommunicate();
    }

    [API("/block/id/(?<block_id>[0-9]+)", Needs = new string[] { "grid", "block", "canCommunicate" })]
    public class BlockAPIByID : R0BlockAPI
    {
        [APIData("grid")]
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
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("block")]
        public override IMyTerminalBlock FindBlock()
        {
            if (!long.TryParse(EventArgs.Components["block_id"], out long blockId))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid block ID specified");
            var block = Grid.GetFatBlocks().OfType<IMyTerminalBlock>().FirstOrDefault(b => b.EntityId == blockId);
            if (block == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block found with the given ID");
            return block;
        }
    }

    [API("/block/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "block", "canCommunicate" })]
    public class BlockAPIByName : R0BlockAPI
    {
        [APIData("grid")]
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
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("block")]
        public override IMyTerminalBlock FindBlock()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            var block = Grid.GetFatBlocks().OfType<IMyTerminalBlock>().FirstOrDefault(b => b.CustomName == name);
            if (block == null)
                throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block with specified name found");
            return block;
        }
    }

    [API("/blocks/name/(?<block_name>[^/]+)", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIByName : R0MultiBlockAPI
    {
        [APIData("grid")]
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
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["block_name"]);
            return Grid.GetFatBlocks().OfType<IMyTerminalBlock>().Where(b => b.CustomName == name);
        }
    }

    [API("/blocks/group/(?<group_name>[^/]+)", Needs = new string[] { "grid", "blocks", "canCommunicate" })]
    public class MultiBlockAPIByGroup : R0MultiBlockAPI
    {
        [APIData("grid")]
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
        public override bool CanCommunicate()
        {
            return base.CanCommunicate();
        }

        [APIData("blocks")]
        public override IEnumerable<IMyTerminalBlock> FindBlocks()
        {
            var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["group_name"]);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            foreach (var group in Grid.GridSystems.TerminalSystem.BlockGroups.Where(g => g.Name.ToString() == name))
                (group as IMyBlockGroup).GetBlocks(blocks);
            return blocks;
        }
    }
}

[API("/r0/block/id/(?<block_id>[0-9]+)", Needs = new string[] { "block", "canCommunicate" })]
public class BlockAPIByID : R0BlockAPI
{
    [APIData("canCommunicate")]
    public override bool CanCommunicate()
    {
        return base.CanCommunicate();
    }

    [APIData("block")]
    public override IMyTerminalBlock FindBlock()
    {
        if (!long.TryParse(EventArgs.Components["block_id"], out long blockId))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid block ID specified");
        var entity = MyEntities.GetEntityById(gridId);
        if (entity == null)
            throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No block with specified ID found");
        if (!(entity is IMyTerminalBlock block))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Entity with given ID is not a block");
        return block;
    }
}

}
